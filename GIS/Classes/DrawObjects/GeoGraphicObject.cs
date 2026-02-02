using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace GIS.Classes.DrawObjects
{
    public abstract class GeoGraphicObject
    {
        public List<double[]> GeoCoords { get; set; } = new();
        public List<Point> GraphicCoords { get; set; } = new();
        public Shape? Figure { get; protected set; }

        public abstract void CreateFigure();
        public void Draw(Canvas canvas)
        {
            if (Figure != null)
            {
                canvas.Children.Remove(Figure);
            }

            CreateFigure();
            Update();

            canvas.Children.Add(Figure);
        }
        public abstract void Update();
        protected void CalculateGraphicCoords()
        {
            GraphicCoords.Clear();

            foreach (var coord in GeoCoords)
            {
                Point graphicPoint = MapToCanvasTranslator.TranslateCoords(coord[0], coord[1]);
                GraphicCoords.Add(graphicPoint);
            }
        }
        public void SetVisibility(bool isVisible)
        {
            Figure.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }
        public static GeoGraphicObject Parse(JsonElement root)
        {
            string? type = root.GetProperty("type").GetString();

            if (type == null)
            {
                throw new Exception("Geometry type is NULL");
            }

            return type switch
            {
                "Point" => GeoGraphicPoint.Parse(root),
                "LineString" => GeoGraphicLineString.Parse(root),
                "Polygon" => GeoGraphicPolygon.Parse(root),
                _ => throw new NotImplementedException()
            };
        }
        public void GetBounds(ref GeoBounds bounds)
        {
            foreach (var coords in GeoCoords)
            {
                bounds.Update(coords[0], coords[1]);
            }
        }
    }
}
