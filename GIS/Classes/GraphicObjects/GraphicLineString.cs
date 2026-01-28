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
    internal class GraphicLineString : GraphicObject
    {
        private List<Point> linePoints;
        private Polyline? polyline;

        public GraphicLineString()
        {
            linePoints = new List<Point>();
        }

        public GraphicLineString(List<Point> line)
        {
            this.linePoints = line;
        }
        public void AddPoint(Point point)
        {
            linePoints.Add(point);
        }
        public void AddPoints(IEnumerable<Point> points)
        {
            foreach (Point point in points)
            {
                linePoints.Add(point);
            }
        }
        public override void Draw(Canvas canvas)
        {
            Polyline pl = new Polyline
            {
                Stroke = Brushes.Black,
                StrokeThickness = 4
            };

            foreach (Point point in linePoints) {
                pl.Points.Add(point);
            }

            canvas.Children.Add(pl);

            polyline = pl;
        }

        public override void Update(double offsetX, double offsetY, double scale = 1)
        {
            if (polyline == null)
            {
                return;
            }

            var points = polyline.Points;
            var newPoints = new PointCollection();

            for (int i = 0; i < points.Count; i++)
            {
                var point = points[i];

                double newX = (point.X + offsetX) * scale;
                double newY = (point.Y + offsetY) * scale;

                newPoints.Add(new Point(newX, newY));

                linePoints[i] = new Point(newX, newY);
            }

            polyline.Points = new PointCollection(newPoints);
        }
    }
}
