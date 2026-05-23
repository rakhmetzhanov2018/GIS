using GIS.Classes.DrawObjects;
using GIS.Classes.Factories;
using GIS.Classes.Main;
using GIS.Classes.Services;
using System.IO;
using System.Text.Json;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GIS.Services
{
    public class FileService
    {
        public Layer LoadGeoJsonFile(string filePath)
        {
            string text = File.ReadAllText(filePath);

            Layer newLayer = new()
            {
                Name = System.IO.Path.GetFileName(filePath)
            };

            ParseGeoJSON(newLayer, text);
            newLayer.AnalyzeFeatureProperties();
            newLayer.LayerStyle = DefaultStyleFactory.CreateDefaultStyle(newLayer.GeoType);

            return newLayer;
        }
        private void ParseGeoJSON(Layer layer, string geoJSON)
        {
            GeoBounds bounds = new GeoBounds();

            var doc = JsonDocument.Parse(geoJSON);
            var root = doc.RootElement;
            var type = root.GetProperty("type").GetString();

            switch (type)
            {
                case "GeometryCollection":
                    ParseGeometryCollection(layer, root, ref bounds);
                    break;

                case "FeatureCollection":
                    ParseFeatureCollection(layer, root, ref bounds);
                    break;

                case "Feature":
                    layer.AddObject(ParseFeature(root, ref bounds));
                    break;
            }

            MapToCanvasTranslator.Bounds = bounds;
            MapToCanvasTranslator.CalculateRatios();

            layer.Bounds = bounds;
        }
        private void ParseFeatureCollection(Layer layer, JsonElement root, ref GeoBounds bounds)
        {
            foreach (JsonElement feature in root.GetProperty("features").EnumerateArray())
            {
                var geo = feature.GetProperty("geometry");
                string geoType = geo.GetProperty("type").GetString();
                switch (geoType)
                {
                    case "MultiPolygon":
                        ParseMultiPolygon(layer, geo, ref bounds);
                        break;
                    case "MultiLineString":
                        ParseMultiLineString(layer, geo, ref bounds);
                        break;
                    case "MultiPoint":
                        ParseMultiPoint(layer, geo, ref bounds);
                        break;
                    default:
                        layer.AddObject(ParseFeature(feature, ref bounds));
                        break;
                }
            }
        }
        private Feature ParseFeature(JsonElement root, ref GeoBounds bounds)
        {
            GeoGraphicObject geo = GeoGraphicObject.Parse(root.GetProperty("geometry"));
            Dictionary<string, string> dict = ParseProperties(root.GetProperty("properties"));

            geo.GetBounds(ref bounds);

            return new Feature(geo, dict);
        }
        private void ParseGeometryCollection(Layer layer, JsonElement root, ref GeoBounds bounds)
        {
            foreach (JsonElement feature in root.GetProperty("geometries").EnumerateArray())
            {
                switch (feature.GetProperty("type").GetString())
                {
                    case "MultiPolygon":
                        ParseMultiPolygon(layer, feature, ref bounds);
                        break;

                    case "MultiLineString":
                        ParseMultiLineString(layer, feature, ref bounds);
                        break;

                    case "MultiPoint":
                        ParseMultiPoint(layer, feature, ref bounds);
                        break;

                    case "Point":
                    case "LineString":
                    case "Polygon":
                        layer.AddObject(ParseFeature(feature, ref bounds));
                        break;
                }
            }
        }
        private void ParseMultiPolygon(Layer layer, JsonElement root, ref GeoBounds bounds)
        {
            var coords = root.GetProperty("coordinates");

            foreach (JsonElement polygons_coords in coords.EnumerateArray())
            {
                GeoGraphicPolygon geoPolygon = GeoGraphicPolygon.Parse(polygons_coords);

                geoPolygon.GetBounds(ref bounds);
                layer.AddObject(new Feature(geoPolygon, []));
            }
        }
        private void ParseMultiLineString(Layer layer, JsonElement root, ref GeoBounds bounds)
        {
            var coords = root.GetProperty("coordinates");
            foreach (var lineCoords in coords.EnumerateArray())
            {
                var points = new List<double[]>();
                foreach (var point in lineCoords.EnumerateArray())
                {
                    points.Add(new double[] { point[0].GetDouble(), point[1].GetDouble() });
                }
                var line = new GeoGraphicLineString(points);
                line.GetBounds(ref bounds);
                layer.AddObject(new Feature(line, []));
            }
        }

        private void ParseMultiPoint(Layer layer, JsonElement root, ref GeoBounds bounds)
        {
            var coords = root.GetProperty("coordinates");
            foreach (var pointCoords in coords.EnumerateArray())
            {
                double lon = pointCoords[0].GetDouble();
                double lat = pointCoords[1].GetDouble();
                var point = new GeoGraphicPoint(lon, lat);
                point.GetBounds(ref bounds);
                layer.AddObject(new Feature(point, []));
            }
        }

        private Dictionary<String, String> ParseProperties(JsonElement root)
        {
            var dict = new Dictionary<String, String>();
            foreach (var prop in root.EnumerateObject())
            {
                dict[prop.Name] = prop.Value.ToString();
            }
            return dict;
        }
        public void SaveLayerToGeoJson(Layer layer, string filePath)
        {
            var featureCollection = new
            {
                type = "FeatureCollection",
                features = layer.ObjectList.Select(feature =>
                    new { type = "Feature", geometry = ConvertGeoToJson(feature.Geometry), properties = feature.props })
            };

            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(featureCollection, options);
            File.WriteAllText(filePath, json);
        }

        private object ConvertGeoToJson(GeoGraphicObject geometry)
        {
            return geometry switch
            {
                GeoGraphicPoint point => new
                {
                    type = "Point",
                    coordinates = new[] { point.GeoCoords[0][0], point.GeoCoords[0][1] }
                },
                GeoGraphicLineString line => new
                {
                    type = "LineString",
                    coordinates = line.GeoCoords.Select(point => new[] { point[0], point[1] }).ToArray()
                },
                GeoGraphicPolygon polygon => new
                {
                    type = "Polygon",
                    coordinates = new[] { polygon.GeoCoords.Select(point => new[] { point[0], point[1] }).ToArray() }
                },
                _ => throw new Exception("Ошибка типа геометрии")
            };
        }

        public RasterLayer ImportImage(string filePath, Canvas mapCanvas)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(filePath);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            Image image = new Image
            {
                Source = bitmap,
                Width = bitmap.Width,
                Height = bitmap.Height,
                Stretch = Stretch.Uniform,
            };

            Canvas.SetLeft(image, 0);
            Canvas.SetTop(image, 0);

            GeoBounds bounds = new GeoBounds(37.5, 37.7, 55.7, 55.9);

            RasterLayer rasterLayer = new RasterLayer(image, Path.GetFileName(filePath));

            image.Tag = rasterLayer;
            rasterLayer.SetBounds(bounds);

            mapCanvas.Children.Add(image);

            return rasterLayer;
        }
    }
}