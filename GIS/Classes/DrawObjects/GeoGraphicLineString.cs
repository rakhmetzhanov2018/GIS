using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GIS.Classes.DrawObjects
{
    internal class GeoGraphicLineString : GeoGraphicObject
    {   
        public GeoGraphicLineString(List<double[]> coords)
        {
            GeoCoords = coords;
        }
        public override void CreateFigure()
        {
            CalculateGraphicCoords();

            Polyline polyline = new Polyline
            {
                Stroke = Brushes.Black,
                StrokeThickness = 4
            };

            foreach (Point point in GraphicCoords)
            {
                polyline.Points.Add(point);
            }

            Figure = polyline;
        }
        public override void Update()
        {
            if (Figure is not Polyline polyline)
            {
                return;
            }

            double offsetX = MapToCanvasTranslator.GlobalOffsetX;
            double offsetY = MapToCanvasTranslator.GlobalOffsetY;
            double scale = MapToCanvasTranslator.GlobalScale;

            var newPoints = new PointCollection();

            foreach (var point in GraphicCoords)
            {
                double newX = point.X * scale + offsetX;
                double newY = point.Y * scale + offsetY;

                newPoints.Add(new Point(newX, newY));
            }

            polyline.Points = new PointCollection(newPoints);
        }
        public new static GeoGraphicLineString Parse(JsonElement root)
        {
            var coords = root.GetProperty("coordinates");
            var points = new List<double[]>();

            foreach (var coord in coords.EnumerateArray())
            {
                var point = new double[] { coord[0].GetDouble(), coord[1].GetDouble() };
                points.Add(point);
            }

            return new GeoGraphicLineString(points);
        }
    }
}
