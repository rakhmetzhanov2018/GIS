using GIS.Classes.DrawObjects;
using GIS.Classes.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GIS.Classes.Styles
{
    public class LineStringStyle : LayerStyle
    {
        private int strokeThickness = 4;
        public int StrokeThickness
        {
            get => strokeThickness;
            set
            {
                if (strokeThickness != value)
                {
                    strokeThickness = value;
                    OnPropertyChanged();
                }
            }
        }
        public LineStringStyle()
        {

        }
        public LineStringStyle(LineStringStyle other)
        {
            Opacity = other.Opacity;
            MainColor = other.MainColor;
            strokeThickness = other.StrokeThickness;
        }
        public override void ApplyToFeature(Feature feature)
        {
            if (feature.Geometry is GeoGraphicLineString lineString && lineString.Figure is Polyline polyline)
            {
                polyline.StrokeThickness = strokeThickness;
                polyline.Stroke = new SolidColorBrush(MainColor);
                polyline.Opacity = Opacity;
            }
        }
    }
}
