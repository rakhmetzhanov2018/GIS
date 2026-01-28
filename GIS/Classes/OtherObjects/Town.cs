using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GIS.Classes.OtherObjects
{
    public class Town : MapObject
    {
        public int Population { get; set; }
        public string Status { get; set; }

        public Town(int x, int y, string name, int population, string status) : base(x, y, name)
        {
            Population = population;
            Status = status;
        }
    }
}
