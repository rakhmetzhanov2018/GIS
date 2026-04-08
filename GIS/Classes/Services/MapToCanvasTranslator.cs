using GIS.Classes.Main;
using System.Data;
using System.Windows;

namespace GIS.Classes.Services
{
    internal static class MapToCanvasTranslator
    {
        private const int EARTH_RADIUS_CM = 637100000;
        static public GeoBounds Bounds { get; set; } = new GeoBounds();
        static public Size CanvasSize { get; set; }
        static private double Ratio { get; set; }
        static public double GlobalOffsetX { get; set; } = 0;
        static public double GlobalOffsetY { get; set; } = 0;
        static public double GlobalScale { get; set; } = 1;
        static public void CalculateRatios()
        {
            double yRatio = CanvasSize.Height / (Bounds.MaxLat - Bounds.MinLat);
            double xRatio = CanvasSize.Width / (Bounds.MaxLon - Bounds.MinLon);
            Ratio = Math.Min(yRatio, xRatio);
        }
        static public Point TranslateFromGeoToCanvas(double X, double Y)
        {
            return new Point((X - Bounds.MinLon) * Ratio, 
               CanvasSize.Height - (Y - Bounds.MinLat) * Ratio);
        }
        static public string GetScale()
        {
            double deltaLon = (Bounds.MaxLon - Bounds.MinLon) * Math.PI / 180;
            double Lat = (Bounds.MaxLat + Bounds.MinLat) / 2 * Math.PI / 180;

            double sqSinLon = Math.Pow(Math.Sin(deltaLon / 2), 2);
            double sqCosLat = Math.Pow(Math.Cos(Lat), 2);

            double d = 2 * EARTH_RADIUS_CM * 
                Math.Asin(Math.Sqrt(sqSinLon * sqCosLat));

            double pixelsPerCM = CanvasSize.Width / 96.0 * 2.54;

            return $"1:{d / pixelsPerCM / GlobalScale:f0}";
        }
        static public double[] TranslateFromCanvasToGeo(double graphicX, double graphicY)
        {
            double X = (graphicX - GlobalOffsetX) / GlobalScale;
            double Y = CanvasSize.Height - (graphicY - GlobalOffsetY) / GlobalScale;

            double lon = X / Ratio + Bounds.MinLon;
            double lat = Y / Ratio + Bounds.MinLat;

            return [lon, lat];
        }
        static public List<double[]> TranslateFromCanvasToGeo(List<Point> points)
        {
            var newPoints = new List<double[]>();

            foreach (var point in points)
            {
                newPoints.Add(TranslateFromCanvasToGeo(point.X, point.Y));
            }

            return newPoints;
        }

        static public void ResetGlobalOffsets()
        {
            GlobalOffsetX = 0;
            GlobalOffsetY = 0;
            GlobalScale = 1;
        }
    }
}
