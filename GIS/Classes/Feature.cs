using GIS.Classes.GraphicObjects;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Shapes;

namespace GIS.Classes.DrawObjects
{
    internal class Feature
    {
        public string Name { get; set; } = "Без названия";
        public Dictionary<String, String> props;
        public GeoGraphicObject Geometry { get; set; }
        public Feature(GeoGraphicObject geo, Dictionary<String, String> props)
        {
            Geometry = geo;
            this.props = props;
        }

        public void DrawFigure(Canvas canvas)
        {
            Geometry.Draw(canvas);
        }
        public void UpdateFigure()
        {
            Geometry.Update();
        }
        public void SetVisibility(bool isVisible)
        {
            Geometry.SetVisibility(isVisible);
        }
    }
}
