using GIS.Classes.Main;
using GIS.Classes.Managers;
using GIS.Classes.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace GIS.Services
{
    public class CanvasManager
    {
        private Canvas mapCanvas;
        private DrawingService drawingService;
        private ObservableCollection<Layer> layersList;

        public bool IsDrawingLines => drawingService.IsDrawingLines;
        public bool IsDrawingPolygons => drawingService.IsDrawingPolygons;

        public CanvasManager(Canvas canvas, ObservableCollection<Layer> layers)
        {
            mapCanvas = canvas;
            layersList = layers;
            drawingService = new DrawingService(mapCanvas);
        }
        public void SetSelectedLayer(Layer layer)
        {
            drawingService.SetSelectedLayer(layer);
        }
        public void DrawPoint(Point position)
        {
            drawingService.DrawPoint(position);
        }
        public void DrawLine(Point position)
        {
            drawingService.DrawLine(position);
        }
        public void DrawPolygon(Point position)
        {
            drawingService.DrawPolygon(position);
        }
        public void UpdateDrawingLine(Point position)
        {
            drawingService.UpdateDrawingLine(position);
        }
        public void UpdateDrawingPolygon(Point position)
        {
            drawingService.UpdateDrawingPolygon(position);
        }
        public void EndDrawing()
        {
            drawingService.EndDrawing();
        }
        public void CancelDrawing()
        {
            drawingService.CancelDrawing();
        }

        public void DrawAll()
        {
            for (int i = layersList.Count - 1; i >= 0; i--)
            {
                layersList[i].DrawAll(mapCanvas);
            }

            ApplyStylesForAllLayers();
        }
        private void ApplyStylesForAllLayers()
        {
            foreach (var layer in layersList)
            {
                layer.ApplyStyleToAllFeatures();
            }
        }
        public void UpdateAll()
        {
            foreach (var layer in layersList)
            {
                layer.UpdateAll();
            }
        }
        public void ZoomToLayer(Layer layer)
        {
            GeoBounds bounds = layer.Bounds;
            double width = bounds.MaxLon - bounds.MinLon;
            double height = bounds.MaxLat - bounds.MinLat;
            const double paddingPercent = 0.15;

            double centerLon = (bounds.MinLon + bounds.MaxLon) / 2;
            double centerLat = (bounds.MinLat + bounds.MaxLat) / 2;

            double halfWidthDeg;
            double halfHeightDeg;

            if (width == 0 && height == 0)
            {
                const double fixedRadiusDeg = 0.05;
                halfWidthDeg = fixedRadiusDeg;
                halfHeightDeg = fixedRadiusDeg;
            }
            else
            {
                halfWidthDeg = (width / 2) * (1 + paddingPercent);
                halfHeightDeg = (height / 2) * (1 + paddingPercent);
            }

            double canvasAspect = MapToCanvasTranslator.CanvasSize.Width / MapToCanvasTranslator.CanvasSize.Height;
            double currentAspect = halfWidthDeg / halfHeightDeg;

            if (currentAspect < canvasAspect)
            {
                halfWidthDeg = halfHeightDeg * canvasAspect;
            }
            else if (currentAspect > canvasAspect)
            {
                halfHeightDeg = halfWidthDeg / canvasAspect;
            }

            GeoBounds newBounds = new GeoBounds(centerLon - halfWidthDeg, centerLon + halfWidthDeg,
                                                centerLat - halfHeightDeg, centerLat + halfHeightDeg);
            if (newBounds.MaxLon - newBounds.MinLon <= 0) newBounds.MaxLon = newBounds.MinLon + 0.01;
            if (newBounds.MaxLat - newBounds.MinLat <= 0) newBounds.MaxLat = newBounds.MinLat + 0.01;

            MapToCanvasTranslator.Bounds = newBounds;
            MapToCanvasTranslator.ResetGlobalOffsets();
            MapToCanvasTranslator.CalculateRatios();
            DrawAll();
        }
        public void HandleMoveMode(Point currentMousePoint, MouseEventArgs e,
            ref bool isLeftMouseButtonDown, ref Point leftMouseButtonDownPoint)
        {
            var offset = currentMousePoint - leftMouseButtonDownPoint;

            if (isLeftMouseButtonDown && e.LeftButton == MouseButtonState.Pressed)
            {
                MapToCanvasTranslator.GlobalOffsetX += offset.X;
                MapToCanvasTranslator.GlobalOffsetY += offset.Y;
                UpdateAll();
                leftMouseButtonDownPoint = currentMousePoint;
            }
            else if (e.LeftButton != MouseButtonState.Pressed)
            {
                isLeftMouseButtonDown = false;
                mapCanvas.Cursor = Cursors.Arrow;
            }
        }
        public void HandleMouseWheel(Point mousePos, int delta)
        {
            double scaleDelta = delta > 0 ? 1.1 : 1 / 1.1;

            MapToCanvasTranslator.GlobalOffsetX =
                mousePos.X - (mousePos.X - MapToCanvasTranslator.GlobalOffsetX) * scaleDelta;
            MapToCanvasTranslator.GlobalOffsetY =
                mousePos.Y - (mousePos.Y - MapToCanvasTranslator.GlobalOffsetY) * scaleDelta;
            MapToCanvasTranslator.GlobalScale *= scaleDelta;

            UpdateAll();
        }

        public void UpdateTempFigures()
        {
            drawingService.UpdateTempFigures();
        }


        public void StartMeasureDistance() => drawingService.StartMeasureDistance();
        public void AddDistancePoint(Point point) => drawingService.AddDistancePoint(point);
        public void UpdateDistanceRubber(Point point) => drawingService.UpdateMouseDistance(point);
        public void EndMeasureDistance() => drawingService.EndMeasureDistance();

        public void StartMeasureArea() => drawingService.StartMeasureArea();
        public void AddAreaPoint(Point point) => drawingService.AddAreaPoint(point);
        public void UpdateAreaRubber(Point point) => drawingService.UpdateMouseArea(point);
        public void EndMeasureArea() => drawingService.EndMeasureArea();

        public void CancelMeasure() => drawingService.CancelMeasure();
        public void StopMeasuring() => drawingService.StopMeasuring();

        public event Action<string> MeasureStatusMessage { 
            add => drawingService.StatusMessage += value; remove => drawingService.StatusMessage -= value; 
        }
    }
}