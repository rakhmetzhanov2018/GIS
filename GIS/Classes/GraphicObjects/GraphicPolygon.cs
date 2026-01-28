using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GIS.Classes.GraphicObjects
{
    internal class GraphicPolygon : GraphicObject
    {
        private List<Point> mainPoints;
        private List<List<Point>> holes;

        private Polygon? mainPolygon;

        public GraphicPolygon(List<List<Point>> allPolygons)
        {
            mainPoints = [];
            holes = [];

            bool isMainPolygon = true;

            foreach (var polygon in allPolygons)
            {
                if (isMainPolygon)
                {
                    mainPoints.AddRange(polygon);
                    isMainPolygon = false;
                }
                else
                {
                    holes.Add(polygon);
                }
            }
        }
        public override void Draw(Canvas canvas)
        {
            Polygon polygon = new Polygon
            {
                Stroke = Brushes.Blue,
                StrokeThickness = 4,
                Fill = Brushes.LightBlue
            };

            // Рисование главного полигона
            foreach (var point in mainPoints)
            {
                polygon.Points.Add(point);
            }

            // TODO: Рисование дырок

            canvas.Children.Add(polygon);

            mainPolygon = polygon;
        }

        public override void Update(double offsetX, double offsetY, double scale = 1)
        {
            if (mainPolygon == null)
            {
                return;
            }

            var points = mainPolygon.Points;
            var newPoints = new PointCollection();

            for (int i = 0; i < points.Count; i++)
            {
                var point = points[i];

                double newX = (point.X + offsetX) * scale;
                double newY = (point.Y + offsetY) * scale;

                newPoints.Add(new Point(newX, newY));

                mainPoints[i] = new Point(newX, newY);
            }

            mainPolygon.Points = new PointCollection(newPoints);
        }
    }
}
