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
        
        //public void CreateFigure(Canvas canvas)
        //{
        //    Geometry.Draw(canvas);
        //}

        //public void CreateFigure()
        //{
        //    if (Figure != null)
        //    {
        //        return;
        //    }

        //    Figure = Geometry switch
        //    {
        //        GeoPoint point => CreatePointFigure(point),
        //        GeoLineString line => CreateLineFigure(line),
        //        GeoPolygon polygon => CreatePolygonFigure(polygon),
        //        _ => throw new NotSupportedException()
        //    };
        //}
        //private GraphicPoint CreatePointFigure(GeoPoint point)
        //{
        //    Point grPoint = MapToCanvasTranslator.TranslateCoords(point.Coords[0], point.Coords[1]);
        //    return new GraphicPoint(grPoint);
        //}
        //private GraphicLineString CreateLineFigure(GeoLineString line)
        //{
        //    var grPoints = new List<Point>();

        //    foreach (var point in line.Coords)
        //    {
        //        grPoints.Add(MapToCanvasTranslator.TranslateCoords(point[0], point[1]));
        //    }

        //    return new GraphicLineString(grPoints);
        //}
        //private GraphicPolygon CreatePolygonFigure(GeoPolygon polygon)
        //{
        //    var grPoints = new List<List<Point>>();

        //    foreach (var pol in polygon.Coords)
        //    {
        //        var grPolPoints = new List<Point>();

        //        foreach (var point in pol)
        //        {
        //            grPolPoints.Add(MapToCanvasTranslator.TranslateCoords(point[0], point[1]));
        //        }

        //        grPoints.Add(grPolPoints);
        //    }

        //    return new GraphicPolygon(grPoints);
        //}

        public void DrawFigure(Canvas canvas)
        {
            Geometry.Draw(canvas);
        }
        public void UpdateFigure()
        {
            Geometry.Update();
        }
    }
}
