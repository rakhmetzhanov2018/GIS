using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GIS.Classes.OtherObjects
{
    public class River : MapObject
    {
        public float Length { get; set; }
        public Boolean Navigability { get; set; }

        public River(int x, int y, string name, float length, Boolean navigability) : base(x, y, name)
        {
            Length = length;
            Navigability = navigability;
        }
    }
}
