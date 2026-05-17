using GIS.Classes.Main;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace GIS.Classes.ViewModels
{
    public class RasterLayerSettingsViewModel : ViewModelBase
    {
        private readonly RasterLayer rasterLayer;

        private string layerName;
        private bool isVisible;
        private double opacity = 1.0;

        private double minLon;
        private double maxLon;
        private double minLat;
        private double maxLat;

        public string LayerName
        {
            get => layerName;
            set => SetField(ref layerName, value);
        }

        public bool IsVisible
        {
            get => isVisible;
            set => SetField(ref isVisible, value);
        }

        public double Opacity
        {
            get => opacity;
            set => SetField(ref opacity, value);
        }

        public double MinLon
        {
            get => minLon;
            set => SetField(ref minLon, value);
        }

        public double MaxLon
        {
            get => maxLon;
            set => SetField(ref maxLon, value);
        }

        public double MinLat
        {
            get => minLat;
            set => SetField(ref minLat, value);
        }

        public double MaxLat
        {
            get => maxLat;
            set => SetField(ref maxLat, value);
        }

        public ICommand ApplyCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand CalibrateCommand { get; }

        public event EventHandler<bool> CloseWindow;
        public event EventHandler StartCalibration;

        public RasterLayerSettingsViewModel(RasterLayer layer)
        {
            rasterLayer = layer;
            LayerName = layer.Name;
            IsVisible = layer.IsVisible;
            Opacity = layer.RasterImage.Opacity;

            MinLon = layer.Bounds.MinLon;
            MaxLon = layer.Bounds.MaxLon;
            MinLat = layer.Bounds.MinLat;
            MaxLat = layer.Bounds.MaxLat;

            ApplyCommand = new RelayCommand(Apply);
            CancelCommand = new RelayCommand(Cancel);
            CalibrateCommand = new RelayCommand(Calibrate);
        }

        private void Apply()
        {
            rasterLayer.Name = LayerName;
            rasterLayer.IsVisible = IsVisible;
            rasterLayer.RasterImage.Opacity = Opacity;

            var newBounds = new GeoBounds { MinLon = MinLon, MaxLon = MaxLon, MinLat = MinLat, MaxLat = MaxLat };
            rasterLayer.SetBounds(newBounds);
            CloseWindow.Invoke(this, true);
        }

        private void Cancel()
        {
            CloseWindow?.Invoke(this, false);
        }

        private void Calibrate()
        {
            StartCalibration?.Invoke(this, EventArgs.Empty);
        }

        protected new bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public RasterLayer GetRasterLayer()
        {
            return rasterLayer;
        }
    }
}
