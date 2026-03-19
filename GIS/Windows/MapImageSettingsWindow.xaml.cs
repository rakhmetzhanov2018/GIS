using GIS.Classes.Main;
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
    /// Логика взаимодействия для MapImageSettingsWindow.xaml
    /// </summary>
    public partial class MapImageSettingsWindow : Window
    {
        public GeoBounds ImageBounds { get; private set; }
        public MapImageSettingsWindow()
        {
            InitializeComponent();
        }

        private void LayerSettingsApplyButton_Click(object sender, RoutedEventArgs e)
        {
            ImageBounds = new GeoBounds
            {
                MinLon = Convert.ToInt32(ImageBounds.MinLon),
                MaxLon = Convert.ToInt32(ImageBounds.MaxLon),
                MinLat = Convert.ToInt32(ImageBounds.MinLat),
                MaxLat = Convert.ToInt32(ImageBounds.MaxLat)
            };

            DialogResult = true;
            Close();
        }

        private void LayerSettingsCancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
