using GIS.Classes.Factories;
using GIS.Classes.Layers;
using GIS.Classes.Main;
using GIS.Classes.Services;
using GIS.Classes.ViewModels;
using GIS.Services;
using GIS.Windows;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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

        private bool isCalibrating = false;
        private RasterLayerSettingsViewModel calibrationViewModel;
        private List<Point> calibrationImagePoints = new List<Point>();
        private List<Point> calibrationGeoPoints = new List<Point>();
        private RasterLayer calibrationTargetRasterLayer;

        private OsmTileLayer osmLayer;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            fileService = new FileService();
            layerManager = new LayerManager();
            canvasManager = new CanvasManager(MapCanvas, layerManager.layersList);
            selectionManager = new SelectionManager(MapCanvas, layerManager.layersList);

            LayerTreeView.ItemsSource = layerManager.layersList;

            osmLayer = new OsmTileLayer(MapCanvas);
            osmLayer.SetLayerType(OSMTileType.None);
            layerManager.AddLayer(osmLayer);
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
        
        #region Основные кнопки
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
                    FeatureProperties = CLViewModel.Attributes,
                    Bounds = new GeoBounds()
                };

                layerManager.AddLayer(newLayer);
                canvasManager.DrawAll();

                StatusTextBox.Text = $"Добавлен новый слой {newLayer.Name}";
            }
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
                var rasterLayer = fileService.ImportImage(openFileDialog.FileName, MapCanvas);

                layerManager.AddLayer(rasterLayer);
                canvasManager.DrawAll();
                StatusTextBox.Text = $"Импортировано изображение: {rasterLayer.Name}";
            }
        }
        private void SaveLayerButton_Click(object sender, RoutedEventArgs e)
        {
            if (LayerTreeView.SelectedItem is Layer layer)
            {
                var sfd = new SaveFileDialog()
                {
                    Filter = "GEOfiles (*.geojson;*.geo.json)|*.geojson;*.geo.json|All files (*.*)|*.*",
                    DefaultExt = ".geo.json",
                    FileName = layer.Name + ".geo.json"
                };

                if (sfd.ShowDialog() == true)
                {
                    try
                    {
                        fileService.SaveLayerToGeoJson(layer, sfd.FileName);
                        StatusTextBox.Text = $"Слой {layer.Name} был успешно сохранён в файл {sfd.FileName}";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Произошла ошибка сохранения {ex}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        # endregion Основные кнопки

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
            var position = e.GetPosition(MapCanvas);

            if (e.RightButton == MouseButtonState.Pressed && (canvasManager.IsDrawingLines || canvasManager.IsDrawingPolygons))
            {
                canvasManager.EndDrawing();
            }
            if (e.RightButton == MouseButtonState.Pressed && isCalibrating)
            {
                var hitImage = FindImageOnClick(position);
                if (hitImage != null && hitImage.Tag is RasterLayer rasterLayer)
                {
                    if (rasterLayer != calibrationTargetRasterLayer)
                    {
                        MessageBox.Show($"Калибровка проводится для слоя '{calibrationTargetRasterLayer?.Name}'. " +
                           $"Вы кликнули на '{rasterLayer.Name}'. Пожалуйста, кликайте на целевой слой.",
                           "Не тот слой", MessageBoxButton.OK, MessageBoxImage.Warning);
                        e.Handled = true;
                        return;
                    }

                    Point imagePoint = e.GetPosition(hitImage);
                    var dialog = new InputCoordinatesWindow();
                    if (dialog.ShowDialog() == true)
                    {
                        calibrationImagePoints.Add(imagePoint);
                        calibrationGeoPoints.Add(new Point(dialog.Lon, dialog.Lat));
                        if (calibrationImagePoints.Count >= 2)
                        {
                            RecalculateBounds(rasterLayer);
                            EndCalibrationProcess();
                        }
                        else
                        {
                            StatusTextBox.Text = $"Добавлена точка {calibrationImagePoints.Count}. Нужно ещё {2 - calibrationImagePoints.Count} точки.";
                        }
                    }
                    else
                    {
                        MessageBox.Show("Кликните прямо на изображении растрового слоя.", "Калибровка", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    e.Handled = true;
                    return;
                }

            }

            if (e.LeftButton != MouseButtonState.Pressed) return;
            switch (currentMapMode)
            {
                case MapMode.Move:
                    isLeftMouseButtonDown = true;
                    leftMouseButtonDownPoint = position;
                    MapCanvas.Cursor = Cursors.Hand;
                    break;

                case MapMode.Select:
                    selectionManager.ClearSelection();
                    ClearTreeViewSelection(LayerTreeView);
                    var hit = VisualTreeHelper.HitTest(MapCanvas, position);

                    if (hit.VisualHit is Shape shape && shape.Tag is Feature feature)
                    {

                        selectionManager.SelectFeature(feature);
                        ShowFeatureInfo(feature);
                        SelectFeatureInTreeView(feature);
                    }
                    else
                    {
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
            //if (canvasManager.IsDrawingLines || canvasManager.IsDrawingPolygons)
            //    return;
            if (canvasManager.IsDrawingLines || canvasManager.IsDrawingPolygons)
            {
                var mousePos2 = e.GetPosition(MapCanvas);
                canvasManager.HandleMouseWheel(mousePos2, e.Delta);
                canvasManager.UpdateTempFigures();
                UpdateScale();
                return;
            }

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
            if (currentMapMode == MapMode.Select && e.LeftButton == MouseButtonState.Released && isSelecting)
            {
                isSelecting = false;
                isLeftMouseButtonDown = false;
                selectionManager.EndRectangleSelection();

                var selectedFeatures = selectionManager.GetSelectedFeatures();
                if (selectedFeatures.Count == 1)
                    ShowFeatureInfo(selectedFeatures[0]);
            }
        }
        private void MapCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            if (currentMapMode == MapMode.Select && isSelecting)
            {
                isSelecting = false;
                isLeftMouseButtonDown = false;
                selectionManager.EndRectangleSelection();
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
                if (layer is RasterLayer rasterLayer)
                {
                    var viewModel = new RasterLayerSettingsViewModel(rasterLayer);
                    var rasterLayerSettings = new RasterLayerSettingsWindow(viewModel);
                    if (rasterLayerSettings.ShowDialog() == true)
                    {
                        canvasManager.DrawAll();
                        StatusTextBox.Text = $"Настройки растрового слоя {rasterLayer.Name} изменены";
                    }
                }
                else
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
                    if (isCalibrating)
                    {
                        EndCalibrationProcess();
                    }
                    else if (currentMapMode == MapMode.Select && isSelecting)
                    {
                        isSelecting = false;
                        selectionManager.EndRectangleSelection();
                        selectionManager.ClearSelection();
                        FeaturePropertiesGrid.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        canvasManager.CancelDrawing();
                    }
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
            if (currentMapMode == MapMode.Select && isSelecting)
            {
                isSelecting = false;
                selectionManager.EndRectangleSelection();
            }

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

            StatusTextBox.Text = $"Объект '{selectedItem.Name}' из слоя '{layer.Name}' удалён";
        }
        private void DeleteLayer(Layer layer)
        {
            layerManager.RemoveLayer(layer);

            foreach (var shape in layer.ObjectList)
            {
                MapCanvas.Children.Remove(shape.Geometry.Figure);
            }
            if (layer is RasterLayer rasterLayer)
            {
                MapCanvas.Children.Remove(rasterLayer.RasterImage);
            }

            StatusTextBox.Text = $"Слой '{layer.Name}' удалён";
        }

        #endregion Функции удаления слоя/объекта

        #region Управление TreeView
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
                foreach (Feature feature in selectedLayer.ObjectList)
                {
                    feature.IsSelected = true;
                }

            }
            else if (e.NewValue is Feature selectedFeature)
            {
                selectionManager.ClearSelection();
                selectedFeature.IsSelected = true;
                ShowFeatureInfo(selectedFeature);
            }

            isSelectionUpdated = false;
        }
        private void ClearTreeViewSelection(ItemsControl control)
        {
            for (int i = 0; i < control.Items.Count; i++)
            {
                var item = control.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;
                if (item != null)
                {
                    item.IsSelected = false;
                    ClearTreeViewSelection(item);
                }
            }
        }
        #endregion Управление TreeView

        #region Калибрация изображения
        public void StartCalibrationProcess(RasterLayerSettingsViewModel calibrationViewModel, RasterLayer targetLayer)
        {
            isCalibrating = true;
            this.calibrationViewModel = calibrationViewModel;
            calibrationTargetRasterLayer = targetLayer;
            calibrationGeoPoints.Clear();
            calibrationImagePoints.Clear();

            canvasManager.ZoomToLayer(targetLayer);
            Canvas.SetZIndex(calibrationTargetRasterLayer.RasterImage, 200);

            StatusTextBox.Text = "Включён режим калибровки, нажниме правой кнопкой на изображении для указания реальных географических коодринат";
        }
        public void EndCalibrationProcess()
        {
            isCalibrating = false;
            calibrationImagePoints.Clear();
            calibrationGeoPoints.Clear();
            StatusTextBox.Text = "Калибровка завершена.";

            Canvas.SetZIndex(calibrationTargetRasterLayer.RasterImage, 0);

            if (calibrationViewModel != null)
            {
                var newWindow = new RasterLayerSettingsWindow(calibrationViewModel);
                if (newWindow.ShowDialog() == true)
                {
                    canvasManager.ZoomToLayer(calibrationTargetRasterLayer);
                }
                calibrationViewModel = null;
                calibrationTargetRasterLayer = null;
            }

        }
        private void RecalculateBounds(RasterLayer rasterLayer)
        {
            if (calibrationImagePoints.Count < 2) return;

            Point img1 = calibrationImagePoints[0];
            Point img2 = calibrationImagePoints[1];
            Point geo1 = calibrationGeoPoints[0];
            Point geo2 = calibrationGeoPoints[1];

            double imgWidth = rasterLayer.RasterImage.ActualWidth;
            double imgHeight = rasterLayer.RasterImage.ActualHeight;

            double deltaX = img2.X - img1.X;

            if (Math.Abs(deltaX) < 1e-6)
            {
                MessageBox.Show("Ошибка: точки имеют одинаковую X-координату. Выберите точки с разной долготой.",
                                "Калибровка", MessageBoxButton.OK, MessageBoxImage.Warning);
                EndCalibrationProcess();
                return;
            }

            double scale = (geo2.X - geo1.X) / deltaX;
            double shiftX = geo1.X - scale * img1.X;
            double shiftY = geo1.Y + scale * img1.Y;

            double minLon = shiftX;
            double maxLon = shiftX + scale * imgWidth;
            double maxLat = shiftY;
            double minLat = shiftY - scale * imgHeight;

            if (minLon > maxLon) { double t = minLon; minLon = maxLon; maxLon = t; }
            if (minLat > maxLat) { double t = minLat; minLat = maxLat; maxLat = t; }

            calibrationViewModel.MinLon = minLon;
            calibrationViewModel.MaxLon = maxLon;
            calibrationViewModel.MinLat = minLat;
            calibrationViewModel.MaxLat = maxLat;

            StatusTextBox.Text = "Калибровка завершена, новые границы применены";
        }
        private Image FindImageOnClick(Point canvasPoint)
        {
            foreach (UIElement elem in MapCanvas.Children)
            {
                if (elem is Image img && img.Visibility == Visibility.Visible)
                {
                    Point relativePoint = Mouse.GetPosition(img);
                    if (relativePoint.X >= 0 && relativePoint.X <= img.ActualWidth &&
                        relativePoint.Y >= 0 && relativePoint.Y <= img.ActualHeight)
                    {
                        return img;
                    }
                }
            }
            return null;
        }
        #endregion Калибрация изображения

        #region Кнопки изменения подложки
        private void StreetMapButton_Click(object sender, RoutedEventArgs e)
        {
            osmLayer.SetLayerType(OSMTileType.Street);
            osmLayer.UpdateAll();
            canvasManager.DrawAll();
        }

        private void SatelliteButton_Click(object sender, RoutedEventArgs e)
        {
            osmLayer.SetLayerType(OSMTileType.Satellite);
            osmLayer.UpdateAll();
            canvasManager.DrawAll();
        }

        private void NoMapButton_Click(object sender, RoutedEventArgs e)
        {
            osmLayer.SetLayerType(OSMTileType.None);
            osmLayer.UpdateAll();
            canvasManager.DrawAll();
        }
        #endregion Кнопки изменения подложки
    }
}