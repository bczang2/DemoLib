using DemoLib.Redis;
using DemoLib.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace DemoLib.ID
{
    public sealed class GenerateIDTool
    {
        private static readonly ConcurrentQueue<long> _idQueue = new ConcurrentQueue<long>();

        private const int _applySize = 20;

        private const string _lockKey = "applyid_lock_";

        private const string _maxId = "max_id_";

        private static string _path = ConfigUtils.GetConfig("path", "127.0.0.1:6379");

        private string _applyType;

        private static Timer _timer;

        private const double _facor = 0.8;

        private const int _dueTime = 5000;

        private const int _expirSec = 1;

        private static object _lock = new object();

        private static bool _isMotinor = false;

        private static bool _workFlag = false;

        private static object _workLock = new object();

        private static DistributedLock _disLock = null;

        public GenerateIDTool(string applyType)
        {
            this._applyType = applyType;
            _disLock = new DistributedLock(_lockKey + applyType);

            if (!_isMotinor)
            {
                GetTimerStart();
                _isMotinor = true;
            }
        }

        /// <summary>
        /// 设定Timer参数
        /// </summary>
        internal void GetTimerStart()
        {
            _timer = new Timer();
            // 循环间隔时间(5秒)
            _timer.Interval = _dueTime;
            // 允许Timer执行
            _timer.Enabled = true;
            // 定义回调
            _timer.Elapsed += new ElapsedEventHandler(Action);
            // 定义多次循环
            _timer.AutoReset = true;
        }

        private void Action(object sender, ElapsedEventArgs e)
        {
            if (_idQueue.Count < (int)_applySize * _facor)
            {
                ApplyId();
            }
        }

        private void ApplyId()
        {
            lock (_workLock)
            {
                if (_workFlag || (_idQueue.Count > (int)_applySize * _facor))
                {
                    return;
                }
                else
                {
                    _workFlag = true;
                }
            }

            try
            {
                string lockValue = Guid.NewGuid().ToString();
                long? timeout = null;
                if ((timeout = _disLock.Lock()) != null)
                {
                    try
                    {
                        long curMaxId = Convert.ToInt64(RedisHelper.Item_GetString(_path, _maxId + this._applyType));
                        curMaxId = curMaxId <= 0 ? 1 : curMaxId + 1;
                        for (int i = 0; i < _applySize; i++)
                        {
                            _idQueue.Enqueue(curMaxId + i);
                        }
                        RedisHelper.Item_SetString(_path, _maxId + this._applyType, (curMaxId + _applySize - 1).ToString());
                    }
                    finally
                    {
                        _disLock.UnLock(timeout.HasValue ? timeout.Value.ToString() : string.Empty);
                    }
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                lock (_workLock)
                {
                    _workFlag = false;
                }
            }
        }

        public long GetId()
        {
            long result = 0;
            if (!_idQueue.TryDequeue(out result))
            {
                lock (_lock)
                {
                    if (!_idQueue.TryDequeue(out result))
                    {
                        ApplyId();
                        while (!_idQueue.TryDequeue(out result)) { }
                    }
                }
            }
            return result;
        }
    }
}
