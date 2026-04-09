using GIS.Classes.DrawObjects;
using GIS.Classes.Styles;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GIS.Classes.Main
{
    public class Layer : INotifyPropertyChanged
    {
        private string _name;
        private bool _isVisible = true;
        private LayerStyle _layerStyle;
        private GeometryType _geoType;
        private bool _isSelected;
        private ObservableCollection<FeatureProperty> _featureProperties;

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool IsVisible
        { 
            get => _isVisible;
            set 
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    OnPropertyChanged();
                    UpdateVisibility();
                }
            } 
        }
        public LayerStyle LayerStyle
        {
            get => _layerStyle;
            set
            {
                if (_layerStyle != value)
                {
                    _layerStyle = value;
                    _layerStyle.PropertyChanged += OnStylePropertyChanged;
                }
            }
        }
        public ObservableCollection<Feature> ObjectList { get; } = new();
        public GeoBounds Bounds { get; set; }
        public GeometryType GeoType
        {
            get
            {
                if (ObjectList.Count == 0)
                {
                    return _geoType;
                }
                return ObjectList.First().Geometry switch
                {
                    GeoGraphicPoint => GeometryType.Point,
                    GeoGraphicLineString => GeometryType.LineString,
                    GeoGraphicPolygon => GeometryType.Polygon,
                    _ => throw new InvalidOperationException()
                };
            }

            set => _geoType = value;
        }
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }
        public ObservableCollection<FeatureProperty> FeatureProperties
        {
            get => _featureProperties;
            set
            {
                if (_featureProperties != value)
                {
                    _featureProperties = value;
                    OnPropertyChanged();
                }
            }
        }

        public Layer()
        {
        }
        public Layer(string name, GeometryType geoType)
        {
            Name = name;
            GeoType = geoType;
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;

        public void UpdateVisibility()
        {
            foreach (Feature feature in ObjectList)
            {
                feature.SetVisibility(IsVisible);
            }
        }
        public void AddObject(Feature ob)
        {
            ObjectList.Add(ob);
        }
        public void DeleteObject(Feature ob)
        {
            ObjectList.Remove(ob);
        }
        public override string ToString()
        {
            return $"{Name}";
        }
        public void DrawAll(Canvas canvas)
        {
            foreach (Feature feature in ObjectList)
            {
                feature.DrawFigure(canvas);
                feature.SetVisibility(IsVisible);
            }
        }
        public virtual void UpdateAll()
        {
            foreach (Feature feature in ObjectList)
            {
                feature.UpdateFigure();
            }
        }
        public string GetStatistics()
        {
            int pointCount = 0;
            int lineCount = 0;
            int polygonCount = 0;

            foreach (Feature feature in ObjectList)
            {
                switch (feature.Geometry)
                {
                    case GeoGraphicPoint:
                        pointCount++;
                        break;
                    case GeoGraphicLineString:
                        lineCount++;
                        break;
                    case GeoGraphicPolygon:
                        polygonCount++;
                        break;
                }
            }

            return $"GeoGraphicPoint - {pointCount}, GeoGraphicLineString - {lineCount}, GeoGraphicPolygon - {polygonCount}";
        }
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void OnStylePropertyChanged(object sender, PropertyChangedEventArgs e) 
        {
            ApplyStyleToAllFeatures();
        }
        public void ApplyStyleToAllFeatures()
        {
            if (_layerStyle == null)
            {
                return;
            }
            foreach (Feature feature in ObjectList)
            {
                LayerStyle.ApplyToFeature(feature);
            }
        }
        public void AnalyzeFeatureProperties()
        {
            FeatureProperties = new ObservableCollection<FeatureProperty>();

            foreach (var prop in ObjectList.First().props)
            {
                FeatureProperties.Add(new FeatureProperty
                {
                    Name = prop.Key,
                    DataType = FeatureProperty.DefineDataType(prop.Value)
                });
            }
        }
    }
}
