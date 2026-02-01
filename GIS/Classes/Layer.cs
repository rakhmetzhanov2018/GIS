using GIS.Classes.DrawObjects;
using GIS.Classes.OtherObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GIS.Classes
{
    internal class Layer : INotifyPropertyChanged
    {
        private bool isVisible = true;
        public string Name { get; set; } = "Новый слой";
        public Boolean IsVisible
        { get => isVisible;
            set 
            {
                if (isVisible != value)
                {
                    isVisible = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(isVisible)));
                    UpdateVisibility();
                }
            } 
        }
        public List<Feature> ObjectList { get; } = new();
        public GeoBounds Bounds { get; set; }
        public int ZIndex { get; set; } = 1;

        public Layer() { }
        public Layer(string name, Boolean isVisible = true)
        {
            Name = name;
            IsVisible = isVisible;
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;

        public void UpdateVisibility()
        {
            foreach (Feature feature in ObjectList)
            {
                feature.SetVisibility(IsVisible);
            }
        }
        public void AddObject(Feature ob)
        {
            ObjectList.Add(ob);
        }
        public void DeleteObject(Feature ob)
        {
            ObjectList.Remove(ob);
        }
        public override string ToString()
        {
            return $"{Name}";
        }
        public void DrawAll(Canvas canvas)
        {
            foreach (Feature feature in ObjectList)
            {
                feature.DrawFigure(canvas);
                feature.SetVisibility(IsVisible);
            }
        }
        public void UpdateAll()
        {
            foreach (Feature feature in ObjectList)
            {
                feature.UpdateFigure();
            }
        }
        public string GetStatistics()
        {
            int pointCount = 0;
            int lineCount = 0;
            int polygonCount = 0;

            foreach (Feature feature in ObjectList)
            {
                switch (feature.Geometry)
                {
                    case GeoGraphicPoint:
                        pointCount++;
                        break;
                    case GeoGraphicLineString:
                        lineCount++;
                        break;
                    case GeoGraphicPolygon:
                        polygonCount++;
                        break;
                }
            }

            return $"GeoGraphicPoint - {pointCount}, GeoGraphicLineString - {lineCount}, GeoGraphicPolygon - {polygonCount}";
        }
    }
}
