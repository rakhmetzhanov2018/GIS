using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GIS.Classes.OtherObjects
{
    public class Road : MapObject
    {
        public string Type { get; set; }
        public string Coverage { get; set; }

        public Road(int x, int y, string name, string type, string coverage) : base(x, y, name)
        {
            Type = type;
            Coverage = coverage;
        }
    }
}
