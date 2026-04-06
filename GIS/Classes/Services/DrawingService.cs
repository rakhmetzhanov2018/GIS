using GIS.Classes.DrawObjects;
using GIS.Classes.Factories;
using GIS.Classes.Main;
using GIS.Classes.Services;
using GIS.Classes.ViewModels;
using GIS.Windows;
using System;
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
            };

            drawingPolygon = new Polygon
            {
                Stroke = Brushes.Black,
                StrokeThickness = 4,
                Fill = Brushes.Gray
            };

            Canvas.SetZIndex(drawingLine, 101);
            Canvas.SetZIndex(drawingPolygon, 101);
        }
        private void CreateAttributeWindow(GeoGraphicObject newGeo)
        {
            var viewModel = new DrawnObjectPropertiesViewModel(selectedLayer, newGeo, mapCanvas);
            var window = new DrawnObjectPropertiesWindow(viewModel);
            window.ShowDialog();
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
            drawingLine.X1 = position.X;
            drawingLine.Y1 = position.Y;

            if (!IsDrawingLines)
            {
                demoPolyLine = new Polyline
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = 4,
                };

                Canvas.SetZIndex(demoPolyLine, 100);
                

                IsDrawingLines = true;
                mapCanvas.Children.Add(drawingLine);
                mapCanvas.Children.Add(demoPolyLine);
            }

            demoPolyLine.Points.Add(position);
        }
        public void DrawPolygon(Point position)
        {
            if (!IsDrawingPolygons)
            {
                demoPolygon = new Polygon
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = 4,
                    Fill = Brushes.Gray
                };

                drawingPolygon = new Polygon
                {
                    Stroke = Brushes.Black,
                    StrokeDashArray = new DoubleCollection { 3, 2 },
                    StrokeThickness = 1,
                    Fill = Brushes.Gray
                };

                Canvas.SetZIndex(drawingPolygon, 101);
                Canvas.SetZIndex(demoPolygon, 100);

                IsDrawingPolygons = true;
                mapCanvas.Children.Add(drawingPolygon);
                mapCanvas.Children.Add(demoPolygon);
            }

            drawingPolygon.Points.Add(position);
            drawingPolygon.Points.Add(position);
            demoPolygon.Points.Add(position);
        }

        public void UpdateDrawingLine(Point position)
        {
            drawingLine.X2 = position.X;
            drawingLine.Y2 = position.Y;
        }
        public void UpdateDrawingPolygon(Point position)
        {
            drawingPolygon.Points[drawingPolygon.Points.Count - 1] = position;
        }
        public void EndDrawing()
        {
            if (IsDrawingLines)
            {
                IsDrawingLines = false;
                mapCanvas.Children.Remove(drawingLine);
                mapCanvas.Children.Remove(demoPolyLine);

                var newPointList = MapToCanvasTranslator.TranslateFromCanvasToGeo(demoPolyLine.Points.ToList());

                GeoGraphicLineString newGeo = new GeoGraphicLineString(newPointList);
                CreateAttributeWindow(newGeo);
            }
            else if (IsDrawingPolygons)
            {
                IsDrawingPolygons = false;
                mapCanvas.Children.Remove(drawingPolygon);
                mapCanvas.Children.Remove(demoPolygon);

                var newPointList = new List<List<double[]>>
                {
                    MapToCanvasTranslator.TranslateFromCanvasToGeo(demoPolygon.Points.ToList())
                };

                GeoGraphicPolygon newGeo = new GeoGraphicPolygon(newPointList);
                CreateAttributeWindow(newGeo);
            }
        }
    }
}
