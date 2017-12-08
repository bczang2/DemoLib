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
        private static Dictionary<string, object> config = null;
        private string topic = "";
        private bool isStop = false;

        /// <summary>
        /// 创建kafka消费者实例
        /// </summary>
        /// <param name="broker">broker地址</param>
        /// <param name="topic">主题（队列名）</param>
        /// <param name="groupID">组名，需要重新消费队列时才传入</param>
        public KafkaConsumerHelper(string broker, KafkaTopic topic, string groupID = "default_group")
        {
            config = new Dictionary<string, object>()
            {
                { "bootstrap.servers", broker },
                { "group.id", groupID },
                { "enable.auto.commit", true },
                { "auto.commit.interval.ms", 100 },
            };
            this.topic = topic.ToString();
        }

        /// <summary>
        /// 开始监听消息
        /// </summary>
        /// <param name="allPartitions">是否多线程监听所有分区，如果生产者指定了分区代码，则为true，否则为false</param>
        public void StartAsync(bool allPartitions)
        {
            isStop = false;
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
                    StartCore(Math.Abs(this.topic.GetHashCode() % KafkaConstant.KafkaSnsPartitionCount));
                });
                thread.Start();
            }
        }

        /// <summary>
        /// 开始监听消息
        /// </summary>
        /// <param name="allPartitions">是否多线程监听所有分区，如果生产者指定了分区代码，则为true，否则为false</param>
        public void Start(bool allPartitions)
        {
            isStop = false;
            if (allPartitions)
            {
                Thread[] threads = new Thread[KafkaConstant.KafkaSnsPartitionCount];
                for (int _partition = 0; _partition < threads.Length; _partition++)
                {
                    threads[_partition] = new Thread(() => StartCore(_partition));
                    threads[_partition].Start();
                };
                for (int _partition = 0; _partition < threads.Length; _partition++)
                {
                    threads[_partition].Join();
                }
            }
            else
            {
                StartCore(Math.Abs(this.topic.GetHashCode() % KafkaConstant.KafkaSnsPartitionCount));
            }
        }

        private void StartCore(int partition)
        {
            try
            {
                using (var consumer = new Consumer(config))
                {
                    consumer.Assign(new List<TopicPartition> { new TopicPartition(topic, partition) });
                    while (!isStop)
                    {
                        Message message = null;
                        if (consumer.Consume(out message, 5000))
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
            isStop = true;
        }

        /// <summary>
        /// 获取消息事件
        /// </summary>
        public event Action<string> OnReceived;
    }
}
