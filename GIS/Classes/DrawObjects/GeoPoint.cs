using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GIS.Classes.DrawObjects
{
    internal class GeoPoint : GeoObject
    {
        public double[] Coords { get; set; } = new double[2];

        public GeoPoint(double X, double Y)
        {
            Coords[0] = X;
            Coords[1] = Y;
        }
        public new static GeoPoint Parse(JsonElement root)
        {
            var coords = root.GetProperty("coordinates");
            return new GeoPoint(coords[0].GetDouble(), coords[1].GetDouble());
        }

        public override void GetBounds(ref GeoBounds bounds)
        {
            bounds.Update(Coords[0], Coords[1]);
        }
    }
}
