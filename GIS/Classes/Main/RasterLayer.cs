using GIS.Classes.Services;
using System.Windows;
using System.Windows.Controls;

namespace GIS.Classes.Main
{
    public class RasterLayer : Layer
    {
        public Image RasterImage { get; private set; }
        public override bool ShowAttributeTableButton => false;

        public RasterLayer(Image rasterImage, string name)
        {
            RasterImage = rasterImage;
            Name = name;
        }

        public override void DrawAll(Canvas mapCanvas)
        {
            if (RasterImage == null) return;

            mapCanvas.Children.Remove(RasterImage);
            mapCanvas.Children.Add(RasterImage);

            UpdateAll();
        }

        //public override void UpdateAll()
        //{
        //    if (RasterImage == null) return;

        //    Canvas.SetLeft(RasterImage, MapToCanvasTranslator.GlobalOffsetX);
        //    Canvas.SetTop(RasterImage, MapToCanvasTranslator.GlobalOffsetY);

        //    RasterImage.Width = MapToCanvasTranslator.CanvasSize.Width * MapToCanvasTranslator.GlobalScale;
        //    RasterImage.Height = MapToCanvasTranslator.CanvasSize.Height * MapToCanvasTranslator.GlobalScale;
        //}

        public override void UpdateAll()
        {
            if (RasterImage == null) return;

            double imageMinLon = Bounds.MinLon;
            double imageMaxLon = Bounds.MaxLon;
            double imageMinLat = Bounds.MinLat;
            double imageMaxLat = Bounds.MaxLat;

            var topLeft = MapToCanvasTranslator.TranslateFromGeoToCanvas(imageMinLon, imageMaxLat);
            var bottomRight = MapToCanvasTranslator.TranslateFromGeoToCanvas(imageMaxLon, imageMinLat);

            double offsetX = MapToCanvasTranslator.GlobalOffsetX;
            double offsetY = MapToCanvasTranslator.GlobalOffsetY;
            double scale = MapToCanvasTranslator.GlobalScale;

            double finalLeft = topLeft.X * scale + offsetX;
            double finalTop = topLeft.Y * scale + offsetY;
            double finalWidth = (bottomRight.X - topLeft.X) * scale;
            double finalHeight = (bottomRight.Y - topLeft.Y) * scale;

            Canvas.SetLeft(RasterImage, finalLeft);
            Canvas.SetTop(RasterImage, finalTop);

            RasterImage.Width = finalWidth;
            RasterImage.Height = finalHeight;
        }
        public void SetBounds(GeoBounds bounds)
        {
            Bounds = bounds;
        }

        public override void UpdateVisibility()
        {
            RasterImage.Visibility = IsVisible ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
