using CommunityToolkit.Mvvm.Input;
using GIS.Classes.Main;
using GIS.Classes.Styles;
using GIS.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GIS.Classes.ViewModels
{
    public class AttributeQueryViewModel : ViewModelBase
    {
        private readonly LayerManager layerManager;
        private readonly SelectionManager selectionManager;
        private Layer selectedLayer;
        private string selectedAttribute;
        private string selectedOperator = "=";
        private string conditionValue;
        private bool resultHighlight = true;
        private bool resultCreateLayer;

        public ObservableCollection<Layer> Layers { get; }
        public ObservableCollection<string> AvailableAttributes { get; } = new();
        public ObservableCollection<string> Operators { get; } = new() { "=", "!=", ">", "<", ">=", "<=", "like" };

        public Layer SelectedLayer
        {
            get => selectedLayer;
            set
            {
                if (SetField(ref selectedLayer, value))
                {
                    UpdateAttributes();
                    (ExecuteCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string SelectedAttribute
        {
            get => selectedAttribute;
            set => SetField(ref selectedAttribute, value);
        }

        public string SelectedOperator
        {
            get => selectedOperator;
            set => SetField(ref selectedOperator, value);
        }

        public string ConditionValue
        {
            get => conditionValue;
            set => SetField(ref conditionValue, value);
        }

        public bool ResultHighlight
        {
            get => resultHighlight;
            set => SetField(ref resultHighlight, value);
        }

        public bool ResultCreateLayer
        {
            get => resultCreateLayer;
            set => SetField(ref resultCreateLayer, value);
        }

        public ICommand ExecuteCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler<bool> CloseWindow;

        public AttributeQueryViewModel(LayerManager layerManager, SelectionManager selectionManager)
        {
            this.layerManager = layerManager;
            this.selectionManager = selectionManager;
            Layers = new ObservableCollection<Layer>(layerManager.layersList.Where(l => l is not RasterLayer && l.ShowInTree));

            ExecuteCommand = new RelayCommand(Execute, CanExecute);
            CancelCommand = new RelayCommand(Cancel);

            UpdateAttributes();
        }

        private void UpdateAttributes()
        {
            AvailableAttributes.Clear();
            if (SelectedLayer?.FeatureProperties != null)
            {
                foreach (var prop in SelectedLayer.FeatureProperties)
                    AvailableAttributes.Add(prop.Name);
            }
            SelectedAttribute = null;
            ConditionValue = null;
        }

        private bool CanExecute()
        {
            if (SelectedLayer == null) return false;
            if (string.IsNullOrWhiteSpace(SelectedAttribute)) return false;
            if (SelectedOperator != "!=" && string.IsNullOrWhiteSpace(ConditionValue)) return false;
            return true;
        }

        private void Execute()
        {
            var result = new List<Feature>();
            foreach (var feature in SelectedLayer.ObjectList)
            {
                if (EvaluateFeature(feature))
                    result.Add(feature);
            }

            if (result.Count == 0)
            {
                MessageBox.Show("Нет объектов, удовлетворяющих условию.", "Атрибутивный запрос", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (ResultHighlight)
            {
                selectionManager.ClearSelection();
                foreach (var f in result)
                    f.IsSelected = true;
                selectionManager.RaiseSelectionChanged();
            }
            else if (ResultCreateLayer)
            {
                var newLayer = AnalysisService.CreateLayerFromFeatures(result,
                    $"{SelectedLayer.Name}_атрибутивная_выборка_{Guid.NewGuid().ToString().Substring(0, 6)}", SelectedLayer.LayerStyle);
                if (newLayer != null)
                {
                    newLayer.LayerStyle = SelectedLayer.LayerStyle?.Clone() as LayerStyle;
                    newLayer.ApplyStyleToAllFeatures();
                    layerManager.AddLayer(newLayer);
                }
                else
                {
                    MessageBox.Show("Не удалось создать слой.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                CloseWindow?.Invoke(this, true);
            }
            
        }

        private bool EvaluateFeature(Feature feature)
        {
            if (!feature.props.TryGetValue(SelectedAttribute, out string propValue))
                return false;

            string dataType = SelectedLayer.FeatureProperties.First(p => p.Name == SelectedAttribute).DataType;

            switch (SelectedOperator)
            {
                case "=":
                    if (dataType == "String")
                        return string.Equals(propValue, ConditionValue, StringComparison.InvariantCultureIgnoreCase);
                    else if (dataType == "Integer")
                        return int.TryParse(propValue, out int i1) && int.TryParse(ConditionValue, out int i2) && i1 == i2;
                    else if (dataType == "Double")
                        return double.TryParse(propValue, out double d1) && double.TryParse(ConditionValue, out double d2) && Math.Abs(d1 - d2) < 1e-9;
                    else if (dataType == "DateTime")
                        return DateTime.TryParse(propValue, out DateTime dt1) && DateTime.TryParse(ConditionValue, out DateTime dt2) && dt1 == dt2;
                    else if (dataType == "Boolean")
                        return bool.TryParse(propValue, out bool b1) && bool.TryParse(ConditionValue, out bool b2) && b1 == b2;
                    break;
                case "!=":
                    if (dataType == "String")
                        return !string.Equals(propValue, ConditionValue, StringComparison.InvariantCultureIgnoreCase);
                    else if (dataType == "Integer")
                        return !(int.TryParse(propValue, out int i1) && int.TryParse(ConditionValue, out int i2) && i1 == i2);
                    else if (dataType == "Double")
                        return !(double.TryParse(propValue, out double d1) && double.TryParse(ConditionValue, out double d2) && Math.Abs(d1 - d2) < 1e-9);
                    else if (dataType == "DateTime")
                        return !(DateTime.TryParse(propValue, out DateTime dt1) && DateTime.TryParse(ConditionValue, out DateTime dt2) && dt1 == dt2);
                    else if (dataType == "Boolean")
                        return !(bool.TryParse(propValue, out bool b1) && bool.TryParse(ConditionValue, out bool b2) && b1 == b2);
                    break;
                case ">":
                    if (dataType == "Integer")
                        return int.TryParse(propValue, out int i1) && int.TryParse(ConditionValue, out int i2) && i1 > i2;
                    else if (dataType == "Double")
                        return double.TryParse(propValue, out double d1) && double.TryParse(ConditionValue, out double d2) && d1 > d2;
                    else if (dataType == "DateTime")
                        return DateTime.TryParse(propValue, out DateTime dt1) && DateTime.TryParse(ConditionValue, out DateTime dt2) && dt1 > dt2;
                    break;
                case "<":
                    if (dataType == "Integer")
                        return int.TryParse(propValue, out int i1) && int.TryParse(ConditionValue, out int i2) && i1 < i2;
                    else if (dataType == "Double")
                        return double.TryParse(propValue, out double d1) && double.TryParse(ConditionValue, out double d2) && d1 < d2;
                    else if (dataType == "DateTime")
                        return DateTime.TryParse(propValue, out DateTime dt1) && DateTime.TryParse(ConditionValue, out DateTime dt2) && dt1 < dt2;
                    break;
                case ">=":
                    return EvaluateFeature(feature, ">") || EvaluateFeature(feature, "=");
                case "<=":
                    return EvaluateFeature(feature, "<") || EvaluateFeature(feature, "=");
                case "like":
                    return propValue.IndexOf(ConditionValue, StringComparison.InvariantCultureIgnoreCase) >= 0;
            }
            return false;
        }

        private bool EvaluateFeature(Feature feature, string op)
        {
            var oper = SelectedOperator;
            SelectedOperator = oper;
            bool result = EvaluateFeature(feature);
            SelectedOperator = oper;
            return result;
        }

        private void Cancel() => CloseWindow?.Invoke(this, false);

        protected new bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;

            field = value;
            OnPropertyChanged(propertyName);

            (ExecuteCommand as RelayCommand)?.RaiseCanExecuteChanged();

            return true;
        }
    }
}
