using DemoLib.PB;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoLib.Util
{
    public class PBUtil<T> where T : IMessage<T>
    {
        /// <summary>
        /// PB序列化
        /// </summary>
        /// <param name="fromObj"></param>
        /// <returns></returns>
        public static byte[] Serialize(T entity)
        {        
            byte[] result = null;
            if (entity != null)
            {
                result = new byte[entity.CalculateSize()];
                using (MemoryStream ms = new MemoryStream(result))
                {
                    entity.WriteTo(ms);
                }
            }

            return result;
        }

        /// <summary>
        /// PB反序列化
        /// </summary>
        /// <param name="jsonStr"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static T Deserialize(byte[] bytes, MessageParser<T> parser)
        {
            T result = default(T);
            if (bytes != null && bytes.Length > 0)
            {
                result = parser.ParseFrom(bytes);
            }

            return result;
        }
    }
}
