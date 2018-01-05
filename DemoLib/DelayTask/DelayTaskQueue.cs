using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoLib.DelayTask
{
    public class DelayTaskQueue<T> where T : DelayTaskQueueParam
    {
        /// <summary>
        /// 队列
        /// </summary>
        public static List<HashSet<T>> _queue;

        /// <summary>
        /// 任务索引
        /// </summary>
        public static Dictionary<string, int> _slotDic;

        /// <summary>
        /// 当前指向的slot
        /// </summary>
        public static int _index = 0;

        /// <summary>
        /// slot数
        /// </summary>
        public const int DEFAULT_SIZE = 60;

        public static object _lock = new object();

        public DelayTaskQueue()
        {

        }

        static DelayTaskQueue()
        {
            _queue = new List<HashSet<T>>(DEFAULT_SIZE);
            for (int i = 0; i < DEFAULT_SIZE; i++)
            {
                _queue.Add(null);
            }
            _slotDic = new Dictionary<string, int>();
        }

        /// <summary>
        /// 添加任务
        /// </summary>
        /// <param name="task"></param>
        public static void Enqueue(T task)
        {
            int curIndex = _index;
            lock (_lock)
            {
                if (_slotDic.ContainsKey(task.TaskId))
                {
                    int tempIndex = _slotDic[task.TaskId];
                    var hisTasks = _queue[tempIndex];
                    if (hisTasks != null && hisTasks.Any())
                    {
                        hisTasks.RemoveWhere(t => t.TaskId == task.TaskId);
                    }
                }
                task.CycleNum = task.ExpireSec / DEFAULT_SIZE;
                var remain = task.ExpireSec % DEFAULT_SIZE;
                int actualIndex = (remain + curIndex) % DEFAULT_SIZE;
                _slotDic[task.TaskId] = actualIndex;
                var newTask = _queue[actualIndex] ?? new HashSet<T>();
                newTask.Add(task);
                _queue[actualIndex] = newTask;
            }
        }

        /// <summary>
        /// 获取任务
        /// </summary>
        /// <param name="action"></param>
        public static void Dequeue(Action<List<string>> action)
        {
            int curIndex = _index;
            var curTasks = _queue[curIndex];
            if (curTasks != null && curTasks.Any())
            {
                List<string> waitTask = curTasks.Where(t => t.CycleNum == 0).Select(t => t.TaskId).ToList();
                if (waitTask != null && waitTask.Any())
                {
                    curTasks.Where(t => t.CycleNum == 0).ToList().ForEach(t =>
                    {
                        curTasks.Remove(t);
                    });

                    Task.Run(() =>
                    {
                        action.Invoke(waitTask);
                    });
                }
                curTasks.Where(t => t.CycleNum > 0).ToList().ForEach(t =>
                {
                    t.CycleNum -= 1;
                });
            }

            _index++;
            if (_index >= DEFAULT_SIZE)
            {
                _index = 0;
            }
        }
    }
}
