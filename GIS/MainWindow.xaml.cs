using GIS.Classes.Factories;
using GIS.Classes.Main;
using GIS.Classes.Services;
using GIS.Classes.ViewModels;
using GIS.Services;
using GIS.Windows;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GIS
{

    public partial class MainWindow : Window
    {
        private FileService fileService;
        private LayerManager layerManager;
        private CanvasManager canvasManager;
        private SelectionManager selectionManager;

        private MapMode currentMapMode = MapMode.Move;
        private bool isSelecting = false;
        private bool isLeftMouseButtonDown = false;
        private Point leftMouseButtonDownPoint;

        private bool isSelectionUpdated = false;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            fileService = new FileService();
            layerManager = new LayerManager();
            canvasManager = new CanvasManager(MapCanvas, layerManager.layersList);
            selectionManager = new SelectionManager(MapCanvas, layerManager.layersList);

            LayerTreeView.ItemsSource = layerManager.layersList;
        }
        private void MapCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            // начальные координаты 1:5000
            MapToCanvasTranslator.CanvasSize = MapCanvas.RenderSize;
            MapToCanvasTranslator.Bounds = new GeoBounds(37.615, 37.63742, 55.753, 55.788);
            MapToCanvasTranslator.CalculateRatios();
            MapToCanvasTranslator.ResetGlobalOffsets();
            UpdateScale();
        }
        private void LoadFileButton_Click(object sender, RoutedEventArgs e) // чтение GEOJSON файла
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "GEOfiles (*.geojson;*.geo.json)|*.geojson;*.geo.json|All files (*.*)|*.*",
                Multiselect = false,
                Title = "Выберите GeoJSON файл"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                Layer newLayer = fileService.LoadGeoJsonFile(openFileDialog.FileName);
                newLayer.AnalyzeFeatureProperties();
                newLayer.LayerStyle = DefaultStyleFactory.CreateDefaultStyle(newLayer.GeoType);
                layerManager.AddLayer(newLayer);

                MapToCanvasTranslator.ResetGlobalOffsets();
                canvasManager.DrawAll();

                StatusTextBox.Text = "Добавлен новый слой";
            }
            else
            {
                StatusTextBox.Text = "Отмена добавления нового слоя";
            }
        }

        #region Управление MapCanvas
        private void MapCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            var currentMousePoint = e.GetPosition(MapCanvas);
            CoordinatesTextBox.Text = $"Координаты: {currentMousePoint.X - MapToCanvasTranslator.GlobalOffsetX:f0}," +
                $" {currentMousePoint.Y - MapToCanvasTranslator.GlobalOffsetY:f0}";
            UpdateCurrentMouseCoordsDisplay(currentMousePoint);

            switch (currentMapMode)
            {
                case MapMode.Move:
                    canvasManager.HandleMoveMode(currentMousePoint, e,
                        ref isLeftMouseButtonDown, ref leftMouseButtonDownPoint);
                    break;

                case MapMode.Select:
                    if (isSelecting && isLeftMouseButtonDown && 
                    e.LeftButton == MouseButtonState.Pressed)
                        selectionManager.UpdateRectangleSelection(currentMousePoint);
                    break;

                case MapMode.Draw:
                    if (canvasManager.IsDrawingLines)
                        canvasManager.UpdateDrawingLine(currentMousePoint);
                    else if (canvasManager.IsDrawingPolygons)
                        canvasManager.UpdateDrawingPolygon(currentMousePoint);
                    break;
            }
        }
        private void MapCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            MapCanvas.Focus();
            var position = e.GetPosition(MapCanvas);

            switch (currentMapMode)
            {
                case MapMode.Move:
                    isLeftMouseButtonDown = true;
                    leftMouseButtonDownPoint = position;
                    MapCanvas.Cursor = Cursors.Hand;
                    break;

                case MapMode.Select:
                    var hit = VisualTreeHelper.HitTest(MapCanvas, position);

                    if (hit.VisualHit is Shape shape && shape.Tag is Feature feature)
                    {
                        selectionManager.SelectFeature(feature);
                        ShowFeatureInfo(feature);
                        SelectFeatureInTreeView(feature);
                    }
                    else
                    {
                        selectionManager.ClearSelection();

                        FeaturePropertiesGrid.Visibility = Visibility.Hidden;
                        isSelecting = true;
                        isLeftMouseButtonDown = true;
                        leftMouseButtonDownPoint = position;

                        selectionManager.StartRectangleSelection(position);
                    }
                    break;

                case MapMode.Draw:
                    if (LayerTreeView.SelectedItem is Layer selectedLayer)
                    {
                        canvasManager.SetSelectedLayer(selectedLayer);
                        switch (selectedLayer.GeoType)
                        {
                            case GeometryType.Point:
                                canvasManager.DrawPoint(position);
                                break;
                            case GeometryType.LineString:
                                canvasManager.DrawLine(position);
                                break;
                            case GeometryType.Polygon:
                                canvasManager.DrawPolygon(position);
                                break;
                        }
                    }
                    canvasManager.DrawAll();
                    break;
            }
        }
        private void MapCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var mousePos = e.GetPosition(MapCanvas);
            canvasManager.HandleMouseWheel(mousePos, e.Delta);
            UpdateScale();
        }
        private void LayerListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LayerTreeView.SelectedItem is Layer layer)
            {
                canvasManager.ZoomToLayer(layer);
            }
        }
        private void MapCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (currentMapMode == MapMode.Select && e.LeftButton == MouseButtonState.Released)
            {
                isSelecting = false;
                isLeftMouseButtonDown = false;
                selectionManager.EndRectangleSelection();

                var selectedFeatures = selectionManager.GetSelectedFeatures();
                if (selectedFeatures.Count > 0)
                    ShowFeatureInfo(selectedFeatures[selectedFeatures.Count - 1]);
            }
        }

        #endregion Управление MapCanvas

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
                var LSViewModel = new LayerSettingsViewModel(layer);
                var settingsWindow = new LayerSettingsWindow(LSViewModel);

                if (settingsWindow.ShowDialog() == true)
                {
                    canvasManager.DrawAll();
                    StatusTextBox.Text = $"Настройки слоя {layer.Name} изменены";
                }
            }
        }
        private void LayerTableButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Layer layer)
            {
                OpenLayerAttributesTableWindow(layer);
            }
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
                layerManager.MoveLayerUp(layer);
                canvasManager.DrawAll();
            }
        }
        private void LayerZIndexDown_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Layer layer)
            {
                layerManager.MoveLayerDown(layer);
                canvasManager.DrawAll();
            }
        }
        
        #endregion Управление кнопками слоёв

        #region FeaturePropertiesDataGrid
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
                //case Key.Up:
                //    MapToCanvasTranslator.GlobalOffsetY += 15;
                //    break;
                //case Key.Down:
                //    MapToCanvasTranslator.GlobalOffsetY -= 15;
                //    break;
                //case Key.Right:
                //    MapToCanvasTranslator.GlobalOffsetX -= 15;
                //    break;
                //case Key.Left:
                //    MapToCanvasTranslator.GlobalOffsetX += 15;
                //    break;
                case Key.Escape:
                    canvasManager.EndDrawing();
                    break;
                default:
                    return;
            }
            

            foreach (Layer layer in layerManager.layersList)
            {
                layer.UpdateAll();
            }
            canvasManager.DrawAll();
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
            var layer = layerManager.FindLayerByFeature(selectedItem);

            layer.DeleteObject(selectedItem);
            MapCanvas.Children.Remove(selectedItem.Geometry.Figure);

            CollectionViewSource.GetDefaultView(LayerTreeView.ItemsSource).Refresh();

            StatusTextBox.Text = $"Объект '{selectedItem.Name}' из слоя '{layer.Name}' удалён";
        }
        private void DeleteLayer(Layer layer)
        {
            layerManager.RemoveLayer(layer);

            foreach (var shape in layer.ObjectList)
            {
                MapCanvas.Children.Remove(shape.Geometry.Figure);
            }

            StatusTextBox.Text = $"Слой '{layer.Name}' удалён";
        }

        #endregion Функции удаления слоя/объекта
        private void CreateLayerButton_Click(object sender, RoutedEventArgs e)
        {
            var CLViewModel = new CreateNewLayerViewModel();
            var createLayerWindow = new CreateNewLayerWindow(CLViewModel);

            if (createLayerWindow.ShowDialog() == true)
            {
                var newLayer = new Layer
                {
                    Name = CLViewModel.LayerName,
                    GeoType = CLViewModel.GeoType,
                    FeatureProperties = CLViewModel.Attributes
                };

                layerManager.AddLayer(newLayer);
                canvasManager.DrawAll();

                StatusTextBox.Text = $"Добавлен новый слой {newLayer.Name}";
            }
        }
        private void SelectFeatureInTreeView(Feature selectedFeature)
        {
            var layer = layerManager.FindLayerByFeature(selectedFeature);

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
                selectionManager.ClearSelection();
                selectedLayer.IsSelected = true;
            }
            else if (e.NewValue is Feature selectedFeature)
            {
                selectionManager.ClearSelection();
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
                var rasterLayer = fileService.ImportImage(openFileDialog.FileName);

                layerManager.AddLayer(rasterLayer);
                canvasManager.DrawAll();
                StatusTextBox.Text = $"Импортировано изображение: {rasterLayer.Name}";
            }
        }
    }
}