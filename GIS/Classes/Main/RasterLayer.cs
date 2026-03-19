using GIS.Classes.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GIS.Classes.Main
{
    public class RasterLayer : Layer
    {
        private Image rasterImage;

        public RasterLayer(Image rasterImage, string name)
        {
            this.rasterImage = rasterImage;
            Name = name;
        }

        public new void DrawAll(Canvas mapCanvas)
        {
            if (rasterImage != null && !mapCanvas.Children.Contains(rasterImage))
            {
                mapCanvas.Children.Add(rasterImage);
            }

            UpdateAll();
        }

        public new void UpdateAll()
        {
            if (rasterImage == null) return;

            Canvas.SetLeft(rasterImage, MapToCanvasTranslator.GlobalOffsetX);
            Canvas.SetTop(rasterImage, MapToCanvasTranslator.GlobalOffsetY);

            rasterImage.Width = MapToCanvasTranslator.CanvasSize.Width * MapToCanvasTranslator.GlobalScale;
            rasterImage.Height = MapToCanvasTranslator.CanvasSize.Height * MapToCanvasTranslator.GlobalScale;
        }
    }
}
