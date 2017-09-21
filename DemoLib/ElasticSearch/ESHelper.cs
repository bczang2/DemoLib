using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nest;
using System.Configuration;
using Elasticsearch.Net;
using DemoLib.DemoEntity;
using DemoLib.Util;

namespace elasticsearchDemo
{
    public class ESHelper
    {
        private static ElasticClient _esClient = null;
        private static object lockObj = new object();

        public static ElasticClient ESClient
        {
            get
            {
                if (_esClient == null)
                {
                    lock (lockObj)
                    {
                        if (_esClient == null)
                        {
                            string connstr = ESConstant.ESPoolConnStr;
                            if (!string.IsNullOrWhiteSpace(connstr))
                            {
                                var nodes = connstr.Split(',').Select(t => new Uri(t));
                                var connectionPool = new SniffingConnectionPool(nodes);
                                var setting = new ConnectionSettings(connectionPool).DefaultIndex(ESConstant.DefaultIndex);

                                _esClient = new ElasticClient(setting);
                            }
                        }
                    }
                }

                return _esClient;
            }
        }

        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="indexName"></param>
        /// <returns></returns>
        public static bool CreateIndex(string indexName)
        {
            bool ret = false;
            var descriptor = new CreateIndexDescriptor(indexName)
            .Settings(s => s.NumberOfShards(ESConstant.ShardsNum).NumberOfReplicas(ESConstant.ReplicasNum))
            .Mappings(p => p.Map<Student>(m => m.AutoMap().Properties(i => i.Text(t => t.Name(n => n.name).Analyzer("standard")))));

            ESClient.CreateIndex(descriptor);

            return ret;
        }

        /// <summary>
        /// 新建文档
        /// </summary>
        /// <param name="stu"></param>
        /// <param name="indexName"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static bool Index(Student stu, string indexName, string typeName)
        {
            bool ret = false;

            var temp = ESClient.Index<Student>(stu, t => t.Index(indexName).Type(typeName).Id("stu_" + stu.id));
            ret = temp.Created;

            return ret;
        }

        /// <summary>
        /// 批量新建文档
        /// </summary>
        /// <param name="posts"></param>
        /// <param name="indexName"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static bool BulkIndex(List<Student> posts, string indexName, string typeName)
        {
            var bulkRequest = new BulkRequest(indexName, typeName) { Operations = new List<IBulkOperation>() };
            var idxops = posts.Select(o => new BulkIndexOperation<Student>(o) { Id = "stu_" + o.id }).Cast<IBulkOperation>().ToList();
            bulkRequest.Operations = idxops;
            var response = ESClient.Bulk(bulkRequest);

            return response.IsValid;
        }

        /// <summary>
        /// 更新文档
        /// </summary>
        /// <param name="stu"></param>
        /// <param name="indexName"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static bool UpdateIndex(Student stu, string indexName, string typeName)
        {
            bool ret = false;

            UpdateRequest<Student, object> update = new UpdateRequest<Student, object>(indexName, typeName, stu.uid);
            update.Doc = new { Name = stu.name };
            update.Refresh = Refresh.True;
            update.RetryOnConflict = 3;

            ESClient.Update<Student, object>(update);

            return ret;
        }

        /// <summary>
        /// 批量更新文档
        /// </summary>
        /// <param name="posts"></param>
        /// <param name="indexName"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static bool BulkUpdateIndex(List<Student> posts, string indexName, string typeName)
        {
            var bulkRequest = new BulkRequest(indexName, typeName) { Operations = new List<IBulkOperation>() };
            var idxops = posts.Select(o => new BulkUpdateOperation<Student, object>(o.uid) { Doc = new { Description = o.description } }).Cast<IBulkOperation>().ToList();
            bulkRequest.Operations = idxops;
            var response = ESClient.Bulk(bulkRequest);

            return response.IsValid;
        }

        /// <summary>
        /// 删除文档
        /// </summary>
        /// <param name="stu"></param>
        /// <param name="indexName"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static bool DeleteIndex(Student stu, string indexName, string typeName)
        {
            bool ret = false;
            DeleteRequest delete = new DeleteRequest(indexName, typeName, stu.uid);
            ESClient.Delete(delete);

            return ret;
        }

        /// <summary>
        /// 批量删除文档
        /// </summary>
        /// <param name="posts"></param>
        /// <param name="indexName"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static bool BulkDeleteIndex(List<Student> posts, string indexName, string typeName)
        {
            var bulkRequest = new BulkRequest(indexName, typeName) { Operations = new List<IBulkOperation>() };
            var idxops = posts.Select(o => new BulkDeleteOperation<Student>(o.uid)).Cast<IBulkOperation>().ToList();
            bulkRequest.Operations = idxops;
            var response = ESClient.Bulk(bulkRequest);

            return response.IsValid;
        }

        /// <summary>
        /// 测试query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static List<Student> GetList(QueryEntity query)
        {
            SearchDescriptor<Student> request = new SearchDescriptor<Student>();
            List<QueryContainer> list = new List<QueryContainer>();
            if (query != null)
            {
                if (!string.IsNullOrWhiteSpace(query.Uid))
                {
                    list.Add(Query<Student>.Term(t => t.Field(f => (f.uid)).Value(query.Uid)));
                }

                if (!string.IsNullOrWhiteSpace(query.Name)) 
                {
                    list.Add(Query<Student>.Match(t => t.Field(f => f.name).Query(query.Name)));
                }

                if (query.Date != DateTime.MinValue)
                {
                    list.Add(Query<Student>.DateRange(t => t.Field(f => f.datetime).LessThan("2017-09-20T05:40:00").GreaterThan("2017-09-24T17:00:00")));
                }
            }

            request = request.Query(t => t.Bool(b => b.Should(list.ToArray())));
            var result = ESClient.Search<Student>(request);

            return null;
        }
    }
}
