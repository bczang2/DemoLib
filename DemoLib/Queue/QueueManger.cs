using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;

namespace DemoLib.Queue
{
    public class QueueManger
    {
        /// <summary>
        /// 队列最大长度
        /// </summary>
        private const int _MaxQueueLen = 1000;
        /// <summary>
        /// 阻塞队列
        /// </summary>
        private static BlockingCollection<string> blockQueue = null;
        /// <summary>
        /// 消费者线程
        /// </summary>
        private static Thread thread = null;
        /// <summary>
        /// 消费者监控timer
        /// </summary>
        private static Timer timer = null;
        /// <summary>
        /// 队列读写允许超时
        /// </summary>
        private const int allowTimeoutMillsec = 1;
        private static object queueLock = new object();
        /// <summary>
        /// timer间隔
        /// </summary>
        private const int timerMoniterMillSec = 1000 * 60;
        /// <summary>
        /// 停止线程token
        /// </summary>
        private static CancellationTokenSource cts = new CancellationTokenSource();

        static QueueManger()
        {
            StartQueueTask();
        }

        private static void StartQueueTask()
        {
            if (blockQueue == null)
            {
                lock (queueLock)
                {
                    if (blockQueue == null)
                    {
                        blockQueue = new BlockingCollection<string>(_MaxQueueLen);
                    }
                }
            }
            thread = new Thread(BlockConsumer) { IsBackground = true };
            thread.Start();

            timer = new Timer(new TimerCallback(ConsumerMoniter), null, 0, 0);
        }

        public static bool Product(string value)
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

        private static void BlockConsumer()
        {
            while (!blockQueue.IsCompleted)
            {
                try
                {
                    string res = string.Empty;
                    if (blockQueue.TryTake(out res, allowTimeoutMillsec, cts.Token) && !string.IsNullOrWhiteSpace(res))
                    {
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "data.txt", true))
                        {
                            file.WriteLine("消费item：" + res);
                        }
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
            blockQueue = new BlockingCollection<string>(_MaxQueueLen);
        }

        private static void ConsumerMoniter(object obj)
        {
            if (thread == null || !thread.IsAlive)
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

            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
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
