using GIS.Classes.DrawObjects;
using GIS.Classes.Factories;
using GIS.Classes.Main;
using GIS.Classes.Services;
using GIS.Windows;
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
                if (feature.GetProperty("geometry").GetProperty("type").ToString() == "MultiPolygon")
                    ParseMultiPolygon(layer, feature.GetProperty("geometry"), ref bounds);
                else
                    layer.AddObject(ParseFeature(feature, ref bounds));
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
                        // TODO
                        break;

                    case "MultiPoint":
                        // TODO
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
        private Dictionary<String, String> ParseProperties(JsonElement root)
        {
            var dict = new Dictionary<String, String>();
            foreach (var prop in root.EnumerateObject())
            {
                dict[prop.Name] = prop.Value.ToString();
            }
            return dict;
        }

        public RasterLayer ImportImage(string filePath)
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
                Tag = filePath
            };

            Canvas.SetLeft(image, 0);
            Canvas.SetTop(image, 0);

            RasterLayer rasterLayer = new RasterLayer(image, Path.GetFileName(filePath));

            var window = new MapImageSettingsWindow();

            if (window.ShowDialog() == true)
            {
                rasterLayer.Bounds = window.ImageBounds;
            }
            else
            {
                rasterLayer.Bounds = new GeoBounds
                {
                    MinLon = -180,
                    MaxLon = 180,
                    MinLat = -90,
                    MaxLat = 90
                };
            }

            return rasterLayer;
        }
    }
}