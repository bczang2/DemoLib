using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoLib.DelayTask
{
    public class DelayTaskQueueParam
    {
        /// <summary>
        /// 任务id
        /// </summary>
        public string TaskId { get; set; }

        /// <summary>
        /// 任务周期数
        /// </summary>
        public int CycleNum { get; set; }

        /// <summary>
        /// 任务过期时间(s)
        /// </summary>
        public int ExpireSec { get; set; }
    }
}
