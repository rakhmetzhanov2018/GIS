using GIS.Classes;
using GIS.Classes.DrawObjects;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GIS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _isLeftMouseButtonDown = false;
        private Point _leftMouseButtonDownPoint;
        private Point _topLeftMapCanvasPoint = new Point(0, 0);

        internal static ObservableCollection<Layer> layersList = [];
        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            LayerListBox.ItemsSource = layersList;

            foreach (Layer layer in layersList)
            {
                layer.VisibilityChanged += OnLayerVisibilityChanged;
            }
        }

        private void LoadFileButton_Click(object sender, RoutedEventArgs e) // чтение GEOJSON файла
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "GEOfiles (*.geojson)|*.geojson|All files (*.*)|*.*",
                Multiselect = false,
                Title = "Выберите GeoJSON файл"
            };


            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                string text = "";

                var fileStream = openFileDialog.OpenFile();

                try
                {
                    using StreamReader streamReader = new(fileStream);
                    text = streamReader.ReadToEnd();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                Layer newLayer = new()
                {
                    Name = System.IO.Path.GetFileName(filePath)
                };

                ParseGeoJSON(newLayer, text);

                Console.WriteLine(MapToCanvasTranslator.Bounds);


                layersList.Add(newLayer);
                newLayer.CreateAll();
                newLayer.VisibilityChanged += OnLayerVisibilityChanged;
                Draw();

                StatusTextBox.Text = "Добавлен новый слой";
            }
            else
            {
                StatusTextBox.Text = "Отмена добавления нового слоя";
            }
        }

        #region Парсинг GeoJSON

        private void ParseGeoJSON(Layer layer, string geoJSON)
        {
            GeoBounds bounds = new GeoBounds();

            var doc = JsonDocument.Parse(geoJSON);
            var root = doc.RootElement;
            var type = root.GetProperty("type").GetString();

            if (type == "FeatureCollection")
            {
                ParseFeatureCollection(layer, root, ref bounds);
            }
            else if (type == "Feature")
            {
                layer.AddObject(ParseFeature(root, ref bounds));
            }
            MapToCanvasTranslator.Bounds = bounds;
            MapToCanvasTranslator.CanvasSize = MapCanvas.RenderSize;
            MapToCanvasTranslator.CalculateRatios();

            layer.Bounds = bounds;
        }

        private void ParseFeatureCollection(Layer layer, JsonElement root, ref GeoBounds bounds)
        {
            foreach (JsonElement feature in root.GetProperty("features").EnumerateArray())
            {
                layer.AddObject(ParseFeature(feature, ref bounds));
            }
        }

        private Feature ParseFeature(JsonElement root, ref GeoBounds bounds)
        {
            GeoObject geo = ParseGeometry(root.GetProperty("geometry"));
            Dictionary<String, String> dict = ParseProperties(root.GetProperty("properties"));
            
            geo.GetBounds(ref bounds);

            return new Feature(geo, dict);
        }

        private GeoObject ParseGeometry(JsonElement root)
        {
            return GeoObject.Parse(root);
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

        #endregion Парсинг GeoJSON

        #region Управление MapCanvas
        private void MapCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            var currentMousePoint = e.GetPosition(MapCanvas);
            var offset = currentMousePoint - _leftMouseButtonDownPoint;

            CoordinatesTextBox.Text = $"Координаты: {currentMousePoint.X - _topLeftMapCanvasPoint.X:f0}," +
                $" {currentMousePoint.Y - _topLeftMapCanvasPoint.Y:f0}";  

            if (_isLeftMouseButtonDown && e.LeftButton == MouseButtonState.Pressed)
            {
                foreach (Layer layer in layersList)
                {
                    layer.UpdateAll(offset.X, offset.Y, 1);
                }

                _leftMouseButtonDownPoint = currentMousePoint;
                _topLeftMapCanvasPoint += offset;
            }
            else if (e.LeftButton != MouseButtonState.Pressed)
            {
                _isLeftMouseButtonDown = false;
                MapCanvas.Cursor = Cursors.Arrow;
            }

        }
        private void MapCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _isLeftMouseButtonDown = true;
                _leftMouseButtonDownPoint = e.GetPosition(MapCanvas);
                MapCanvas.Cursor = Cursors.Hand;
            }
        }
        private void MapCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            foreach (Layer layer in layersList)
            {
                var mousePos = e.GetPosition(MapCanvas);
                layer.UpdateAll(-mousePos.X, -mousePos.Y, (1 + e.Delta * 0.001));
                layer.UpdateAll(mousePos.X, mousePos.Y, 1);

            }
            UpdateScale();
        }

        private void ZoomToLayer(Layer layer)
        {
            MapToCanvasTranslator.Bounds = layer.Bounds;
            MapToCanvasTranslator.CalculateRatios();
            Draw();
        }

        private void LayerListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LayerListBox.SelectedItem is Layer layer)
            {
                ZoomToLayer(layer);
            }
        }

        #endregion Управление MapCanvas

        #region Рисование фигур
        private void RecreateAllFigures()
        {
            MapCanvas.Children.Clear();

            foreach (var layer in layersList)
            {
                foreach (var feature in layer.ObjectList)
                {
                    feature.Figure = null;
                }
                layer.CreateAll();
            }
        }
        private void Draw()
        {
            RecreateAllFigures();

            foreach (var layer in layersList)
            {
                if (layer.IsVisible)
                {
                    layer.DrawAll(MapCanvas);
                }
            }
            UpdateScale();
            UpdateBoundsDisplay();
        }

        #endregion Рисование фигур

        private void OnLayerVisibilityChanged(object sender, EventArgs e)
        {
            Draw();
        }
        private void UpdateScale()
        {
            MapToCanvasTranslator.CanvasSize = new Size(MapCanvas.ActualWidth, MapCanvas.ActualHeight);
            ScaleTextBox.Text = MapToCanvasTranslator.GetScale();
        }
        private void UpdateBoundsDisplay()
        {
            const double KM_PER_DEGREE = 111.32;

            if (MapToCanvasTranslator.Bounds.MinLon == double.MaxValue)
            {
                BoundsCenterTextBox.Text = "Границ пока нет";
                BoundsSizeTextBox.Text = "Границ пока нет";
                return;
            }

            double centerLon = (MapToCanvasTranslator.Bounds.MinLon + MapToCanvasTranslator.Bounds.MaxLon) / 2;
            double centerLat = (MapToCanvasTranslator.Bounds.MinLat + MapToCanvasTranslator.Bounds.MaxLat) / 2;

            double deltaLon = (MapToCanvasTranslator.Bounds.MaxLon - MapToCanvasTranslator.Bounds.MinLon);
            double deltaLat = (MapToCanvasTranslator.Bounds.MaxLat - MapToCanvasTranslator.Bounds.MinLat);

            double widthKM = deltaLon * KM_PER_DEGREE;
            double heightKM = deltaLat * KM_PER_DEGREE * Math.Cos(centerLat * Math.PI / 180);

            BoundsCenterTextBox.Text = $"Центр: {centerLon:F6}°; {centerLat:F6}°";
            BoundsSizeTextBox.Text = $"Размер: {widthKM:F1}×{heightKM:F1} км";
        }
    }
}