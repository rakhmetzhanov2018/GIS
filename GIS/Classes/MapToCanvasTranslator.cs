using System.Windows;

namespace GIS.Classes
{
    internal static class MapToCanvasTranslator
    {
        static public GeoBounds Bounds { get; set; } = new GeoBounds();
        static public Size CanvasSize { get; set; }

        static private double Ratio { get; set; }
        static public void CalculateRatios()
        {
            double yRatio = CanvasSize.Height / (Bounds.MaxLat - Bounds.MinLat);
            double xRatio = CanvasSize.Width / (Bounds.MaxLon - Bounds.MinLon);
            Ratio = Math.Min(yRatio, xRatio);
        }
        static public Point TranslateCoords(double X, double Y)
        {
            return new Point((X - Bounds.MinLon) * Ratio, CanvasSize.Height - (Y - Bounds.MinLat) * Ratio);
        }
    }
}
