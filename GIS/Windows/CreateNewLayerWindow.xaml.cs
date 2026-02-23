using GIS.Classes;
using GIS.Classes.Factories;
using GIS.Classes.Styles;
using System;
using System.Collections.Generic;
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

namespace GIS.Windows
{
    /// <summary>
    /// Логика взаимодействия для CreateNewLayerWindow.xaml
    /// </summary>
    public partial class CreateNewLayerWindow : Window
    {
        public Layer CreatedLayer;
        public CreateNewLayerWindow()
        {
            InitializeComponent();
        }

        private void CreateLayer_ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            var newLayer = new Layer(
                LayerNameTextBox.Text,
                GeometryTypeComboBox.Text
            );

            newLayer.LayerStyle = DefaultStyleFactory.CreateDefaultStyle(GeometryTypeComboBox.Text);

            CreatedLayer = newLayer;

            DialogResult = true;
            Close();
        }

        private void CreateLayer_CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
