using System.Text.Json;

namespace GIS.Classes.DrawObjects
{
    internal class GeoPolygon : GeoObject
    {
        public List<List<double[]>> Coords { get; set; } = new List<List<double[]>>();

        public GeoPolygon(List<List<double[]>> coords)
        {
            Coords = coords;
        }

        public new static GeoPolygon Parse(JsonElement root)
        {
            var polygons_coords = root.GetProperty("coordinates");
            var polygons = new List<List<double[]>>();

            foreach (var coords in polygons_coords.EnumerateArray())
            {
                var points = new List<double[]>();

                foreach (var coord in coords.EnumerateArray())
                {
                    var point = new double[] { coord[0].GetDouble(), coord[1].GetDouble() };
                    points.Add(point);
                }

                polygons.Add(points);
            }

            return new GeoPolygon(polygons);
        }
        public override void GetBounds(ref GeoBounds bounds)
        {
            foreach (var coords in Coords[0])
            {
                bounds.Update(coords[0], coords[1]);
            }
        }
    }
}
