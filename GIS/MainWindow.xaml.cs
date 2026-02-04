using GIS.Classes;
using GIS.Classes.DrawObjects;
using GIS.Windows;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GIS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _isLeftMouseButtonDown = false;
        private Point _leftMouseButtonDownPoint;

        public static ObservableCollection<Layer> layersList = [];
        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            LayerListBox.ItemsSource = layersList;
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
                    Name = System.IO.Path.GetFileName(filePath),
                    ZIndex = layersList.Count + 1
                };

                ParseGeoJSON(newLayer, text);

                layersList.Add(newLayer);

                MapToCanvasTranslator.ResetGlobalOffsets();
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
            GeoGraphicObject geo = GeoGraphicObject.Parse(root.GetProperty("geometry"));
            Dictionary<String, String> dict = ParseProperties(root.GetProperty("properties"));
            
            geo.GetBounds(ref bounds);

            return new Feature(geo, dict);
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

            CoordinatesTextBox.Text = $"Координаты: {currentMousePoint.X - MapToCanvasTranslator.GlobalOffsetX:f0}," +
                $" {currentMousePoint.Y - MapToCanvasTranslator.GlobalOffsetY:f0}";  

            if (_isLeftMouseButtonDown && e.LeftButton == MouseButtonState.Pressed)
            {
                MapToCanvasTranslator.GlobalOffsetX += offset.X;
                MapToCanvasTranslator.GlobalOffsetY += offset.Y;

                foreach (Layer layer in layersList)
                {
                    layer.UpdateAll();                
                }

                _leftMouseButtonDownPoint = currentMousePoint;
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
            var mousePos = e.GetPosition(MapCanvas);

            double scaleDelta = e.Delta > 0 ? 1.1 : 1 / 1.1;

            MapToCanvasTranslator.GlobalOffsetX =
                mousePos.X - (mousePos.X - MapToCanvasTranslator.GlobalOffsetX) * scaleDelta;
            MapToCanvasTranslator.GlobalOffsetY =
                mousePos.Y - (mousePos.Y - MapToCanvasTranslator.GlobalOffsetY) * scaleDelta;
            MapToCanvasTranslator.GlobalScale *= scaleDelta;

            foreach (Layer layer in layersList)
            {
                layer.UpdateAll();
            }

            UpdateScale();
        }

        private void ZoomToLayer(Layer layer)
        {
            MapToCanvasTranslator.Bounds = layer.Bounds;
            MapToCanvasTranslator.ResetGlobalOffsets();
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
        private void Draw()
        {
            foreach (var layer in layersList)
            {
                layer.DrawAll(MapCanvas);
            }
            ApplyStylesForAllLayers();
            UpdateScale();
            UpdateBoundsDisplay();
        }

        private void ApplyStylesForAllLayers()
        {
            foreach (var layer in layersList)
            {
                layer.ApplyStyleToAllFeatures();
            }
        }

        #endregion Рисование фигур

        #region Обновление статус-бара
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
        #endregion Обновление статус-бара

        #region Управление кнопками слоёв
        private void LayerSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Layer layer)
            {
                OpenLayerSettingWindow(layer);
            }
        }

        private void LayerTableButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Layer layer)
            {
                OpenLayerAttributesTableWindow(layer);
            }
        }
        private void OpenLayerSettingWindow(Layer layer)
        {
            var settingsWindow = new LayerSettingsWindow(layer);
            settingsWindow.ShowDialog();
        }
        private void OpenLayerAttributesTableWindow(Layer layer)
        {
            var tableWindow = new LayerAttributesTableWindow(layer);
            tableWindow.Show();
        }
        #endregion Управление кнопками слоёв

        public void OnZIndexChanged()
        {
            MapCanvas.Children.Clear();

            var orderedList = layersList.OrderBy(x => x.ZIndex).ToList();
            layersList = new ObservableCollection<Layer>(orderedList);
        }
    }
}