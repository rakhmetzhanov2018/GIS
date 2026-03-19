using GIS.Classes.Factories;
using GIS.Classes.Main;
using GIS.Classes.Styles;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace GIS.Classes.ViewModels
{
    public class LayerSettingsViewModel
    {
        private Layer layer;
        public LayerStyle NewStyle { get; set; }
        public string LayerName { get; set; }
        public bool IsLayerVisible { get; set; }

        public bool IsPointStyle => NewStyle is PointStyle;
        public bool IsLineStringStyle => NewStyle is LineStringStyle;
        public bool IsPolygonStyle => NewStyle is PolygonStyle;
        public bool IsLineStringOrPolygonStyle => IsLineStringStyle || IsPolygonStyle;

        public LayerSettingsViewModel(Layer layer)
        {
            this.layer = layer;
            
            if (layer.LayerStyle == null)
            {
                NewStyle = DefaultStyleFactory.CreateDefaultStyle(layer.GeoType);
            }
            else
            {
                NewStyle = CopyStyle(layer.LayerStyle);
            }

            LayerName = layer.Name;
            IsLayerVisible = layer.IsVisible;
        }
        private LayerStyle CopyStyle(LayerStyle oldStyle)
        {
            return oldStyle switch
            {
                PointStyle => new PointStyle((PointStyle)oldStyle),
                LineStringStyle => new LineStringStyle((LineStringStyle)oldStyle),
                PolygonStyle => new PolygonStyle((PolygonStyle)oldStyle),
                _ => throw new NotImplementedException()
            };
        }
        private void CopyStyleProperties(LayerStyle oldStyle, LayerStyle newStyle)
        {
            newStyle.Opacity = oldStyle.Opacity;
            newStyle.MainColor = oldStyle.MainColor;

            if (oldStyle is PointStyle point)
            {
                ((PointStyle)newStyle).Size = point.Size;
            }
            else if (oldStyle is LineStringStyle line)
            {
                ((LineStringStyle)newStyle).StrokeThickness = line.StrokeThickness;
            }
            else if (oldStyle is PolygonStyle polygon)
            {
                ((PolygonStyle)newStyle).StrokeThickness = polygon.StrokeThickness;
                ((PolygonStyle)newStyle).FillColor = polygon.FillColor;
            }
        }
        public void ApplyChanges()
        {
            layer.Name = LayerName;
            layer.IsVisible = IsLayerVisible;

            if (layer.LayerStyle == null)
            {
                layer.LayerStyle = CopyStyle(NewStyle);
            }
            else
            {
                CopyStyleProperties(NewStyle, layer.LayerStyle);
            }
        }
    }
}
