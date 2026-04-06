using GIS.Classes.Factories;
using GIS.Classes.Main;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Packaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GIS.Classes.ViewModels
{
    public class CreateNewLayerViewModel : ViewModelBase, INotifyPropertyChanged, IAttributeEditorViewModel
    {
        private string _layerName;
        private GeometryType _geoType;
        private FeatureProperty _selectedAttribute;
        private string _newAttrubuteName;
        private AttributeDataType _newAttrubuteType;
        private string _newAttributeDefaultValue;

        public ObservableCollection<FeatureProperty> Attributes { get; set; }
        public IEnumerable<AttributeDataType> DTypes => 
            Enum.GetValues(typeof(AttributeDataType)).Cast<AttributeDataType>();
        public IEnumerable<GeometryType> GTypes => 
            Enum.GetValues(typeof(GeometryType)).Cast<GeometryType>();

        public string LayerName
        {
            get => _layerName;
            set => SetField(ref _layerName, value);
        }

        public GeometryType GeoType
        {
            get => _geoType;
            set => SetField(ref _geoType, value);
        }

        public FeatureProperty SelectedAttribute
        {
            get => _selectedAttribute;
            set => SetField(ref _selectedAttribute, value);
        }

        public string NewAttributeName
        {
            get => _newAttrubuteName;
            set => SetField(ref _newAttrubuteName, value);
        }

        public AttributeDataType NewAttributeType
        {
            get => _newAttrubuteType;
            set => SetField(ref _newAttrubuteType, value);
        }

        public string NewAttributeDefaultValue
        {
            get => _newAttributeDefaultValue;
            set => SetField(ref _newAttributeDefaultValue, value);
        }

        public ICommand AddAttributeCommand { get; }
        public ICommand RemoveSelectedAttributeCommand { get; }
        public ICommand ApplyCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler<bool> CloseWindow;

        public CreateNewLayerViewModel()
        {
            Attributes = new ObservableCollection<FeatureProperty>();

            AddAttributeCommand = new RelayCommand(AddAttribute, CanAddAttribute);
            RemoveSelectedAttributeCommand = new RelayCommand(RemoveSelectedAttribute, CanRemoveSelectedAttribute);
            ApplyCommand = new RelayCommand(Apply, CanApply);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void AddAttribute()
        {
            Attributes.Add(new FeatureProperty
            {
                Name = NewAttributeName,
                DataType = NewAttributeType.ToString(),
                DefaultValue = NewAttributeDefaultValue
            });

            NewAttributeName = string.Empty;
            NewAttributeType = AttributeDataType.String;
            NewAttributeDefaultValue = string.Empty;
        }

        private bool CanAddAttribute()
        {
            return !string.IsNullOrWhiteSpace(NewAttributeName);
        }

        private void RemoveSelectedAttribute()
        {
            Attributes.Remove(SelectedAttribute);
        }

        private bool CanRemoveSelectedAttribute()
        {
            return SelectedAttribute != null;
        }

        private void Apply()
        {
            CloseWindow?.Invoke(this, true);
        }

        private bool CanApply()
        {
            return !string.IsNullOrWhiteSpace(LayerName); 
        }

        private void Cancel()
        {
            CloseWindow?.Invoke(this, false);
        }
        protected new bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;

            field = value;
            OnPropertyChanged(propertyName);

            (AddAttributeCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (RemoveSelectedAttributeCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (ApplyCommand as RelayCommand)?.RaiseCanExecuteChanged();

            return true;
        }
    } 
}

