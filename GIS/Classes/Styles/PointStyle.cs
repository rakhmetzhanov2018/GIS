using GIS.Classes.DrawObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GIS.Classes.Styles
{
    public class PointStyle : LayerStyle
    {
        private int size = 6;
        public int Size
        {
            get => size;
            set
            {
                if (value != size)
                {
                    size = value;
                    OnPropertyChanged();
                }
            }
        }

        public override void ApplyToFeature(Feature feature)
        {
            if (feature.Geometry is GeoGraphicPoint point && point.Figure is Ellipse ellipse)
            {
                ellipse.Width = size;
                ellipse.Height = size;
                ellipse.Fill = new SolidColorBrush(MainColor);
                ellipse.Opacity = Opacity;
            }
        }
    }
}
