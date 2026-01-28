using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GIS.Classes.GraphicObjects
{
    internal abstract class GraphicObject
    {
        public abstract void Draw(Canvas canvas);
        public abstract void Update(double offsetX, double offsetY, double scale = 1);
    }
}
