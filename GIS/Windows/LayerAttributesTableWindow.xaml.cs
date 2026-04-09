using GIS.Classes.DrawObjects;
using GIS.Classes.Main;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GIS
{
    /// <summary>
    /// Логика взаимодействия для LayerAttributesTableWindow.xaml
    /// </summary>
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
        }

        private void FillTable(ObservableCollection<Feature> ObjectList)
        {
            DataTable dt = new DataTable();

            if (ObjectList.Count == 0)
                return;

            foreach (var key in ObjectList[0].props.Keys)
            {
                dt.Columns.Add(key);
            }

            foreach (var obj in ObjectList)
            {
                var row = dt.NewRow();

                foreach (var key in obj.props.Keys)
                {
                    if (dt.Columns.Contains(key))
                        row[key] = obj.props.ContainsKey(key) ? obj.props[key] : "Нет данных";
                }

                dt.Rows.Add(row);
            }

            LayerItemsDataGrid.ItemsSource = dt.DefaultView;
        }
       
    }
}
