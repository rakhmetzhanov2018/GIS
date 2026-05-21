using GIS.Classes.Main;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace GIS.Classes.Styles
{
    public abstract class LayerStyle : INotifyPropertyChanged, ICloneable
    {
        private double opacity = 1.0;
        private Color mainColor = Colors.Red;
        public Color MainColor
        {
            get => mainColor;
            set
            {
                if (value != mainColor)
                {
                    mainColor = value;
                    OnPropertyChanged();
                }
            }
        }
        public double Opacity
        {
            get => opacity;
            set
            {
                if (value != opacity)
                {
                    opacity = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public abstract void ApplyToFeature(Feature feature);
        public abstract LayerStyle Clone();

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
