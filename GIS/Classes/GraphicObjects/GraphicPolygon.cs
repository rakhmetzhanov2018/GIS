using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;

namespace GIS.Classes.GraphicObjects
{
    internal class GraphicPolygon : GraphicObject
    {
        private List<Point> mainPoints;
        private List<List<Point>> holes;

        private Polygon? mainPolygon;

        //private TranslateTransform TranslateT = new TranslateTransform();
        //private ScaleTransform ScaleT = new ScaleTransform();
        //private TransformGroup TransformGroup = new TransformGroup();

        public GraphicPolygon(List<List<Point>> allPolygons)
        {
            //TransformGroup.Children.Add(TranslateT);
            //TransformGroup.Children.Add(ScaleT); 

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

            //Path path = new Path
            //{
            //    Stroke = Brushes.Blue,
            //    StrokeThickness = 4,
            //    Fill = Brushes.LightBlue,
            //    RenderTransform = TransformGroup
            //};

            //var geoGroup = new GeometryGroup();
            //geoGroup.FillRule = FillRule.EvenOdd;

            //PathGeometry mainGeoPath = CreatePathGeometry(mainPoints);
            //geoGroup.Children.Add(mainGeoPath);

            //foreach (var holePoints in holes)
            //{
            //    geoGroup.Children.Add(CreatePathGeometry(holePoints));
            //}

            //path.Data = geoGroup;
            //canvas.Children.Add(path);

            //polygon = path;
        }

        //private PathGeometry CreatePathGeometry(List<Point> points) 
        //{
        //    PathGeometry pg = new PathGeometry();

        //    PathFigure polygon = new PathFigure
        //    {
        //        StartPoint = points[0],
        //        IsClosed = true
        //    };

        //    for (int i = 1; i < points.Count; i++)
        //    {
        //        polygon.Segments.Add(new LineSegment(points[i], true));
        //    }

        //    pg.Figures.Add(polygon);

        //    return pg;
        //}

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


            //if (polygon == null)
            //{
            //    return;
            //}

            //TranslateT.X += offsetX;
            //TranslateT.Y += offsetY;

            //ScaleT.ScaleX *= scale;
            //ScaleT.ScaleY *= scale;

            //if (scale != 1)
            //{
            //    ScaleT.CenterX = offsetX;
            //    ScaleT.CenterY = offsetY;
            //}
        }
    }
}
