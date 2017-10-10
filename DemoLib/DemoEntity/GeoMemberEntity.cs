using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoLib.DemoEntity
{
    public class GeoMemberEntity
    {
        public double? Distance { get; set; }

        public long? Hash { get; set; }

        public string Mmeber { get; set; }

        public double Lat { get; set; }

        public double Lng { get; set; }

        public string GeoHash { get; set; }
    }
}
