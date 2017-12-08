using DemoLib.Util;

namespace DemoLib.Queue.Kafka
{
    public class KafkaConstant
    {
        /// <summary>
        /// Broker
        /// </summary>
        public static string KafkaSnsConsumerBroker = ConfigUtils.GetConfig("Broker", "192.168.136.130:9092");

        /// <summary>
        /// Partition
        /// </summary>
        internal static int KafkaSnsPartitionCount = ConfigUtils.GetConfig("Partition", 3);
    }
}
