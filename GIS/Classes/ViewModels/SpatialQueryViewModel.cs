using GIS.Classes.Main;
using GIS.Classes.Styles;
using GIS.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GIS.Classes.ViewModels
{
    public class SpatialQueryViewModel : ViewModelBase
    {
        private readonly LayerManager layerManager;
        private readonly SelectionManager selectionManager;

        private Layer sourceLayer;
        private Layer targetLayer;
        private string selectedOperation;
        private bool useAllObjects = true;
        private bool useSelectedOnly;
        private bool resultHighlight = true;
        private bool resultCreateLayer;
        private int selectedCount;
        private string selectedCountInfo;

        public ObservableCollection<Layer> SourceLayers { get; }
        public ObservableCollection<Layer> TargetLayers { get; }
        public ObservableCollection<string> Operations { get; } = new() { "Intersects", "Contains", "Within" };

        public Layer SourceLayer
        {
            get => sourceLayer;
            set
            {
                if (SetField(ref sourceLayer, value))
                    UpdateSelectedCount();
            }
        }
        public Layer TargetLayer { get => targetLayer; set => SetField(ref targetLayer, value); }
        public string SelectedOperation { get => selectedOperation; set => SetField(ref selectedOperation, value); }
        public bool UseAllObjects { get => useAllObjects; set => SetField(ref useAllObjects, value); }
        public bool UseSelectedOnly
        {
            get => useSelectedOnly;
            set
            {
                if (SetField(ref useSelectedOnly, value))
                    UpdateSelectedCount();
            }
        }
        public bool ResultHighlight { get => resultHighlight; set => SetField(ref resultHighlight, value); }
        public bool ResultCreateLayer { get => resultCreateLayer; set => SetField(ref resultCreateLayer, value); }
        public string SelectedCountInfo { get => selectedCountInfo; set => SetField(ref selectedCountInfo, value); }

        public ICommand ExecuteCommand { get; }
        public ICommand CancelCommand { get; }
        public event EventHandler<bool> CloseWindow;

        public SpatialQueryViewModel(LayerManager layerManager, SelectionManager selectionManager)
        {
            this.layerManager = layerManager;
            this.selectionManager = selectionManager;
            SourceLayers = new ObservableCollection<Layer>(layerManager.layersList.Where(l => l is not RasterLayer && l.ShowInTree));
            TargetLayers = new ObservableCollection<Layer>(layerManager.layersList.Where(l => l is not RasterLayer && l.ShowInTree));
            SelectedOperation = Operations.First();
            UseAllObjects = true;
            ResultHighlight = true;

            ExecuteCommand = new RelayCommand(Execute, CanExecute);
            CancelCommand = new RelayCommand(Cancel);

            this.selectionManager.SelectionChanged += (s, e) => UpdateSelectedCount();
        }

        public void UpdateSelectedCount()
        {
            if (SourceLayer == null)
            {
                SelectedCountInfo = "";
                return;
            }
            var selectedInLayer = selectionManager.GetSelectedFeatures()
                .Where(f => layerManager.FindLayerByFeature(f) == SourceLayer).Count();
            selectedCount = selectedInLayer;
            SelectedCountInfo = UseSelectedOnly ? $"Выделено: {selectedCount} объектов" : "";
            (ExecuteCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        private bool CanExecute()
        {
            if (SourceLayer == null || TargetLayer == null) return false;
            if (SourceLayer == TargetLayer) return false; // нельзя с самим собой
            if (UseSelectedOnly && selectedCount == 0) return false;
            return true;
        }

        private void Execute()
        {
            var resultFeatures = AnalysisService.SpatialQuery(
                SourceLayer,
                TargetLayer,
                SelectedOperation,
                UseSelectedOnly ? selectionManager.GetSelectedFeatures().Where(f => layerManager.FindLayerByFeature(f) == SourceLayer).ToList() : null);

            if (resultFeatures == null || resultFeatures.Count == 0)
            {
                MessageBox.Show("Не найдено объектов, удовлетворяющих условию запроса.",
                                "Пространственная выборка", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (ResultHighlight)
            {
                selectionManager.ClearSelection();
                foreach (var feature in resultFeatures)
                    feature.IsSelected = true;
                selectionManager.RaiseSelectionChanged();
            }
            else if (ResultCreateLayer)
            {
                var newLayer = AnalysisService.CreateLayerFromFeatures(resultFeatures,
                        $"{SourceLayer.Name}_пространственная_выборка_{Guid.NewGuid().ToString().Substring(0, 6)}");
                if (newLayer != null)
                {
                    newLayer.LayerStyle = SourceLayer.LayerStyle?.Clone() as LayerStyle;
                    newLayer.ApplyStyleToAllFeatures();
                    layerManager.AddLayer(newLayer);
                    CloseWindow?.Invoke(this, true);
                }
                else
                {
                    MessageBox.Show("Не удалось создать слой (возможно, пустой результат).",
                                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            (ExecuteCommand as RelayCommand)?.RaiseCanExecuteChanged();
            return true;
        }

        private void Cancel() => CloseWindow?.Invoke(this, false);

    }
}
