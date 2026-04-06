using GIS.Classes.Styles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace GIS.Classes.Factories
{
    public static class DefaultStyleFactory
    {
        public static LayerStyle CreateDefaultStyle(GeometryType type)
        {
            return type switch
            {
                GeometryType.Point => new PointStyle
                {
                    Opacity = 1.0,
                    MainColor = Colors.Red,
                    Size = 6
                },
                GeometryType.LineString => new LineStringStyle
                {
                    Opacity = 1.0,
                    MainColor = Colors.Black,
                    StrokeThickness = 4
                },
                GeometryType.Polygon => new PolygonStyle
                {
                    Opacity = 1.0,
                    MainColor = Colors.Blue,
                    FillColor = Colors.LightBlue,
                    StrokeThickness = 4
                },
                _ => throw new NotImplementedException()
            };
        }
    }
}
