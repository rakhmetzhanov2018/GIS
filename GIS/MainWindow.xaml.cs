using GIS.Classes;
using GIS.Classes.DrawObjects;
using GIS.Classes.Factories;
using GIS.Classes.Managers;
using GIS.Windows;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net.WebSockets;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GIS
{
    public enum MapMode
    {
        Move,
        Select,
        Draw
    }


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MapMode currentMapMode = MapMode.Move;

        private bool isSelecting = false;
        private Rectangle selectionRectangle;

        private bool _isLeftMouseButtonDown = false;
        private Point _leftMouseButtonDownPoint;

        private DrawingService drawingService;

        private bool isSelectionUpdated = false;

        public static ObservableCollection<Layer> layersList = [];
        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            LayerTreeView.ItemsSource = layersList;

            CreateSelectionRectangle();

            drawingService = new DrawingService(MapCanvas);
        }

        #region Управление слоями
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

        #endregion Управление слоями

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
            else if (type == "GeometryCollection")
            {
                foreach (JsonElement feature in root.GetProperty("geometries").EnumerateArray())
                {
                    if (feature.GetProperty("type").GetString() == "MultiPolygon")
                    {
                        ParseMultiPolygon(layer, feature, ref bounds);
                    }
                }
            }
            MapToCanvasTranslator.Bounds = bounds;
            MapToCanvasTranslator.CanvasSize = MapCanvas.RenderSize;
            MapToCanvasTranslator.CalculateRatios();

            layer.Bounds = bounds;
        }

        private void ParseMultiPolygon(Layer layer, JsonElement root, ref GeoBounds bounds)
        {
            foreach (JsonElement polygon in root.EnumerateArray())
            {
                GeoGraphicObject geo = GeoGraphicObject.Parse(polygon);
            }
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

        #region MapCanvas_MouseMove
        private void MapCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            var currentMousePoint = e.GetPosition(MapCanvas);

            CoordinatesTextBox.Text = $"Координаты: {currentMousePoint.X - MapToCanvasTranslator.GlobalOffsetX:f0}," +
                $" {currentMousePoint.Y - MapToCanvasTranslator.GlobalOffsetY:f0}";

            UpdateCurrentMouseCoordsDisplay(currentMousePoint);

            switch (currentMapMode)
            {
                case MapMode.Move:
                    MoveMode_MouseMove(currentMousePoint, e);
                    break;

                case MapMode.Select:
                    SelectMode_MouseMove(currentMousePoint, e);
                    break;

                case MapMode.Draw:
                    DrawMode_MouseMove(currentMousePoint, e);
                    break;
            }
        }
        private void MoveMode_MouseMove(Point currentMousePoint, MouseEventArgs e)
        {
            var offset = currentMousePoint - _leftMouseButtonDownPoint;

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
        private void SelectMode_MouseMove(Point currentMousePoint, MouseEventArgs e)
        {
            if (!isSelecting) return;

            if (_isLeftMouseButtonDown && e.LeftButton == MouseButtonState.Pressed)
            {
                UpdateSelectionRectangle(currentMousePoint);
            }
        }
        private void DrawMode_MouseMove(Point currentMousePoint, MouseEventArgs e)
        {
            if (drawingService.IsDrawingLines)
            {
                drawingService.UpdateDrawingLine(currentMousePoint);
            }
            else if (drawingService.IsDrawingPolygons)
            {
                drawingService.UpdateDrawingPolygon(currentMousePoint);
            }
        }

        #endregion MapCanvas_MouseMove

        #region MapCanvas_MouseDown
        private void MapCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            MapCanvas.Focus();

            var position = e.GetPosition(MapCanvas);

            switch (currentMapMode)
            {
                case MapMode.Move:
                    MoveMode_MouseDown(position);
                    break;

                case MapMode.Select:
                    SelectMode_MouseDown(position);
                    break;

                case MapMode.Draw:
                    DrawMode_MouseDown(position);
                    break;
            }
        }
        private void MoveMode_MouseDown(Point position)
        {
            _isLeftMouseButtonDown = true;
            _leftMouseButtonDownPoint = position;
            MapCanvas.Cursor = Cursors.Hand;
        }
        private void SelectMode_MouseDown(Point position)
        {
            var hit = VisualTreeHelper.HitTest(MapCanvas, position);

            if (hit.VisualHit is Shape shape && shape.Tag is Feature feature)
            {
                ClearSelection();
                feature.IsSelected = true;
                ShowFeatureInfo(feature);

                SelectFeatureInTreeView(feature);
            }
            else
            {
                ClearSelection();
                StartSelection(position);
            }
        }
        private void DrawMode_MouseDown(Point position)
        {
            if (LayerTreeView.SelectedItem is not Layer selectedLayer)
                return;

            drawingService.SetSelectedLayer(selectedLayer);

            switch (selectedLayer.GeoType) {

                case "Point":
                    drawingService.DrawPoint(position);
                    break;
                
                case "LineString":
                    drawingService.DrawLine(position);
                    break;

                case "Polygon":
                    drawingService.DrawPolygon(position);
                    break;
            };

            Draw();
        }

        #endregion MapCanvas_MouseDown

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
            if (LayerTreeView.SelectedItem is Layer layer)
            {
                ZoomToLayer(layer);
            }
        }

        private void MapCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (currentMapMode == MapMode.Select && e.LeftButton == MouseButtonState.Released)
            {
                EndSelection(e.GetPosition(MapCanvas));
            }
        }

        #endregion Управление MapCanvas

        #region Выделение фигур прямоугольником

        private void CreateSelectionRectangle()
        {
            selectionRectangle = new Rectangle
            {
                Stroke = Brushes.Gray,
                StrokeThickness = 2,
                StrokeDashArray = new DoubleCollection { 4, 2 },
                Fill = new SolidColorBrush(Color.FromArgb(40, 30, 144, 255)),
                IsHitTestVisible = false,
                Visibility = Visibility.Collapsed
            };

            MapCanvas.Children.Add(selectionRectangle);
            Canvas.SetZIndex(selectionRectangle, 100);
        }
        private void StartSelection(Point startPoint)
        {
            isSelecting = true;
            _isLeftMouseButtonDown = true;
            _leftMouseButtonDownPoint = startPoint;

            Canvas.SetLeft(selectionRectangle, startPoint.X);
            Canvas.SetTop(selectionRectangle, startPoint.Y);

            selectionRectangle.Width = 0;
            selectionRectangle.Height = 0;

            selectionRectangle.Visibility = Visibility.Visible;
        }
        private void UpdateSelectionRectangle(Point currentPoint)
        {
            if (!isSelecting) return;

            var width = Math.Abs(currentPoint.X - _leftMouseButtonDownPoint.X);
            var height = Math.Abs(currentPoint.Y - _leftMouseButtonDownPoint.Y);

            Canvas.SetLeft(selectionRectangle, Math.Min(currentPoint.X, _leftMouseButtonDownPoint.X));
            Canvas.SetTop(selectionRectangle, Math.Min(currentPoint.Y, _leftMouseButtonDownPoint.Y));

            selectionRectangle.Width = width;
            selectionRectangle.Height = height;
        }
        private void EndSelection(Point endPoint)
        {
            isSelecting = false;
            _isLeftMouseButtonDown = false;

            selectionRectangle.Visibility = Visibility.Collapsed;

            var selectedFeatures = FindSelectedFeatures();

            foreach( var feature in selectedFeatures )
            {
                feature.IsSelected = true;
            }
        }
        private List<Feature> FindSelectedFeatures()
        {

            List<Feature> selectedFeatures = new List<Feature>();

            Rect selectionArea = new Rect
            {
                X = Canvas.GetLeft(selectionRectangle),
                Y = Canvas.GetTop(selectionRectangle),
                Width = selectionRectangle.Width,
                Height = selectionRectangle.Height
            };

            foreach (Layer layer in layersList)
            {
                foreach (Feature feature in layer.ObjectList)
                {
                    if (IsFigureInArea(feature.Geometry.Figure, selectionArea))
                    {
                        selectedFeatures.Add(feature);
                    }
                }
            }

            return selectedFeatures;

        }
        private bool IsFigureInArea(Shape figure, Rect area)
        {
            if (figure is Ellipse ellipse)
            {
                return area.Contains(Canvas.GetLeft(ellipse) + ellipse.Width / 2, 
                                     Canvas.GetTop(ellipse) + ellipse.Height / 2);
            }

            RectangleGeometry rectGeo = new RectangleGeometry(area);
            Geometry figureGeo = figure.RenderedGeometry;

            return rectGeo.FillContainsWithDetail(figureGeo) != IntersectionDetail.Empty;
        }

        #endregion Выделение фигур прямоугольником

        #region Отрисовка фигур
        private void Draw()
        {
            foreach (var layer in layersList)
            {
                layer.DrawAll(MapCanvas);
            }

            ApplyStylesForAllLayers();
            UpdateScale();
        }

        private void ApplyStylesForAllLayers()
        {
            foreach (var layer in layersList)
            {
                layer.ApplyStyleToAllFeatures();
            }
        }

        #endregion Отрисовка фигур

        #region Обновление статус-бара
        private void UpdateScale()
        {
            MapToCanvasTranslator.CanvasSize = new Size(MapCanvas.ActualWidth, MapCanvas.ActualHeight);
            ScaleTextBox.Text = MapToCanvasTranslator.GetScale();
        }
        private void UpdateCurrentMouseCoordsDisplay(Point position)
        {
            if (MapToCanvasTranslator.Bounds.MinLon == double.MaxValue)
            {
                MouseGeoCoordsTextBox.Text = "Границ пока нет";
                return;
            }

            var geoCoords = MapToCanvasTranslator.TranslateFromCanvasToGeo(position.X, position.Y);

            MouseGeoCoordsTextBox.Text = $"Текущие координаты: {geoCoords[0]:F6}°; {geoCoords[1]:F6}°";
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

        private void LayerZIndexUp_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Layer layer)
            {
                int index = layersList.IndexOf(layer);

                if (index > 0)
                {
                    layersList.Move(index, index - 1);
                }

                Draw();
            }
        }
        private void LayerZIndexDown_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Layer layer)
            {
                int index = layersList.IndexOf(layer);

                if (index < layersList.Count - 1)
                {
                    layersList.Move(index, index + 1);
                }

                Draw();
            }
        }
        #endregion Управление кнопками слоёв

        #region FeaturePropertiesDataGrid
        private void ClearSelection()
        {
            foreach (var layer in layersList)
            {
                foreach (var feature in layer.ObjectList)
                {
                    feature.IsSelected = false;
                }
                layer.IsSelected = false;
            }

            FeaturePropertiesGrid.Visibility = Visibility.Hidden;
        }
        private void ShowFeatureInfo(Feature feature)
        {
            FeaturePropertiesGrid.Visibility = Visibility.Visible;

            FillTable(feature);
        }

        private void FillTable(Feature feature)
        {
            FeaturePropertiesDataGrid.Items.Clear();

            foreach (var prop in feature.props)
            {
                FeaturePropertiesDataGrid.Items.Add(new
                {
                    Key = prop.Key,
                    Value = prop.Value
                });
            }           
        }
        #endregion FeaturePropertiesDataGrid

        #region Экспериментальные/временные функции
        private void MapCanvas_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    MapToCanvasTranslator.GlobalOffsetY += 5;
                    break;
                case Key.Down:
                    MapToCanvasTranslator.GlobalOffsetY -= 5;
                    break;
                case Key.Right:
                    MapToCanvasTranslator.GlobalOffsetY += 5;
                    break;
                case Key.Left:
                    MapToCanvasTranslator.GlobalOffsetY -= 5;
                    break;
                case Key.Escape:
                    drawingService.EndDrawing();
                    break;
                default:
                    return;
            }

            foreach (Layer layer in layersList)
            {
                layer.UpdateAll();
            }
            Draw();
        }
        private void MapModeRadioButton_Click(object sender, EventArgs e)
        {
            if (sender is RadioButton radioButton)
            {
                var mode = radioButton.Tag.ToString();

                currentMapMode = mode switch
                {
                    "Move" => MapMode.Move,
                    "Select" => MapMode.Select,
                    "Draw" => MapMode.Draw,
                    _ => throw new NotImplementedException()
                };

            } 
        }
        
        #endregion Экспериментальные/временные функции

        #region Функции удаления слоя/объекта
        private void LayerListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                if (LayerTreeView.SelectedItem is Layer layer)
                {
                    var result = MessageBox.Show($"Вы уверели что хотите удалить слой '{layer.Name}'?",
                        "Удаление",
                        MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                    
                    if (result == MessageBoxResult.Yes)
                        DeleteLayer(layer);
                }
                else if (LayerTreeView.SelectedItem is Feature feature)
                {
                    var result = MessageBox.Show($"Вы уверели что хотите удалить объект '{feature.Name}'?",
                        "Удаление",
                        MessageBoxButton.YesNo, MessageBoxImage.Exclamation);

                    if (result == MessageBoxResult.Yes)
                        DeleteFeature(feature);
                }
            }
        }
        private void DeleteFeature(Feature selectedItem)
        {
            var layer = FindLayerByFeature(selectedItem);

            layer.DeleteObject(selectedItem);
            MapCanvas.Children.Remove(selectedItem.Geometry.Figure);

            CollectionViewSource.GetDefaultView(LayerTreeView.ItemsSource).Refresh();

            StatusTextBox.Text = $"Объект '{selectedItem.Name}' из слоя '{layer.Name}' удалён";
        }
        private void DeleteLayer(Layer layer)
        {
            layersList.Remove(layer);

            foreach (var shape in layer.ObjectList)
            {
                MapCanvas.Children.Remove(shape.Geometry.Figure);
            }

            StatusTextBox.Text = $"Слой '{layer.Name}' удалён";
        }

        private Layer FindLayerByFeature(Feature feature)
        {
            foreach (Layer layer in layersList)
            {
                if (layer.ObjectList.Contains(feature))
                    return layer;
            }
            throw new Exception("Попытка удалить уже удалённый объект.");
        }

        #endregion Функции удаления слоя/объекта
        private void CreateLayerButton_Click(object sender, RoutedEventArgs e)
        {
            var createLayerWindow = new CreateNewLayerWindow();

            if (createLayerWindow.ShowDialog() == true)
            {
                var newLayer = createLayerWindow.CreatedLayer;
                layersList.Add(newLayer);
                Draw();

                StatusTextBox.Text = $"Добавлен новый слой {newLayer.Name}";
            }
        }

        private void AddObjectToLayer_MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SelectFeatureInTreeView(Feature selectedFeature)
        {
            var layer = FindLayerByFeature(selectedFeature);

            if (LayerTreeView.ItemContainerGenerator.ContainerFromItem(layer) is TreeViewItem layerItem)
            {
                layerItem.IsExpanded = true;
            }

            if (LayerTreeView.ItemContainerGenerator.ContainerFromItem(selectedFeature) is TreeViewItem featureItem)
            {
                featureItem.BringIntoView();
                featureItem.Focus();
            }
        }

        private void LayerTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (isSelectionUpdated)
            {
                return;
            }

            isSelectionUpdated = true;

            if (e.NewValue is Layer selectedLayer)
            {
                ClearSelection();
                selectedLayer.IsSelected = true;
            }
            else if (e.NewValue is Feature selectedFeature)
            {
                ClearSelection();
                selectedFeature.IsSelected = true;
                ShowFeatureInfo(selectedFeature);
            }

            isSelectionUpdated = false;
        }

        private void ImportImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "Image files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png|All files (*.*)|*.*",
                Title = "Выберите изображение",
                Multiselect = false,
            };
            
            if (openFileDialog.ShowDialog() == true)
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(openFileDialog.FileName);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                Image image = new Image
                {
                    Source = bitmap,
                    Width = bitmap.Width,
                    Height = bitmap.Height,
                    Stretch = Stretch.Uniform,
                    Tag = openFileDialog.FileName
                };

                Canvas.SetLeft(image, 0);
                Canvas.SetTop(image, 0);

                RasterLayer rasterLayer = new RasterLayer(image, openFileDialog.FileName);
                layersList.Add(rasterLayer);
            }
        }
    }
}