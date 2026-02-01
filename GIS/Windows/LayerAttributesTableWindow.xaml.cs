using GIS.Classes;
using GIS.Classes.DrawObjects;
using System;
using System.Collections.Generic;
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

            FillTable(layer.ObjectList);
        }

        private void FillTable(List<Feature> ObjectList)
        {
            DataTable dt = new DataTable();

            foreach (var key in ObjectList[0].props.Keys)
            {
                dt.Columns.Add(key);
            }

            foreach (var obj in ObjectList)
            {
                var row = dt.NewRow();

                foreach (var key in obj.props.Keys)
                {
                    row[key] = obj.props[key];
                }

                dt.Rows.Add(row);
            }

            LayerItemsDataGrid.ItemsSource = dt.DefaultView;
        }
       
    }
}
