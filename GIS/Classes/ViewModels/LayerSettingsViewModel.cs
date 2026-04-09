using GIS.Classes.Main;
using GIS.Classes.Styles;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;

namespace GIS.Classes.ViewModels
{
    public class LayerSettingsViewModel : ViewModelBase, IAttributeEditorViewModel
    {
        private readonly Layer _layer;

        private string _layerName;
        private bool _isLayerVisible;
        private double _opacity = 1.0;
        private Color _mainColor = Colors.Red;
        private GeometryType _geometryType;

        private int _pointSize = 6;

        private int _lineThickness = 4;

        private Color _fillColor = Colors.LightBlue;

        public string LayerName
        {
            get => _layerName;
            set => SetField(ref _layerName, value);
        }

        public bool IsLayerVisible
        {
            get => _isLayerVisible;
            set => SetField(ref _isLayerVisible, value);
        }

        public double Opacity
        {
            get => _opacity;
            set => SetField(ref _opacity, value);
        }

        public Color MainColor
        {
            get => _mainColor;
            set => SetField(ref _mainColor, value);
        }

        public GeometryType GeometryType
        {
            get => _geometryType;
            private set => SetField(ref _geometryType, value);
        }
        public bool IsPointType => GeometryType == GeometryType.Point;
        public bool IsLineType => GeometryType == GeometryType.LineString;
        public bool IsPolygonType => GeometryType == GeometryType.Polygon;

        public int PointSize
        {
            get => _pointSize;
            set => SetField(ref _pointSize, value);
        }

        public int LineThickness
        {
            get => _lineThickness;
            set => SetField(ref _lineThickness, value);
        }
        public Color FillColor
        {
            get => _fillColor;
            set => SetField(ref _fillColor, value);
        }

        public ICommand ApplyCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler<bool> CloseWindow;
        public LayerSettingsViewModel(Layer layer)
        {
            _layer = layer;
            Attributes = layer.FeatureProperties;

            ApplyCommand = new RelayCommand(Apply, CanApply);
            CancelCommand = new RelayCommand(Cancel);

            AddAttributeCommand = new RelayCommand(AddAttribute, CanAddAttribute);
            RemoveSelectedAttributeCommand = new RelayCommand(RemoveSelectedAttribute, CanRemoveSelectedAttribute);

            LoadFromLayer();
        }

        private void LoadFromLayer()
        {
            LayerName = _layer.Name;
            IsLayerVisible = _layer.IsVisible;
            GeometryType = _layer.GeoType;

            Attributes = _layer.FeatureProperties;

            var style = _layer.LayerStyle;

            if (style != null)
            {
                Opacity = style.Opacity;
                MainColor = style.MainColor;

                switch (style)
                {
                    case PointStyle pointStyle:
                        PointSize = pointStyle.Size;
                        break;
                    case LineStringStyle lineStyle:
                        LineThickness = lineStyle.StrokeThickness;
                        break;
                    case PolygonStyle polygonStyle:
                        LineThickness = polygonStyle.StrokeThickness;
                        FillColor = polygonStyle.FillColor;
                        break;
                }
            }
        }

        private LayerStyle CreateStyleFromCurrentSettings()
        {
            switch (GeometryType)
            {
                case GeometryType.Point:
                    return new PointStyle
                    {
                        Opacity = Opacity,
                        MainColor = MainColor,
                        Size = PointSize
                    };

                case GeometryType.LineString:
                    return new LineStringStyle
                    {
                        Opacity = Opacity,
                        MainColor = MainColor,
                        StrokeThickness = LineThickness
                    };

                case GeometryType.Polygon:
                    return new PolygonStyle
                    {
                        Opacity = Opacity,
                        MainColor = MainColor,
                        StrokeThickness = LineThickness,
                        FillColor = FillColor
                    };

                default:
                    throw new Exception("Что-то явно не так с типом LayerStyle в CreateStyleFromCurrentSettings");
            }
        }

        private void Apply()
        {
            if (string.IsNullOrWhiteSpace(LayerName)) return;

            _layer.Name = LayerName;
            _layer.IsVisible = IsLayerVisible;
            _layer.LayerStyle = CreateStyleFromCurrentSettings();
            _layer.FeatureProperties = Attributes;

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

            (ApplyCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (AddAttributeCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (RemoveSelectedAttributeCommand as RelayCommand)?.RaiseCanExecuteChanged();

            return true;
        }

        private FeatureProperty _selectedAttribute;
        private string _newAttrubuteName;
        private AttributeDataType _newAttrubuteType;
        private string _newAttributeDefaultValue;

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

        public ObservableCollection<FeatureProperty> Attributes { get; set; }
        public IEnumerable<AttributeDataType> DTypes =>
            Enum.GetValues(typeof(AttributeDataType)).Cast<AttributeDataType>();
        public IEnumerable<GeometryType> GTypes =>
            Enum.GetValues(typeof(GeometryType)).Cast<GeometryType>();

        public ICommand AddAttributeCommand { get; }
        public ICommand RemoveSelectedAttributeCommand { get; }

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


    }
}