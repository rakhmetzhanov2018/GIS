using GIS.Classes.DrawObjects;
using GIS.Classes.Main;
using GIS.Classes.Styles;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GIS.Services
{
    public static class AnalysisService
    {
        private static double MetersToDegrees(double meters, double centerLat)
        {
            double km = meters / 1000.0;
            double latDeg = km / 111.0; 
            double lonDeg = km / (111.0 * Math.Cos(centerLat * Math.PI / 180.0));
            return Math.Max(latDeg, lonDeg); 
        }

        public static Layer CreateBuffer(Layer layer, double radiusM)
        {
            if (layer.ObjectList.Count == 0) return null;

            string newLayerName = $"{layer.Name}_буфер_анализ_{Guid.NewGuid().ToString().Substring(0, 6)}";
            Layer bufferLayer = new Layer(newLayerName, GeometryType.Polygon);

            foreach (var feature in layer.ObjectList)
            {
                var bufferGeometries = CreateBufferGeometry(feature.Geometry, radiusM);
                foreach (var bufferGeo in bufferGeometries)
                {
                    if (bufferGeo != null)
                    {
                        var newFeature = new Feature(bufferGeo, new Dictionary<string, string>());
                        bufferLayer.AddObject(newFeature);
                    }
                }
            }

            bufferLayer.LayerStyle = new PolygonStyle
            {
                FillColor = Colors.LightGreen,
                Opacity = 0.5,
                StrokeThickness = 2,
                MainColor = Colors.DarkGreen
            };
            bufferLayer.ApplyStyleToAllFeatures();

            GeoBounds layerBounds = new GeoBounds();
            foreach (var feature in bufferLayer.ObjectList)
                feature.Geometry.GetBounds(ref layerBounds);
            bufferLayer.Bounds = layerBounds;

            return bufferLayer;
        }

        private static List<GeoGraphicObject> CreateBufferGeometry(GeoGraphicObject geo, double radiusM)
        {
            double avgLat = geo.GeoCoords.Average(c => c[1]);
            double bufferDeg = MetersToDegrees(radiusM, avgLat);

            var ntsGeometry = ConvertToNtsGeometry(geo);
            if (ntsGeometry == null) return new List<GeoGraphicObject>();

            var buffer = ntsGeometry.Buffer(bufferDeg);

            return ConvertFromNtsGeometry(buffer);
        }

        private static NetTopologySuite.Geometries.Geometry ConvertToNtsGeometry(GeoGraphicObject geo)
        {
            var factory = new GeometryFactory();
            if (geo is GeoGraphicPoint point)
            {
                var coord = new Coordinate(point.GeoCoords[0][0], point.GeoCoords[0][1]);
                return factory.CreatePoint(coord);
            }
            if (geo is GeoGraphicLineString line)
            {
                var coords = line.GeoCoords.Select(c => new Coordinate(c[0], c[1])).ToArray();
                return factory.CreateLineString(coords);
            }
            if (geo is GeoGraphicPolygon polygon)
            {
                var coords = polygon.GeoCoords.Select(c => new Coordinate(c[0], c[1])).ToArray();
                return factory.CreatePolygon(coords);
            }
            return null;
        }

        private static List<GeoGraphicObject> ConvertFromNtsGeometry(NetTopologySuite.Geometries.Geometry geom)
        {
            var result = new List<GeoGraphicObject>();

            if (geom is NetTopologySuite.Geometries.Polygon polygon)
            {
                var extRing = polygon.ExteriorRing.Coordinates
                    .Select(c => new double[] { c.X, c.Y }).ToList();
                if (extRing.Count > 1 && extRing[0][0] == extRing[extRing.Count - 1][0] &&
                    extRing[0][1] == extRing[extRing.Count - 1][1])
                    extRing.RemoveAt(extRing.Count - 1);
                var poly = new GeoGraphicPolygon(new List<List<double[]>> { extRing });
                result.Add(poly);
            }
            else if (geom is MultiPolygon multi)
            {
                for (int i = 0; i < multi.NumGeometries; i++)
                {
                    var subPoly = multi.GetGeometryN(i) as NetTopologySuite.Geometries.Polygon;
                    if (subPoly != null)
                    {
                        var extRing = subPoly.ExteriorRing.Coordinates
                            .Select(c => new double[] { c.X, c.Y }).ToList();
                        if (extRing.Count > 1 && extRing[0][0] == extRing[extRing.Count - 1][0] &&
                            extRing[0][1] == extRing[extRing.Count - 1][1])
                            extRing.RemoveAt(extRing.Count - 1);
                        var poly = new GeoGraphicPolygon(new List<List<double[]>> { extRing });
                        result.Add(poly);
                    }
                }
            }
            return result;
        }
    }
}