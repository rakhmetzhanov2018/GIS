using GIS.Classes.Main;
using GIS.Services;
using System;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace GIS.Classes.ViewModels
{
    public class BufferAnalysisViewModel : ViewModelBase
    {
        private readonly LayerManager layerManager;
        private Layer selectedLayer;
        private string radiusM;

        public ObservableCollection<Layer> Layers { get; }
        public Layer SelectedLayer { get => selectedLayer; set => SetField(ref selectedLayer, value); }
        public string Radius { get => radiusM; set => SetField(ref radiusM, value); }

        public ICommand CreateCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler<bool> CloseWindow;

        public BufferAnalysisViewModel(LayerManager layerManager)
        {
            this.layerManager = layerManager;
            Layers = new ObservableCollection<Layer>(layerManager.layersList);
            CreateCommand = new RelayCommand(Create, CanCreate);
            CancelCommand = new RelayCommand(Cancel);
        }

        private bool CanCreate()
        {
            return SelectedLayer != null && double.TryParse(Radius, out var r) && r > 0;
        }

        private void Create()
        {
            double radius = double.Parse(Radius);
            var bufferLayer = AnalysisService.CreateBuffer(SelectedLayer, radius);
            if (bufferLayer != null)
            {
                layerManager.AddLayer(bufferLayer);
            }
            CloseWindow?.Invoke(this, true);
        }

        private void Cancel() => CloseWindow?.Invoke(this, false);

        protected new bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;

            field = value;
            OnPropertyChanged(propertyName);

            (CreateCommand as RelayCommand)?.RaiseCanExecuteChanged();

            return true;
        }
    }
}