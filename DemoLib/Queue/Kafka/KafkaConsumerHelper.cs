using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DemoLib.Queue.Kafka
{
    public class KafkaConsumerHelper
    {
        private static Dictionary<string, object> _config;
        private string _topic;
        private bool _isStop;
        private const int _timeout = 1000;
        private const int _commitIntervalMs = 100;

        /// <summary>
        /// 创建kafka消费者实例
        /// </summary>
        /// <param name="broker">broker地址</param>
        /// <param name="topic">主题</param>
        /// <param name="groupID">组名，需要重新消费队列时才传入</param>
        public KafkaConsumerHelper(string broker, KafkaTopic topic, string groupID = "default_group")
        {
            _config = new Dictionary<string, object>()
            {
                { "bootstrap.servers", broker },
                { "group.id", groupID },
                { "enable.auto.commit", true },
                { "auto.commit.interval.ms", _commitIntervalMs },
            };
            this._topic = topic.ToString();
        }

        /// <summary>
        /// 开始监听消息
        /// </summary>
        /// <param name="allPartitions">是否多线程消费所有分区，生产者指定了Partition，则为true，否则false</param>
        public void StartAsync(bool allPartitions)
        {
            _isStop = false;
            if (allPartitions)
            {
                Parallel.For(0, KafkaConstant.KafkaSnsPartitionCount, _partition =>
                {
                    StartCore(_partition);
                });
            }
            else
            {
                Thread thread = new Thread(() =>
                {
                    StartCore(Math.Abs(this._isStop.GetHashCode() % KafkaConstant.KafkaSnsPartitionCount));
                });
                thread.Start();
            }
        }

        /// <summary>
        /// 开始监听消息
        /// </summary>
        /// <param name="allPartitions">是否多线程消费所有分区，生产者指定了Partition，则为true，否则false</param>
        public void Start(bool allPartitions)
        {
            _isStop = false;
            if (allPartitions)
            {
                Thread[] threads = new Thread[KafkaConstant.KafkaSnsPartitionCount];
                for (int _partition = 0; _partition < threads.Length; _partition++)
                {
                    int partition = _partition;
                    threads[partition] = new Thread(() => StartCore(partition));
                    threads[partition].Start();
                };
                for (int _partition = 0; _partition < threads.Length; _partition++)
                {
                    threads[_partition].Join();
                }
            }
            else
            {
                StartCore(Math.Abs(this._isStop.GetHashCode() % KafkaConstant.KafkaSnsPartitionCount));
            }
        }

        private void StartCore(int partition)
        {
            try
            {
                using (var consumer = new Consumer(_config))
                {
                    consumer.Assign(new List<TopicPartition> { new TopicPartition(_topic, partition) });
                    while (!_isStop)
                    {
                        Message message = null;
                        if (consumer.Consume(out message, _timeout))
                        {
                            string value = Encoding.UTF8.GetString(message.Value);
                            OnReceived.Invoke(value);
                        }
                    }
                }
            }
            catch (Exception ex) { }
        }

        /// <summary>
        /// 结束监听消息
        /// </summary>
        public void Stop()
        {
            _isStop = true;
        }

        /// <summary>
        /// 获取消息事件
        /// </summary>
        public event Action<string> OnReceived;
    }
}
