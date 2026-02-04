using GIS.Classes.DrawObjects;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Documents;
using System.Windows.Media;

namespace GIS.Classes.Styles
{
    public abstract class LayerStyle : INotifyPropertyChanged
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

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
