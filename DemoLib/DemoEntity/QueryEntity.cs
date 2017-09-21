using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoLib.DemoEntity
{
    public class QueryEntity
    {
        public string Uid { get; set; }

        public string Name { get; set; }

        public DateTime Date { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }
    }
}
