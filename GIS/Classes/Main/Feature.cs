using GIS.Classes.DrawObjects;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace GIS.Classes.Main
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
        public Dictionary<string, string> props;

        public event PropertyChangedEventHandler? PropertyChanged;

        public GeoGraphicObject Geometry { get; set; }
        public Feature(GeoGraphicObject geo, Dictionary<string, string> props)
        {
            Geometry = geo;
            this.props = props;

            if (props.ContainsKey("name") && string.IsNullOrWhiteSpace(props["name"]))
            {
                Name = props["name"];
            }
            else
            {
                Name = Geometry switch
                {
                    GeoGraphicPoint => $"Точка {Guid.NewGuid().ToString().Substring(0, 6)}",
                    GeoGraphicLineString => $"Линия {Guid.NewGuid().ToString().Substring(0, 6)}",
                    GeoGraphicPolygon => $"Полигон {Guid.NewGuid().ToString().Substring(0, 6)}",
                    _ => throw new Exception("Проблема в типе объекта")
                };

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
            System.Diagnostics.Debug.WriteLine($"UpdateHighlightFeature: isSelected={isSelected}, Figure={Geometry.Figure?.GetType()}");
            if (isSelected && Geometry.Figure is Shape shape)
            {
                shape.StrokeDashArray = new DoubleCollection { 2, 1 };
                shape.Effect = new DropShadowEffect
                {
                    Color = Colors.DarkRed,
                    BlurRadius = 5
                };

                Panel.SetZIndex(shape, 100);
                System.Diagnostics.Debug.WriteLine("  -> HIGHLIGHTED");
            }
            else
            {
                Geometry.Figure.StrokeDashArray = null;
                Geometry.Figure.Effect = null;

                Panel.SetZIndex(Geometry.Figure, 0);
                System.Diagnostics.Debug.WriteLine("  -> UNHIGHLIGHTED");
            }
        }
    }
}
