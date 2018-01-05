using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Text;

namespace DemoLib.Queue.Kafka
{
    public class KafkaProducerHelper
    {
        private static object obj = new object();
        private static Dictionary<string, Producer> _dic = new Dictionary<string, Producer>();
        private string _broker;
        private string _topic;

        /// <summary>
        /// 创建kafka生产者实例
        /// </summary>
        /// <param name="broker">broker地址</param>
        /// <param name="topic">topic</param>
        public KafkaProducerHelper(string broker, KafkaTopic topic)
        {
            if (!_dic.ContainsKey(broker))
            {
                lock (obj)
                {
                    if (!_dic.ContainsKey(broker))
                    {
                        Dictionary<string, object> config = new Dictionary<string, object>() { { "bootstrap.servers", broker } };
                        _dic.Add(broker, new Producer(config));
                    }
                }
            }
            this._broker = broker;
            this._topic = topic.ToString();
        }

        /// <summary>
        /// 发送消息
        /// 保证所有消息的顺序
        /// 消费时只能单线程消费
        /// </summary>
        /// <param name="message">消息内容</param>
        public void SendMessage(string message)
        {
            SendCore(message, Math.Abs(this._topic.GetHashCode() % KafkaConstant.KafkaSnsPartitionCount));
        }

        /// <summary>
        /// 发送消息
        /// 保证分区代码相同的消息的顺序
        /// 消费时多线程对每个分区进行消费
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="partitionCode">分区代码</param>
        public void SendMessage(string message, long partitionCode)
        {
            SendCore(message, (int)Math.Abs(partitionCode % KafkaConstant.KafkaSnsPartitionCount));
        }

        private void SendCore(string message, int partition)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            var task = KafkaProducerHelper._dic[_broker].ProduceAsync(_topic.ToString(), null, 0, 0, buffer, 0, buffer.Length, partition, false);
            task.ContinueWith(p =>
            {
                if (p.Result.Error.HasError)
                {
                    OnSendError.Invoke(p.Result);
                }
                else
                {
                    OnSendComplete.Invoke(p.Result);
                }
            });
        }

        /// <summary>
        /// 消息发送成功事件
        /// </summary>
        public event Action<Message> OnSendComplete;

        /// <summary>
        /// 消息发送失败事件
        /// </summary>
        public event Action<Message> OnSendError;
    }
}
