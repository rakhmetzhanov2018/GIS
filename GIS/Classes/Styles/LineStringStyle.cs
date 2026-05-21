using GIS.Classes.DrawObjects;
using GIS.Classes.Main;
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

        public override LineStringStyle Clone()
        {
            return new LineStringStyle(this);
        }
    }
}
