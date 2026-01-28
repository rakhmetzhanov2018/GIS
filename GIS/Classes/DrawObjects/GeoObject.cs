using System.Text.Json;

namespace GIS.Classes.DrawObjects
{
    internal abstract class GeoObject
    {
        
        public static GeoObject Parse(JsonElement root)
        {
            string? type = root.GetProperty("type").GetString();
            
            if (type == null)
            {
                throw new Exception("Geometry type is NULL");
            }

            return type switch
            {
                "Point" => GeoPoint.Parse(root),
                "LineString" => GeoLineString.Parse(root),
                "Polygon" => GeoPolygon.Parse(root),
                _ => throw new NotImplementedException()
            };
        }
        public abstract void GetBounds(ref GeoBounds bounds);
    }
}
