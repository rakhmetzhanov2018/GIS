using GIS.Classes.Main;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace GIS
{
    public partial class LayerAttributesTableWindow : Window
    {
        public Layer Layer { get; private set; }
        internal LayerAttributesTableWindow(Layer layer)
        {
            InitializeComponent();
            Layer = layer;
            Loaded += (s, e) =>
            {
                SizeToContent = SizeToContent.WidthAndHeight;
                InvalidateMeasure();
            };
            FillTable(layer.ObjectList);
            
        }

        public void UpdateTable(Layer layer)
        {
            Layer = layer;
            FillTable(layer.ObjectList);
        }

        private void FillTable(ObservableCollection<Feature> objectList)
        {
            var dt = new DataTable();
            dt.Columns.Add("Название объекта", typeof(string));

            if (objectList.Count > 0)
            {
                foreach (var key in objectList[0].props.Keys)
                {
                    if (!dt.Columns.Contains(key))
                        dt.Columns.Add(key);
                }
                foreach (var obj in objectList)
                {
                    var row = dt.NewRow();
                    row["Название объекта"] = obj.Name;
                    foreach (var key in obj.props.Keys)
                    {
                        if (dt.Columns.Contains(key))
                            row[key] = obj.props[key];
                    }
                    dt.Rows.Add(row);
                }
            }

            LayerItemsDataGrid.ItemsSource = dt.DefaultView;
            LayerItemsDataGrid.Loaded += (s, e) => AdjustColumnWidths();
        }

        private void AdjustColumnWidths()
        {
            if (LayerItemsDataGrid.Columns.Count == 0) return;
            LayerItemsDataGrid.Columns[0].Width = new DataGridLength(150);
            for (int i = 1; i < LayerItemsDataGrid.Columns.Count; i++)
                LayerItemsDataGrid.Columns[i].Width = DataGridLength.Auto;
        }
    }
}