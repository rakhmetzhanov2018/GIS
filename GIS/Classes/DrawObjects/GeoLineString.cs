using System.Text.Json;

namespace GIS.Classes.DrawObjects
{
    internal class GeoLineString : GeoObject
    {
        public List<double[]> Coords { get; set; } = new List<double[]>();

        public GeoLineString(List<double[]> coords)
        {
            Coords = coords;
        }
        public new static GeoLineString Parse(JsonElement root)
        {
            var coords = root.GetProperty("coordinates");
            var points = new List<double[]>();

            foreach (var coord in coords.EnumerateArray())
            {
                var point = new double[] { coord[0].GetDouble(), coord[1].GetDouble() };
                points.Add(point);
            }

            return new GeoLineString(points);
        }
        public override void GetBounds(ref GeoBounds bounds)
        {
            foreach (var coords in Coords)
            {
                bounds.Update(coords[0], coords[1]);
            }
        }
    }
}
