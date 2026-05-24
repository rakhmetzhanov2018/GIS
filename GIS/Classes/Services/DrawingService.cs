using GIS.Classes.DrawObjects;
using GIS.Classes.Main;
using GIS.Classes.Services;
using GIS.Classes.Styles;
using GIS.Classes.ViewModels;
using GIS.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GIS.Classes.Managers
{
    public class DrawingService
    {
        private Canvas mapCanvas;

        public bool IsDrawingLines { get; private set; } = false;
        public bool IsDrawingPolygons { get; private set; } = false;
        private List<Point> points;

        private Layer selectedLayer;

        private Line drawingLine;
        private Polygon drawingPolygon;

        private Polyline demoPolyLine;
        private Polygon demoPolygon;

        private List<double[]> tempLineGeoPoints = new();
        private List<List<double[]>> tempPolygonGeoPoints = new();


        public bool IsMeasuringDistance { get; private set; } = false;
        private List<Point> distancePoints = new();
        private Polyline tempRulerLine;
        private Line mouseLine;
        private TextBlock distanceLabel;
        private double metersPerPixel;

        public event Action<string> StatusMessage;


        public DrawingService(Canvas mapCanvas)
        {
            this.mapCanvas = mapCanvas;

            points = new List<Point>();

            CreateDrawingFigures();

        }

        private void CreateDrawingFigures()
        {
            drawingLine = new Line
            {
                Stroke = Brushes.Gray,
                StrokeThickness = 4,
                Opacity = 0.5
            };

            drawingPolygon = new Polygon
            {
                Stroke = Brushes.Black,
                StrokeThickness = 4,
                Fill = Brushes.Gray,
                Opacity = 0.5
            };

            Canvas.SetZIndex(drawingLine, 101);
            Canvas.SetZIndex(drawingPolygon, 101);
        }
        private void CreateAttributeWindow(GeoGraphicObject newGeo)
        {
            var viewModel = new DrawnObjectPropertiesViewModel(selectedLayer);
            var window = new DrawnObjectPropertiesWindow(viewModel);

            var props = new Dictionary<string, string>();

            if (viewModel.AttributeFields.Count == 0)
            {
                AddFeatureToLayer(newGeo, props);
            }
            else if (window.ShowDialog() == true)
            {
                foreach (var field in viewModel.AttributeFields)
                {
                    props[field.Name] = field.Value;
                }
                AddFeatureToLayer(newGeo, props);
            }
        }

        private void AddFeatureToLayer(GeoGraphicObject newGeo, Dictionary<string, string> props)
        {
            var feature = new Feature(newGeo, props);
            selectedLayer.AddObject(feature);

            if (selectedLayer.LayerStyle == null)
                selectedLayer.LayerStyle = DefaultStyleFactory.CreateDefaultStyle(selectedLayer.GeoType);
            selectedLayer.LayerStyle.ApplyToFeature(feature);

            feature.DrawFigure(mapCanvas);
            feature.SetVisibility(selectedLayer.IsVisible);

            var layerBounds = selectedLayer.Bounds;
            feature.Geometry.GetBounds(ref layerBounds);
            selectedLayer.Bounds = layerBounds;

        }

        public void SetSelectedLayer(Layer selectedLayer)
        {
            this.selectedLayer = selectedLayer;
        }

        public void DrawPoint(Point position)
        {
            var geoCoords = MapToCanvasTranslator.TranslateFromCanvasToGeo(position.X, position.Y);

            GeoGraphicPoint newGeo = new GeoGraphicPoint(geoCoords[0], geoCoords[1]);

            CreateAttributeWindow(newGeo);
        }
        public void DrawLine(Point position)
        {
            var geoCoords = MapToCanvasTranslator.TranslateFromCanvasToGeo(position.X, position.Y);
            var finalPoint = MapToCanvasTranslator.TranslateFromGeoToCanvasFinal(geoCoords[0], geoCoords[1]);

            drawingLine.X1 = position.X;
            drawingLine.Y1 = position.Y;

            if (!IsDrawingLines)
            {
                tempLineGeoPoints.Clear();
                tempLineGeoPoints.Add(geoCoords);

                demoPolyLine = new Polyline
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = 4,
                    Opacity = 0.5
                };

                Canvas.SetZIndex(demoPolyLine, 100);


                IsDrawingLines = true;
                mapCanvas.Children.Add(drawingLine);
                mapCanvas.Children.Add(demoPolyLine);
            }

            tempLineGeoPoints.Add(geoCoords);
            demoPolyLine.Points.Add(finalPoint);
        }
        public void DrawPolygon(Point position)
        {
            var geoCoords = MapToCanvasTranslator.TranslateFromCanvasToGeo(position.X, position.Y);
            var finalPoint = MapToCanvasTranslator.TranslateFromGeoToCanvasFinal(geoCoords[0], geoCoords[1]);

            if (!IsDrawingPolygons)
            {
                tempPolygonGeoPoints.Clear();
                tempPolygonGeoPoints.Add(new List<double[]>());

                demoPolygon = new Polygon
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = 0,
                    Fill = Brushes.Gray,
                    Opacity = 0.5
                };

                drawingPolygon = new Polygon
                {
                    Stroke = Brushes.Black,
                    StrokeDashArray = new DoubleCollection { 3, 2 },
                    StrokeThickness = 1,
                    Fill = Brushes.Gray,
                    Opacity = 0.3
                };

                Canvas.SetZIndex(drawingPolygon, 101);
                Canvas.SetZIndex(demoPolygon, 100);

                IsDrawingPolygons = true;
                mapCanvas.Children.Add(drawingPolygon);
                mapCanvas.Children.Add(demoPolygon);
            }

            tempPolygonGeoPoints[0].Add(geoCoords);

            drawingPolygon.Points.Add(finalPoint);
            drawingPolygon.Points.Add(finalPoint);
            demoPolygon.Points.Add(finalPoint);
        }

        public void UpdateTempFigures()
        {
            if (IsDrawingLines && tempLineGeoPoints.Count > 0)
            {
                demoPolyLine.Points.Clear();
                foreach (var geoPoint in tempLineGeoPoints)
                {
                    var finalPoint = MapToCanvasTranslator.TranslateFromGeoToCanvasFinal(geoPoint[0], geoPoint[1]);
                    demoPolyLine.Points.Add(finalPoint);
                }

                // Обновляем резиновую линию
                if (demoPolyLine.Points.Count > 1)
                {
                    drawingLine.X1 = demoPolyLine.Points[demoPolyLine.Points.Count - 2].X;
                    drawingLine.Y1 = demoPolyLine.Points[demoPolyLine.Points.Count - 2].Y;
                    drawingLine.X2 = demoPolyLine.Points[demoPolyLine.Points.Count - 1].X;
                    drawingLine.Y2 = demoPolyLine.Points[demoPolyLine.Points.Count - 1].Y;
                }
            }

            if (IsDrawingPolygons && tempPolygonGeoPoints.Count > 0 && tempPolygonGeoPoints[0].Count > 0)
            {
                demoPolygon.Points.Clear();
                drawingPolygon.Points.Clear();

                foreach (var geoPoint in tempPolygonGeoPoints[0])
                {
                    var finalPoint = MapToCanvasTranslator.TranslateFromGeoToCanvasFinal(geoPoint[0], geoPoint[1]);
                    demoPolygon.Points.Add(finalPoint);
                    drawingPolygon.Points.Add(finalPoint);
                    drawingPolygon.Points.Add(finalPoint); // для штриховой линии
                }
            }
        }


        public void UpdateDrawingLine(Point position)
        {
            var geoCoords = MapToCanvasTranslator.TranslateFromCanvasToGeo(position.X, position.Y);
            var finalPoint = MapToCanvasTranslator.TranslateFromGeoToCanvasFinal(geoCoords[0], geoCoords[1]);
            drawingLine.X2 = finalPoint.X;
            drawingLine.Y2 = finalPoint.Y;
        }
        public void UpdateDrawingPolygon(Point position)
        {
            var geoCoords = MapToCanvasTranslator.TranslateFromCanvasToGeo(position.X, position.Y);
            var finalPoint = MapToCanvasTranslator.TranslateFromGeoToCanvasFinal(geoCoords[0], geoCoords[1]);
            if (drawingPolygon.Points.Count > 0) drawingPolygon.Points[drawingPolygon.Points.Count - 1] = finalPoint;
        }

        public void CancelDrawing()
        {
            if (IsDrawingLines)
            {
                tempLineGeoPoints.Clear();
                IsDrawingLines = false;

                if (drawingLine != null && mapCanvas.Children.Contains(drawingLine))
                    mapCanvas.Children.Remove(drawingLine);

                if (demoPolyLine != null && mapCanvas.Children.Contains(demoPolyLine))
                    mapCanvas.Children.Remove(demoPolyLine);

                if (demoPolyLine != null)
                    demoPolyLine.Points.Clear();
            }

            if (IsDrawingPolygons)
            {
                tempPolygonGeoPoints.Clear();
                IsDrawingPolygons = false;

                if (drawingPolygon != null && mapCanvas.Children.Contains(drawingPolygon))
                    mapCanvas.Children.Remove(drawingPolygon);

                if (demoPolygon != null && mapCanvas.Children.Contains(demoPolygon))
                    mapCanvas.Children.Remove(demoPolygon);

                if (drawingPolygon != null)
                    drawingPolygon.Points.Clear();

                if (demoPolygon != null)
                    demoPolygon.Points.Clear();
            }

            points.Clear();

            CreateDrawingFigures();
        }

        public void EndDrawing()
        {
            if (IsDrawingLines)
            {
                var newGeo = new GeoGraphicLineString(tempLineGeoPoints);
                CreateAttributeWindow(newGeo);

                tempLineGeoPoints.Clear();

                IsDrawingLines = false;
                mapCanvas.Children.Remove(drawingLine);
                mapCanvas.Children.Remove(demoPolyLine);
            }
            else if (IsDrawingPolygons)
            {
                var newGeo = new GeoGraphicPolygon(tempPolygonGeoPoints);
                CreateAttributeWindow(newGeo);

                tempPolygonGeoPoints.Clear();
                IsDrawingPolygons = false;
                mapCanvas.Children.Remove(drawingPolygon);
                mapCanvas.Children.Remove(demoPolygon);
            }
        }

       

        public void StartMeasureDistance()
        {
            CancelDrawing();        
            CancelMeasure();       
            IsMeasuringDistance = true;
            distancePoints.Clear();
            metersPerPixel = GetMetersPerPixel();

            tempRulerLine = new Polyline { Stroke = Brushes.Red, StrokeThickness = 2, StrokeDashArray = new DoubleCollection { 4, 2 } };
            mouseLine = new Line { Stroke = Brushes.Red, StrokeThickness = 2, StrokeDashArray = new DoubleCollection { 4, 2 } };
            distanceLabel = new TextBlock { Foreground = Brushes.Red, FontSize = 12, Background = Brushes.White, Padding = new Thickness(2) };

            Canvas.SetZIndex(tempRulerLine, 200);
            Canvas.SetZIndex(mouseLine, 200);
            Canvas.SetZIndex(distanceLabel, 200);

            mapCanvas.Children.Add(tempRulerLine);
            mapCanvas.Children.Add(mouseLine);
            mapCanvas.Children.Add(distanceLabel);
        }

        public void AddDistancePoint(Point canvasPoint)
        {
            if (!IsMeasuringDistance) return;


            if (distancePoints.Count == 0)
            {
                if (tempRulerLine != null) tempRulerLine.Visibility = Visibility.Visible;
                if (mouseLine != null) mouseLine.Visibility = Visibility.Visible;
                if (distanceLabel != null) distanceLabel.Visibility = Visibility.Visible;
            }


            distancePoints.Add(canvasPoint);
            tempRulerLine.Points.Clear();
            foreach (var p in distancePoints)
                tempRulerLine.Points.Add(p);
        }

        public void UpdateMouseDistance(Point currentPoint)
        {
            if (!IsMeasuringDistance) return;
            if (distancePoints.Count == 0) return;

            mouseLine.X1 = distancePoints.Last().X;
            mouseLine.Y1 = distancePoints.Last().Y;
            mouseLine.X2 = currentPoint.X;
            mouseLine.Y2 = currentPoint.Y;

            double totalMeters = 0;
            for (int i = 0; i < distancePoints.Count - 1; i++)
            {
                double dx = distancePoints[i + 1].X - distancePoints[i].X;
                double dy = distancePoints[i + 1].Y - distancePoints[i].Y;
                totalMeters += Math.Sqrt(dx * dx + dy * dy) * metersPerPixel;
            }
            double lastSegMeters = 0;
            if (distancePoints.Count > 0)
            {
                var last = distancePoints.Last();
                double dx = currentPoint.X - last.X;
                double dy = currentPoint.Y - last.Y;
                lastSegMeters = Math.Sqrt(dx * dx + dy * dy) * metersPerPixel;
            }
            double total = totalMeters + lastSegMeters;

            distanceLabel.Text = $"Длина: {total:F1} м";
            Canvas.SetLeft(distanceLabel, currentPoint.X + 10);
            Canvas.SetTop(distanceLabel, currentPoint.Y - 20);
        }

        public void EndMeasureDistance()
        {
            if (IsMeasuringDistance && distancePoints.Count >= 2)
            {
                double totalMeters = 0;
                for (int i = 0; i < distancePoints.Count - 1; i++)
                {
                    double dx = distancePoints[i + 1].X - distancePoints[i].X;
                    double dy = distancePoints[i + 1].Y - distancePoints[i].Y;
                    totalMeters += Math.Sqrt(dx * dx + dy * dy) * metersPerPixel;
                }
                StatusMessage?.Invoke($"Измеренная длина: {totalMeters:F1} м");
            }
            CancelMeasure();
        }


        public bool IsMeasuringArea { get; private set; } = false;
        private List<Point> _areaPoints = new();
        private Polygon tempAreaPolygon;
        private Polygon areaMousePolygon;
        private TextBlock areaLabel;
        private double _areaMetersPerPixel;

        public void StartMeasureArea()
        {
            CancelDrawing();
            CancelMeasure();
            IsMeasuringArea = true;
            _areaPoints.Clear();
            _areaMetersPerPixel = GetMetersPerPixel();

            tempAreaPolygon = new Polygon { Stroke = Brushes.Red, StrokeThickness = 2, StrokeDashArray = new DoubleCollection { 4, 2 }, Fill = new SolidColorBrush(Color.FromArgb(50, 255, 0, 0)) };
            areaMousePolygon = new Polygon { Stroke = Brushes.Red, StrokeThickness = 2, StrokeDashArray = new DoubleCollection { 4, 2 }, Fill = new SolidColorBrush(Color.FromArgb(30, 255, 0, 0)) };
            areaLabel = new TextBlock { Foreground = Brushes.Red, FontSize = 12, Background = Brushes.White, Padding = new Thickness(2) };

            Canvas.SetZIndex(tempAreaPolygon, 200);
            Canvas.SetZIndex(areaMousePolygon, 200);
            Canvas.SetZIndex(areaLabel, 200);

            mapCanvas.Children.Add(tempAreaPolygon);
            mapCanvas.Children.Add(areaMousePolygon);
            mapCanvas.Children.Add(areaLabel);

            _areaMetersPerPixel = GetMetersPerPixel();
        }

        public void AddAreaPoint(Point canvasPoint)
        {
            if (!IsMeasuringArea) return;


            if (distancePoints.Count == 0)
            {
                if (tempAreaPolygon != null) tempAreaPolygon.Visibility = Visibility.Visible;
                if (areaMousePolygon != null) areaMousePolygon.Visibility = Visibility.Visible;
                if (areaLabel != null) areaLabel.Visibility = Visibility.Visible;
            }


            _areaPoints.Add(canvasPoint);
            tempAreaPolygon.Points.Clear();
            foreach (var p in _areaPoints)
                tempAreaPolygon.Points.Add(p);
        }

        public void UpdateMouseArea(Point currentPoint)
        {
            if (!IsMeasuringArea) return;
            if (_areaPoints.Count == 0) return;

            var tempPoints = new List<Point>(_areaPoints);
            tempPoints.Add(currentPoint);
            areaMousePolygon.Points.Clear();
            foreach (var p in tempPoints)
                areaMousePolygon.Points.Add(p);

            double areaSqMeters = 0;
            if (_areaPoints.Count >= 2)
                areaSqMeters = ComputeAreaInMeters(tempPoints);


            areaLabel.Text = $"Площадь: {areaSqMeters:F1} м²";
            Canvas.SetLeft(areaLabel, currentPoint.X + 10);
            Canvas.SetTop(areaLabel, currentPoint.Y - 20);
        }

        public void EndMeasureArea()
        {
            if (IsMeasuringArea && _areaPoints.Count >= 3)
            {
                double area = ComputeAreaInMeters(_areaPoints);
                StatusMessage?.Invoke($"Измеренная площадь: {area:F1} м²");
            }
            CancelMeasure();
        }

        private double GetMetersPerPixel()
        {
            double centerX = mapCanvas.ActualWidth / 2;
            double centerY = mapCanvas.ActualHeight / 2;
            var geo = MapToCanvasTranslator.TranslateFromCanvasToGeo(centerX, centerY);
            var geo2 = MapToCanvasTranslator.TranslateFromCanvasToGeo(centerX + 1, centerY);
            return DistanceInMeters(geo[0], geo[1], geo2[0], geo2[1]);
        }

        private double ComputeAreaInMeters(List<Point> points)
        {
            if (points.Count < 3) return 0;
            double areaPixels = 0;
            for (int i = 0; i < points.Count; i++)
            {
                int j = (i + 1) % points.Count;
                areaPixels += points[i].X * points[j].Y - points[j].X * points[i].Y;
            }
            areaPixels = Math.Abs(areaPixels) / 2.0;
            return areaPixels * _areaMetersPerPixel * _areaMetersPerPixel;
        }

        private double DistanceInMeters(double lon1, double lat1, double lon2, double lat2)
        {
            const double R = 6371000;
            double dLat = (lat2 - lat1) * Math.PI / 180;
            double dLon = (lon2 - lon1) * Math.PI / 180;
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }


        public void CancelMeasure()
        {
            distancePoints.Clear();
            _areaPoints.Clear();

            if (tempRulerLine != null) tempRulerLine.Visibility = Visibility.Collapsed;
            if (mouseLine != null) mouseLine.Visibility = Visibility.Collapsed;
            if (distanceLabel != null) distanceLabel.Visibility = Visibility.Collapsed;
            if (tempAreaPolygon != null) tempAreaPolygon.Visibility = Visibility.Collapsed;
            if (areaMousePolygon != null) areaMousePolygon.Visibility = Visibility.Collapsed;
            if (areaLabel != null) areaLabel.Visibility = Visibility.Collapsed;

        }

        public void StopMeasuring()
        {
            IsMeasuringDistance = false;
            IsMeasuringArea = false;

            if (tempRulerLine != null) mapCanvas.Children.Remove(tempRulerLine);
            if (mouseLine != null) mapCanvas.Children.Remove(mouseLine);
            if (distanceLabel != null) mapCanvas.Children.Remove(distanceLabel);
            if (tempAreaPolygon != null) mapCanvas.Children.Remove(tempAreaPolygon);
            if (areaMousePolygon != null) mapCanvas.Children.Remove(areaMousePolygon);
            if (areaLabel != null) mapCanvas.Children.Remove(areaLabel);

            tempRulerLine = null;
            mouseLine = null;
            distanceLabel = null;
            tempAreaPolygon = null;
            areaMousePolygon = null;
            areaLabel = null;

            distancePoints.Clear();
            _areaPoints.Clear();
        }
    }
}
