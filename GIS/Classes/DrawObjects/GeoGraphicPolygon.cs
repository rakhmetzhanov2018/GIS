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
    internal class GeoGraphicPolygon : GeoGraphicObject
    {
        public List<List<double[]>> HolesGeoCoords = new();
        public GeoGraphicPolygon(List<List<double[]>> coords)
        {
            GeoCoords = coords.First();
            
            for (int i = 1; i < coords.Count; i++)
            {
                HolesGeoCoords.Add(coords[i]);
            }
        }

        public GeoGraphicPolygon(List<Point> coords)
        {
            foreach (Point point in coords)
            {
                GeoCoords.Add([point.X, point.Y]);
            }
        }

        public override void CreateFigure()
        {
            CalculateGraphicCoords();

            Polygon polygon = new Polygon
            {
                Stroke = Brushes.Blue,
                StrokeThickness = 4,
                Fill = Brushes.LightBlue
            };

            // Рисование главного полигона
            foreach (var point in GraphicCoords)
            {
                polygon.Points.Add(point);
            }

            // TODO: Рисование дырок

            Figure = polygon;
        }
        public override void Update()
        {
            if (Figure is not Polygon mainPolygon)
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

            mainPolygon.Points = newPoints;
        }
        
        public new static GeoGraphicPolygon Parse(JsonElement root)
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

            return new GeoGraphicPolygon(polygons);
        }
    }
}
