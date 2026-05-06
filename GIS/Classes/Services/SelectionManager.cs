using GIS.Classes.Main;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GIS.Services
{
    public class SelectionManager
    {
        private Canvas mapCanvas;
        private ObservableCollection<Layer> layersList;
        private Rectangle selectionRectangle;
        private List<Feature> selectedFeatures = new();
        private Point _rectangleStartPoint;

        public SelectionManager(Canvas canvas, ObservableCollection<Layer> layers)
        {
            mapCanvas = canvas;
            layersList = layers;
            CreateSelectionRectangle();
        }

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
            mapCanvas.Children.Add(selectionRectangle);
            Canvas.SetZIndex(selectionRectangle, 100);
        }

        public void ClearSelection()
        {
            foreach (var layer in layersList)
            {
                foreach (var feature in layer.ObjectList)
                {
                    feature.IsSelected = false;
                }
                layer.IsSelected = false;
            }
            selectedFeatures.Clear();
        }

        public void SelectFeature(Feature feature)
        {
            ClearSelection();
            feature.IsSelected = true;
            selectedFeatures.Add(feature);
        }

        public void StartRectangleSelection(Point startPoint)
        {
            _rectangleStartPoint = startPoint;
            Canvas.SetLeft(selectionRectangle, startPoint.X);
            Canvas.SetTop(selectionRectangle, startPoint.Y);
            selectionRectangle.Width = 0;
            selectionRectangle.Height = 0;
            selectionRectangle.Visibility = Visibility.Visible;
        }

        public void UpdateRectangleSelection(Point currentPoint)
        {
            var width = Math.Abs(currentPoint.X - _rectangleStartPoint.X);
            var height = Math.Abs(currentPoint.Y - _rectangleStartPoint.Y);

            Canvas.SetLeft(selectionRectangle, Math.Min(currentPoint.X, _rectangleStartPoint.X));
            Canvas.SetTop(selectionRectangle, Math.Min(currentPoint.Y, _rectangleStartPoint.Y));

            selectionRectangle.Width = width;
            selectionRectangle.Height = height;
        }

        public void EndRectangleSelection()
        {
            selectionRectangle.Visibility = Visibility.Collapsed;

            if (selectionRectangle.Width < 5 && selectionRectangle.Height < 5)
                return;

            var selectedFeaturesList = FindSelectedFeatures();

            foreach (var feature in selectedFeaturesList)
            {
                feature.IsSelected = true;
                selectedFeatures.Add(feature);
            }
        }

        private List<Feature> FindSelectedFeatures()
        {
            List<Feature> foundFeatures = new List<Feature>();

            Rect selectionArea = new Rect
            {
                X = Canvas.GetLeft(selectionRectangle),
                Y = Canvas.GetTop(selectionRectangle),
                Width = selectionRectangle.Width,
                Height = selectionRectangle.Height
            };

            foreach (Layer layer in layersList)
            {
                if (!layer.IsVisible) continue;
                foreach (Feature feature in layer.ObjectList)
                {
                    if (IsFigureInArea(feature.Geometry.Figure, selectionArea))
                    {
                        foundFeatures.Add(feature);
                    }
                }
            }

            return foundFeatures;
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

            if (figureGeo != null)
            {
                return rectGeo.FillContainsWithDetail(figureGeo) != IntersectionDetail.Empty;
            }

            return false;
        }

        public List<Feature> GetSelectedFeatures() => selectedFeatures;
    }
}