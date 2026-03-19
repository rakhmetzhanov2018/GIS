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
    public class CreateNewLayerViewModel : INotifyPropertyChanged
    {
        private string _layerName;
        private string _geoType;
        private FeatureProperty _selectedAttribute;
        private string _newAttrubuteName;
        private string _newAttrubuteType;
        private string _newAttributeDefaultValue;

        public ObservableCollection<FeatureProperty> Attributes { get; set; }

        public string LayerName
        {
            get => _layerName;
            set => SetField(ref _layerName, value);
        }

        public string GeoType
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

        public string NewAttributeType
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
                DataType = NewAttributeType,
                DefaultValue = NewAttributeDefaultValue
            });

            NewAttributeName = string.Empty;
            NewAttributeType = string.Empty;
            NewAttributeDefaultValue = string.Empty;
        }

        private bool CanAddAttribute()
        {
            return !string.IsNullOrWhiteSpace(NewAttributeName) && !string.IsNullOrWhiteSpace(NewAttributeType);
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
            return !string.IsNullOrWhiteSpace(LayerName) && !string.IsNullOrEmpty(GeoType); 
        }

        private void Cancel()
        {
            CloseWindow?.Invoke(this, false);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string protertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;

            field = value;
            OnPropertyChanged();

            (AddAttributeCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (RemoveSelectedAttributeCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (ApplyCommand as RelayCommand)?.RaiseCanExecuteChanged();

            return true;
        }
    } 
}

