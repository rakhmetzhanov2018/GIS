using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace GIS.Classes.OtherObjects
{
    public abstract class MapObject
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Name { get; set; } = "Без названия";

        public MapObject(int x, int y, string name)
        {
            X = x; 
            Y = y; 
            Name = name;
        }
    }
}
