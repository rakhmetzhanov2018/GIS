using GIS.Classes.DrawObjects;
using GIS.Classes.Factories;
using GIS.Classes.Main;
using GIS.Classes.Services;
using GIS.Classes.ViewModels;
using GIS.Windows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        private List<double[]> _tempLineGeoPoints = new();   // географические точки линии
        private List<List<double[]>> _tempPolygonGeoPoints = new(); // географические точки полигона (один внешний контур)

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
                _tempLineGeoPoints.Clear();
                _tempLineGeoPoints.Add(geoCoords);

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

            _tempLineGeoPoints.Add(geoCoords);
            demoPolyLine.Points.Add(finalPoint);
        }
        public void DrawPolygon(Point position)
        {
            var geoCoords = MapToCanvasTranslator.TranslateFromCanvasToGeo(position.X, position.Y);
            var finalPoint = MapToCanvasTranslator.TranslateFromGeoToCanvasFinal(geoCoords[0], geoCoords[1]);

            if (!IsDrawingPolygons)
            {
                _tempPolygonGeoPoints.Clear();
                _tempPolygonGeoPoints.Add(new List<double[]>());

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

            _tempPolygonGeoPoints[0].Add(geoCoords);

            drawingPolygon.Points.Add(finalPoint);
            drawingPolygon.Points.Add(finalPoint);
            demoPolygon.Points.Add(finalPoint);
        }

        public void UpdateTempFigures()
        {
            if (IsDrawingLines && _tempLineGeoPoints.Count > 0)
            {
                demoPolyLine.Points.Clear();
                foreach (var geoPoint in _tempLineGeoPoints)
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

            if (IsDrawingPolygons && _tempPolygonGeoPoints.Count > 0 && _tempPolygonGeoPoints[0].Count > 0)
            {
                demoPolygon.Points.Clear();
                drawingPolygon.Points.Clear();

                foreach (var geoPoint in _tempPolygonGeoPoints[0])
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
                _tempLineGeoPoints.Clear();
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
                _tempPolygonGeoPoints.Clear();
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
                var newGeo = new GeoGraphicLineString(_tempLineGeoPoints); // нужно, чтобы конструктор принимал List<double[]>
                CreateAttributeWindow(newGeo);

                _tempLineGeoPoints.Clear();

                IsDrawingLines = false;
                mapCanvas.Children.Remove(drawingLine);
                mapCanvas.Children.Remove(demoPolyLine);
            }
            else if (IsDrawingPolygons)
            {
                var newGeo = new GeoGraphicPolygon(_tempPolygonGeoPoints);
                CreateAttributeWindow(newGeo);

                _tempPolygonGeoPoints.Clear();
                IsDrawingPolygons = false;
                mapCanvas.Children.Remove(drawingPolygon);
                mapCanvas.Children.Remove(demoPolygon);
            }
        }
    }
}
