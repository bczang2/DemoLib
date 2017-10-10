using DemoLib.Geo;
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
        private const string conn = "127.0.0.1:6379";

        static void Main(string[] args)
        {
            Stopwatch sp = new Stopwatch();

            #region Set
            //普通set
            sp.Start();
            for (int i = 0; i < 10000; i++)
            {
                RedisHelper.Item_SetString(conn, "test_" + i, "value_" + i);
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
            RedisHelper.Item_MSet(conn, temp);
            sp.Stop();
            Console.WriteLine("batch_time:" + sp.Elapsed.TotalMilliseconds);
            //管道set
            sp.Restart();
            Dictionary<string, string> temp2 = new Dictionary<string, string>();
            for (int i = 0; i < 10000; i++)
            {
                temp2.Add("test_" + i, "value_" + i);
            }
            RedisHelper.Item_SetAsync(conn, temp2);
            sp.Stop();
            Console.WriteLine("pipeline_time:" + sp.Elapsed.TotalMilliseconds);
            #endregion

            #region Get
            //普通get
            sp.Restart();
            List<string> list1 = new List<string>();
            for (int i = 0; i < 10000; i++)
            {
                list1.Add(RedisHelper.Item_GetString(conn, "test_" + i));
            }
            sp.Stop();
            Console.WriteLine("Get_Time:" + sp.Elapsed.TotalMilliseconds);
            //批量get
            sp.Restart();
            List<string> list2 = new List<string>();
            string[] keys1 = new string[10000];
            for (int i = 0; i < 10000; i++)
            {
                keys1[i] = "test_" + i;
            }
            list2.AddRange(RedisHelper.Item_MGet(conn, keys1));
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
            list3.AddRange(RedisHelper.Item_MGet(conn, keys2));
            sp.Stop();
            Console.WriteLine("pipeline_Time:" + sp.Elapsed.TotalMilliseconds);
            #endregion

            double lng = 121.490546;
            double lat = 31.262235;

            #region GeoHash
            string geohash = GeoHash.Encode(lat, lng);
            var ret = GeoHash.Decode(geohash); 
            #endregion

            #region RedisGeo
            RedisHelper.GeoHashAdd(conn, "TestGeo", lat, lng, "1234");
            RedisHelper.GeoHashAdd(conn, "TestGeo", lat + 1, lng + 1, "1235");
            RedisHelper.GeoHashAdd(conn, "TestGeo", lat + 23, lng + 45, "1236");
            RedisHelper.GeoHashAdd(conn, "TestGeo", lat + 8, lng + 37, "1237");
            RedisHelper.GeoHashAdd(conn, "TestGeo", lat + 15, lng - 23, "1238");

            string geoHash = RedisHelper.GeoHash(conn, "TestGeo", "1234");
            var distince = RedisHelper.GeoHashDistance(conn, "TestGeo", "1234", "1235");
            var location = RedisHelper.GeoHashLocation(conn, "TestGeo", "1234");
            var list = RedisHelper.GeoHashRadius(conn, "TestGeo", lat, lng, 10000, -1, 0, 1); 
            #endregion

            Console.ReadKey();
        }
    }
}
