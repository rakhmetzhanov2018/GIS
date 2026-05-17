using GIS.Classes.DrawObjects;
using GIS.Classes.Main;
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
        public PointStyle()
        {

        }
        public PointStyle(PointStyle other)
        {
            Opacity = other.Opacity;
            MainColor = other.MainColor;
            Size = other.Size;
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
