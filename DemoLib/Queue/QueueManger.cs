using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using DemoLib.Util;

namespace DemoLib.Queue
{
    public class QueueManger<T>
    {
        #region 参数
        /// <summary>
        /// 队列最大长度
        /// </summary>
        private const int _MaxQueueLen = 1000;
        /// <summary>
        /// 阻塞队列
        /// </summary>
        private static BlockingCollection<T> blockQueue = new BlockingCollection<T>(_MaxQueueLen);
        /// <summary>
        /// 批量处理数据
        /// </summary>
        private static List<T> queueDataList = new List<T>(_MaxQueueLen);
        /// <summary>
        /// 消费者线程
        /// </summary>
        private static Thread thread = null;
        /// <summary>
        /// 消费者线程监控timer
        /// </summary>
        private static Timer timer = null;
        /// <summary>
        /// 批量处理数据timer
        /// </summary>
        private static Timer dataTimer = null;
        /// <summary>
        /// 队列读写允许超时
        /// </summary>
        private const int allowTimeoutMillsec = 1;
        /// <summary>
        /// 批量数据处理timer间隔(10秒)
        /// </summary>
        private const int dataTimerMillSec = 1000 * 10;
        /// <summary>
        /// 监控timer间隔(5分钟)
        /// </summary>
        private const int timerMoniterMillSec = 1000 * 60 * 5;
        /// <summary>
        /// 停止线程token
        /// </summary>
        private static CancellationTokenSource cts = new CancellationTokenSource();
        /// <summary>
        /// 批量数据处理标识
        /// </summary>
        private static bool flag = false;
        private static object processLock = new object();
        /// <summary>
        /// 内存队列容量阀值因子
        /// </summary>
        private const double factor = 0.9; 
        #endregion

        static QueueManger()
        {
            StartQueueTask();
        }

        private static void StartQueueTask()
        {
            thread = new Thread(BlockConsumer) { IsBackground = true };
            thread.Priority = ThreadPriority.Normal;
            thread.Start();

            //批量数据处理timer
            dataTimer = new Timer(new TimerCallback(BatchDataProcess), null, 0, 0);
            //监控timer
            timer = new Timer(new TimerCallback(ConsumerMoniter), null, 0, 0);
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool Product(T value)
        {
            bool ret = false;
            try
            {
                ret = blockQueue.TryAdd(value, allowTimeoutMillsec, cts.Token);
            }
            catch (OperationCanceledException)
            {
                blockQueue.CompleteAdding();
            }

            return ret;
        }

        /// <summary>
        /// 消费数据
        /// </summary>
        private static void BlockConsumer()
        {
            while (!blockQueue.IsCompleted)
            {
                try
                {
                    T res = default(T);
                    if (blockQueue.TryTake(out res, allowTimeoutMillsec, cts.Token) && res != null)
                    {
                        WriteToQueueDataList(res);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception)
                {
                    break;
                }
            }
            blockQueue = new BlockingCollection<T>(_MaxQueueLen);
        }

        /// <summary>
        /// 写入数据到内存队列
        /// </summary>
        /// <param name="res"></param>
        private static void WriteToQueueDataList(T res)
        {
            if (res != null)
            {
                queueDataList.Add(res);
            }

            if (queueDataList != null && queueDataList.Count >= queueDataList.Count * factor)
            {
                CommonDataProcess();
            }
        }

        /// <summary>
        /// 定时批量处理数据
        /// </summary>
        /// <param name="state"></param>
        private static void BatchDataProcess(object state)
        {
            CommonDataProcess();

            dataTimer = new Timer(new TimerCallback(BatchDataProcess), null, dataTimerMillSec, 0);
        }

        /// <summary>
        /// 批量处理数据
        /// </summary>
        private static void CommonDataProcess()
        {
            if (queueDataList == null || queueDataList.Count == 0)
            {
                return;
            }

            lock (processLock)
            {
                if (flag)
                {
                    return;
                }
                else
                {
                    flag = true;
                }
            }
            try
            {
                SendDataToQueue(queueDataList);
                queueDataList.Clear();
            }
            catch (Exception)
            {
                queueDataList = new List<T>(_MaxQueueLen);
            }
            finally
            {
                lock (processLock)
                {
                    flag = false;
                }
            }
        }

        /// <summary>
        /// 批量写入数据到队列
        /// </summary>
        /// <param name="queueDataList"></param>
        private static void SendDataToQueue(List<T> queueDataList)
        {
            if (queueDataList != null && queueDataList.Any())
            {
                //文件模拟消息队列
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "data.txt", true))
                {
                    file.WriteLine("消费item：" + JsonUtil<object>.Serialize(queueDataList));
                }
            }
        }

        /// <summary>
        /// 消费者线程监控
        /// </summary>
        /// <param name="obj"></param>
        private static void ConsumerMoniter(object obj)
        {
            if (thread == null || thread.ThreadState == ThreadState.Stopped || !thread.IsAlive)
            {
                //开启新的
                thread = new Thread(BlockConsumer) { IsBackground = true };
                thread.Start();
                //重置Token
                cts = new CancellationTokenSource();
            }
            else
            {
                Console.WriteLine("ThreadId:" + thread.ManagedThreadId + "\tStatus:" + thread.ThreadState.ToString());
            }

            timer = new Timer(new TimerCallback(ConsumerMoniter), null, timerMoniterMillSec, 0);
        }

        public static void StopQueueTask()
        {
            if (timer != null)
            {
                timer.Dispose();
            }

            if (dataTimer != null)
            {
                dataTimer.Dispose();
            }

            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
                cts = null;
            }
        }

        public static void RestartQueueTask()
        {
            cts = new CancellationTokenSource();
            StartQueueTask();
        }

        /// <summary>
        /// Test
        /// </summary>
        public static void StopTimer()
        {
            if (timer != null)
                timer.Dispose();
        }

        /// <summary>
        /// Test
        /// </summary>
        public static void StopThread()
        {
            cts.Cancel();
            cts.Token.Register(() =>
            {
                Console.WriteLine("后台线程stop");
            });
        }
    }
}
