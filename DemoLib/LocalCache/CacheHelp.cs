using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace DemoLib.LocalCache
{
    public class CacheHelp
    {
        private static MemoryCache cache = MemoryCache.Default;

        public static bool AddCache<T>(string key, T value, DateTime expireTime)
        {
            bool ret = false;
            if (string.IsNullOrWhiteSpace(key) || value == null)
            {
                return ret;
            }

            try
            {
                cache.Set(key, value, expireTime);
                ret = true;
            }
            catch (Exception) { }

            return ret;
        }

        public static T GetCache<T>(string key)
        {
            T ret = default(T);
            if (string.IsNullOrWhiteSpace(key))
            {
                return ret;
            }

            try
            {
                ret = (T)Convert.ChangeType(cache.Get(key), typeof(T));
            }
            catch (Exception) { }

            return ret;
        }

        public static bool RemoveCache<T>(string key, ref T item)
        {
            bool ret = false;
            if (string.IsNullOrWhiteSpace(key))
            {
                return ret;
            }

            try
            {
                item = (T)Convert.ChangeType(cache.Remove(key), typeof(T));
                ret = true;
            }
            catch (Exception) { }

            return ret;
        }

        public static bool ContainCachesItem<T>(string key)
        {
            bool ret = false;
            if (string.IsNullOrWhiteSpace(key))
            {
                return ret;
            }

            try
            {
                ret = cache.Contains(key);
            }
            catch (Exception) { }

            return ret;
        }

        public static IDictionary<string, T> GetValues<T>(IEnumerable<string> keys)
        {
            IDictionary<string, T> ret = null;
            if (keys == null || keys.Count() == 0)
            {
                return ret;
            }

            try
            {
                var temp = cache.GetValues(keys);
                if (temp != null && temp.Any())
                {
                    ret = new Dictionary<string, T>();
                    foreach (var item in temp)
                    {
                        ret.Add(item.Key, (T)Convert.ChangeType(item.Value, typeof(T)));
                    }
                }
            }
            catch (Exception) { }

            return ret;
        }
    }
}
