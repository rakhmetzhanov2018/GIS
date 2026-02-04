using GIS.Classes;
using GIS.Classes.Factories;
using GIS.Classes.Styles;
using GIS.Classes.ViewModels;
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
    /// <summary>-
    /// Логика взаимодействия для LayerSettingsWindow.xaml
    /// </summary>
    public partial class LayerSettingsWindow : Window
    {
        private Layer layer;
        private LayerSettingsViewModel layerSettingsViewModel;
        internal LayerSettingsWindow(Layer layer)
        {
            InitializeComponent();

            this.layer = layer;
            layerSettingsViewModel = new(layer);

            DataContext = layerSettingsViewModel;
        }

        private void LayerSettingsApplyButton_Click(object sender, RoutedEventArgs e)
        {
            layerSettingsViewModel.ApplyChanges();

            layer.ApplyStyleToAllFeatures();

            DialogResult = true;
        }

        private void LayerSettingsCancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
