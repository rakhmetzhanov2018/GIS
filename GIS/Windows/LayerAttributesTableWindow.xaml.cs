using GIS.Classes.Main;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace GIS
{
    public partial class LayerAttributesTableWindow : Window
    {
        internal LayerAttributesTableWindow(Layer layer)
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                SizeToContent = SizeToContent.WidthAndHeight;
                InvalidateMeasure();
            };
            FillTable(layer.ObjectList);
            LayerItemsDataGrid.Loaded += (s, e) => AdjustColumnWidths();
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
        }

        private void AdjustColumnWidths()
        {
            foreach (DataGridColumn col in LayerItemsDataGrid.Columns)
            {
                if (col.Header?.ToString() == "Название объекта")
                    col.Width = new DataGridLength(150);
                else
                    col.Width = DataGridLength.Auto;
            }
        }
    }
}