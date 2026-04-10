using GIS.Classes.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GIS.Classes.Main
{
    public class RasterLayer : Layer
    {
        public Image RasterImage { get; private set; }
        private new GeoBounds Bounds { get; set; }

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
        public override void UpdateAll()
        {
            if (RasterImage == null) return;

            Canvas.SetLeft(RasterImage, MapToCanvasTranslator.GlobalOffsetX);
            Canvas.SetTop(RasterImage, MapToCanvasTranslator.GlobalOffsetY);

            RasterImage.Width = MapToCanvasTranslator.CanvasSize.Width * MapToCanvasTranslator.GlobalScale;
            RasterImage.Height = MapToCanvasTranslator.CanvasSize.Height * MapToCanvasTranslator.GlobalScale;
        }
    }
}
