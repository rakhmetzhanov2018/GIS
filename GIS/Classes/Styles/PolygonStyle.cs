using GIS.Classes.DrawObjects;
using GIS.Classes.Main;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GIS.Classes.Styles
{
    public class PolygonStyle : LayerStyle
    {
        private Color fillColor = Colors.Blue;
        private int strokeThickness = 4;

        public Color FillColor
        {
            get => fillColor;
            set
            {
                if (value != fillColor)
                {
                    fillColor = value;
                    OnPropertyChanged();
                }
            }
        }
        public int StrokeThickness
        {
            get => strokeThickness;
            set
            {
                if (value != strokeThickness)
                {
                    strokeThickness = value;
                    OnPropertyChanged();
                }
            }
        }
        public PolygonStyle()
        {

        }
        public PolygonStyle(PolygonStyle other)
        {
            Opacity = other.Opacity;
            MainColor = other.MainColor;
            StrokeThickness = other.StrokeThickness;
            FillColor = other.FillColor;
        }

        public override void ApplyToFeature(Feature feature)
        {
            if (feature.Geometry is GeoGraphicPolygon geoGraphicPolygon && geoGraphicPolygon.Figure is Polygon polygon)
            {
                polygon.Opacity = Opacity;
                polygon.Stroke = new SolidColorBrush(MainColor);
                polygon.StrokeThickness = StrokeThickness;
                polygon.Fill = new SolidColorBrush(FillColor);
            }
        }
    }
}
