using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoLib.ID
{
    public class IDManager
    {
        private static readonly Dictionary<string, GenerateIDTool> _dic = new Dictionary<string, GenerateIDTool>();
        private static object _lock = new object();

        private static GenerateIDTool GenerateIDPool(string type)
        {
            GenerateIDTool result = null;
            if (!_dic.ContainsKey(type))
            {
                lock (_lock)
                {
                    if (!_dic.ContainsKey(type))
                    {
                        result = new GenerateIDTool(type);
                        _dic.Add(type, result);
                    }
                }
            }
            else
            {
                result = _dic[type];
            }

            return result;
        }

        public static long GetId(string type)
        {
            return GenerateIDPool(type).GetId();
        }
    }
}
