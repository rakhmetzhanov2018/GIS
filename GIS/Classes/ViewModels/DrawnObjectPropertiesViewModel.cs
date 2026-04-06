using GIS.Classes.DrawObjects;
using GIS.Classes.Factories;
using GIS.Classes.Main;
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
    public class AttributeField : ViewModelBase
    {
        private string _value;
        public string Name { get; set; }
        public string DataType { get; set; }
        public string Value
        {
            get => _value;
            set { _value = value; OnPropertyChanged(); }
        }
    }

    public class DrawnObjectPropertiesViewModel : ViewModelBase
    {
        private readonly Layer _layer;
        private readonly GeoGraphicObject _geometry;
        public Canvas TargetCanvas { get; set; }

        public ObservableCollection<AttributeField> AttributeFields { get; } = new();
        public ICommand CreateCommand { get; }
        public ICommand CancelCommand { get; }
        public event EventHandler<bool> CloseWindow;

        public DrawnObjectPropertiesViewModel(Layer layer, GeoGraphicObject geometry, Canvas canvas)
        {
            _layer = layer;
            _geometry = geometry;
            TargetCanvas = canvas;

            CreateCommand = new RelayCommand(Create);
            CancelCommand = new RelayCommand(Cancel);

            InitializeAttributeFields();
        }

        private void InitializeAttributeFields()
        {
            if (_layer.FeatureProperties == null) return;
            foreach (var prop in _layer.FeatureProperties)
            {
                AttributeFields.Add(new AttributeField
                {
                    Name = prop.Name,
                    DataType = prop.DataType,
                    Value = prop.DefaultValue ?? string.Empty
                });
            }
        }

        private void Create()
        {
            var props = new Dictionary<string, string>();
            foreach (var field in AttributeFields)
                props[field.Name] = field.Value;

            var feature = new Feature(_geometry, props);
            feature.Name = props.TryGetValue("name", out var name) ? name : "Без названия";
            _layer.AddObject(feature);

            if (_layer.LayerStyle == null)
                _layer.LayerStyle = DefaultStyleFactory.CreateDefaultStyle(_layer.GeoType);
            _layer.LayerStyle.ApplyToFeature(feature);

            feature.DrawFigure(TargetCanvas);
            feature.SetVisibility(_layer.IsVisible);

            CloseWindow?.Invoke(this, true);
        }

        private void Cancel() => CloseWindow?.Invoke(this, false);
    }
}