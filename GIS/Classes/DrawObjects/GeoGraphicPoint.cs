using GIS.Classes.Services;
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
    internal class GeoGraphicPoint : GeoGraphicObject
    {
        public GeoGraphicPoint(double lon, double lat)
        {
            GeoCoords = new List<double[]> { new double[] { lon, lat } };
        }
        public override void CreateFigure()
        {
            CalculateGraphicCoords();

            var ellipse = new Ellipse
            {
                Width = 6,
                Height = 6,
                Fill = Brushes.Red
            };

            Figure = ellipse;
        }
        public override void Update()
        {
            if (Figure is not Ellipse ellipse)
            {
                return;
            }

            double offsetX = MapToCanvasTranslator.GlobalOffsetX;
            double offsetY = MapToCanvasTranslator.GlobalOffsetY;
            double scale = MapToCanvasTranslator.GlobalScale;

            var point = GraphicCoords.First();

            double Xcoord = point.X * scale + offsetX;
            double Ycoord = point.Y * scale + offsetY;

            Canvas.SetLeft(ellipse, Xcoord - ellipse.Width / 2);
            Canvas.SetTop(ellipse, Ycoord - ellipse.Height / 2);
        }
        
        public new static GeoGraphicPoint Parse(JsonElement coords)
        {
            return new GeoGraphicPoint(coords[0].GetDouble(), coords[1].GetDouble());
        }
    }
}
