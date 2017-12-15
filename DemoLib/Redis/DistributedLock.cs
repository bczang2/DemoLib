using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DemoLib.Util;
using System.Threading;

namespace DemoLib.Redis
{
    public class DistributedLock
    {
        private static string _path = ConfigUtils.GetConfig("path", "127.0.0.1:6379");

        /// <summary>
        /// 锁超时默认时间(ms)
        /// </summary>
        public const long _defaultLockTimeout = 1000;

        /// <summary>
        /// 未获取锁重试默认时间(ms)
        /// </summary>
        public const long _defaultRetryTime = 2000;

        /// <summary>
        /// 重试等待默认时间(ms)
        /// </summary>
        public const int _defaultWaiteTime = 10;

        public string LockKey;
        public long LockTimeout = _defaultLockTimeout;
        public long RetryTime = _defaultRetryTime;
        public int WaiteTime = _defaultWaiteTime;
        public bool _isLocked;

        public DistributedLock(string lockKey)
        {
            this.LockKey = lockKey;
        }

        public DistributedLock SetLockTimeout(long lockTimeout)
        {
            this.LockTimeout = lockTimeout;
            return this;
        }

        public DistributedLock SetRetryTime(long retryTime)
        {
            this.RetryTime = retryTime;
            return this;
        }

        public DistributedLock SetWaiteTime(int waiteTime)
        {
            this.WaiteTime = waiteTime;
            return this;
        }

        public long? Lock()
        {
            long? result = 0;
            try
            {
                long curTimestamp = DateTime.Now.ToTimeStamp();
                long timeout = this.LockTimeout + curTimestamp;
                while (this.RetryTime + curTimestamp < DateTime.Now.ToTimeStamp())
                {
                    if (RedisHelper.StringSexnx(_path, this.LockKey, timeout.ToString(), TimeSpan.FromMilliseconds(this.LockTimeout)))
                    {
                        _isLocked = true;
                        return timeout;
                    }

                    string lockVal = RedisHelper.Item_GetString(_path, this.LockKey);
                    if (!string.IsNullOrWhiteSpace(lockVal) && Convert.ToInt64(lockVal) < DateTime.Now.ToTimeStamp())
                    {
                        timeout = this.LockTimeout + curTimestamp;
                        string oldlockVal = RedisHelper.StringGetSet(_path, this.LockKey, timeout.ToString());
                        if (!string.IsNullOrWhiteSpace(oldlockVal) && oldlockVal.Equals(lockVal))
                        {
                            _isLocked = true;
                            return timeout;
                        }
                    }

                    Thread.Sleep(this.WaiteTime);
                }
            }
            catch (Exception)
            {

            }

            return result;
        }

        public void UnLock(string lockValue)
        {
            if (this._isLocked)
            {
                string newLockValue = RedisHelper.Item_GetString(_path, this.LockKey);
                if (!string.IsNullOrWhiteSpace(newLockValue) && newLockValue.Equals(lockValue))
                {
                    RedisHelper.Item_Remove(_path, this.LockKey);
                    this._isLocked = false;
                }
            }
        }
    }
}
