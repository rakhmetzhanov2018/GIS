using GIS.Classes.DrawObjects;
using GIS.Classes.OtherObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
                    VisibilityChanged?.Invoke(this, EventArgs.Empty);
                }
            } 
        }
        public List<Feature> ObjectList { get; } = new();
        public GeoBounds Bounds { get; set; }

        public Layer() { }
        public Layer(string name, Boolean isVisible = true)
        {
            Name = name;
            IsVisible = isVisible;
        }

        public event EventHandler VisibilityChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

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
        public void CreateAll()
        {
            foreach (Feature feature in ObjectList)
            {
                feature.CreateFigure();
            }
        }
        public void DrawAll(Canvas canvas)
        {
            foreach (Feature feature in ObjectList)
            {
                feature.DrawFigure(canvas);
            }
        }
        public void UpdateAll(double offsetX, double offsetY, double scale)
        {
            foreach (Feature feature in ObjectList)
            {
                feature.UpdateFigure(offsetX, offsetY, scale);
            }
        }
    }
}
