using GIS.Classes.DrawObjects;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace GIS.Classes
{
    public class Feature : INotifyPropertyChanged
    {
        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    OnPropertyChanged();
                    UpdateHighlightFeature();
                }
            }
        }
        public string Name { get; set; } = "Без названия";
        public Dictionary<String, String> props;

        public event PropertyChangedEventHandler? PropertyChanged;

        public GeoGraphicObject Geometry { get; set; }
        public Feature(GeoGraphicObject geo, Dictionary<String, String> props)
        {
            Geometry = geo;
            this.props = props;

            if (props.ContainsKey("name"))
            {
                Name = props["name"];  
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void DrawFigure(Canvas canvas)
        {
            Geometry.Draw(canvas);

            Geometry.Figure.Tag = this;
        }
        public void UpdateFigure()
        {
            Geometry.Update();
        }
        public void SetVisibility(bool isVisible)
        {
            Geometry.SetVisibility(isVisible);
        }
        private void UpdateHighlightFeature()
        {
            if (isSelected && Geometry.Figure is Shape shape)
            {
                shape.StrokeDashArray = new DoubleCollection { 2, 1 };
                shape.Effect = new DropShadowEffect
                {
                    Color = Colors.DarkRed,
                    BlurRadius = 5
                };
            }
            else
            {
                Geometry.Figure.StrokeDashArray = null;
                Geometry.Figure.Effect = null;
            }
        }
    }
}
