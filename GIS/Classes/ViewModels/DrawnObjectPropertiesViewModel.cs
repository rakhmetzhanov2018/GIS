using GIS.Classes.DrawObjects;
using GIS.Classes.Factories;
using GIS.Classes.Main;
using GIS.Classes.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GIS.Classes.ViewModels
{
    
    public class AttributeField : ViewModelBase, IDataErrorInfo
    {
        private string _value;
        public string Name { get; set; }
        public string DataType { get; set; }
        public string Value
        {
            get => _value;
            set {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Error));
                }
            }
        }

        public string Error => null;
        public string this[string columnName]
        {
            get
            {
                if (columnName == nameof(Value))
                {
                    DataValidator.Validate(Value, DataType, out string error);
                    return error;
                }
                return null;
            }
        }
    }

    public class DrawnObjectPropertiesViewModel : ViewModelBase
    {
        private readonly Layer _layer;

        public ObservableCollection<AttributeField> AttributeFields { get; } = [];
        public ICommand CreateCommand { get; }
        public ICommand CancelCommand { get; }
        public event EventHandler<bool> CloseWindow;

        public DrawnObjectPropertiesViewModel(Layer layer)
        {
            _layer = layer;

            CreateCommand = new RelayCommand(Create, CanCreate);
            CancelCommand = new RelayCommand(Cancel);

            InitializeAttributeFields();
        }

        private void InitializeAttributeFields()
        {
            if (_layer.FeatureProperties == null) return;
            foreach (var prop in _layer.FeatureProperties)
            {
                var field = new AttributeField
                {
                    Name = prop.Name,
                    DataType = prop.DataType,
                    Value = prop.DefaultValue ?? string.Empty
                };

                field.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(AttributeField.Value))
                    {
                        (CreateCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    }
                };

                AttributeFields.Add(field);

            }
        }

        private void Create() => CloseWindow?.Invoke(this, true);

        private void Cancel() => CloseWindow?.Invoke(this, false);

        private bool CanCreate() => AttributeFields.All(field => 
            string.IsNullOrEmpty((field as IDataErrorInfo)[nameof(AttributeField.Value)]));
    }
}