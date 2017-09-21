using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoLib.Util
{
    public class ESConstant
    {
        /// <summary>
        /// 默认索引名
        /// </summary>
        public const string DefaultIndex = "db_student";

        /// <summary>
        /// 分片数
        /// </summary>
        public const int ShardsNum = 5;

        /// <summary>
        /// 副本数
        /// </summary>
        public const int ReplicasNum = 1;

        /// <summary>
        /// es集群地址
        /// </summary>
        public static string ESPoolConnStr = ConfigurationManager.AppSettings["ESPoolConnStr"];
    }
}
