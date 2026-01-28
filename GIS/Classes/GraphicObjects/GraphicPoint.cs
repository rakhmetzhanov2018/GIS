using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GIS.Classes.GraphicObjects
{
    internal class GraphicPoint : GraphicObject
    {
        private Point point;
        private Ellipse? ellipse;
        public GraphicPoint(Point point)
        {
            this.point = point;
        }
        public override void Draw(Canvas canvas)
        {
            var ellipse = new Ellipse
            {
                Width = 6,
                Height = 6,
                Fill = Brushes.Red
            };

            canvas.Children.Add(ellipse);

            Canvas.SetLeft(ellipse, point.X - 3);
            Canvas.SetTop(ellipse, point.Y - 3);

            this.ellipse = ellipse;
        }

        public override void Update(double offsetX, double offsetY, double scale = 1)
        {
            if (ellipse == null)
            {
                return;
            }

            double Xcoord = (point.X + offsetX) * scale;
            double Ycoord = (point.Y + offsetY) * scale;

            point.X = Xcoord; 
            point.Y = Ycoord;

            Canvas.SetLeft(ellipse, point.X - 3);
            Canvas.SetTop(ellipse, point.Y - 3);


        }
    }
}
