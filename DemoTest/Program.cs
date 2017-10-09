using DemoLib.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch sp = new Stopwatch();

            #region Set
            //普通set
            sp.Start();
            for (int i = 0; i < 10000; i++)
            {
                RedisHelper.Item_SetString("127.0.0.1:6379", "test_" + i, "value_" + i);
            }
            sp.Stop();
            Console.WriteLine("time:" + sp.Elapsed.TotalMilliseconds);
            //批量set
            sp.Restart();
            Dictionary<string, string> temp = new Dictionary<string, string>();
            for (int i = 0; i < 10000; i++)
            {
                temp.Add("test_" + i, "value_" + i);
            }
            RedisHelper.Item_MSet("127.0.0.1:6379", temp);
            sp.Stop();
            Console.WriteLine("batch_time:" + sp.Elapsed.TotalMilliseconds);
            //管道set
            sp.Restart();
            Dictionary<string, string> temp2 = new Dictionary<string, string>();
            for (int i = 0; i < 10000; i++)
            {
                temp2.Add("test_" + i, "value_" + i);
            }
            RedisHelper.Item_SetAsync("127.0.0.1:6379", temp2);
            sp.Stop();
            Console.WriteLine("pipeline_time:" + sp.Elapsed.TotalMilliseconds);
            #endregion

            #region Get
            //普通get
            sp.Restart();
            List<string> list1 = new List<string>();
            for (int i = 0; i < 10000; i++)
            {
                list1.Add(RedisHelper.Item_GetString("127.0.0.1:6379", "test_" + i));
            }
            sp.Stop();
            Console.WriteLine("Get_Time:" + sp.Elapsed.TotalMilliseconds);
            //批量get
            sp.Restart();
            List<string> list2 = new List<string>();
            string[] keys1=new string[10000];
            for (int i = 0; i < 10000; i++)
            {
                keys1[i] = "test_" + i;
            }
            list2.AddRange(RedisHelper.Item_MGet("127.0.0.1:6379", keys1));
            sp.Stop();
            Console.WriteLine("batch_Time:" + sp.Elapsed.TotalMilliseconds);
            //管道get
            sp.Restart();
            List<string> list3 = new List<string>();
            string[] keys2 = new string[10000];
            for (int i = 0; i < 10000; i++)
            {
                keys2[i] = "test_" + i;
            }
            list3.AddRange(RedisHelper.Item_MGet("127.0.0.1:6379", keys2));
            sp.Stop();
            Console.WriteLine("pipeline_Time:" + sp.Elapsed.TotalMilliseconds);
            #endregion

            Console.ReadKey();
        }
    }
}
