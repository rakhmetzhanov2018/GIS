using GIS.Classes.Factories;
using GIS.Classes.Main;
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
        public LayerSettingsWindow(LayerSettingsViewModel layerSettingsViewModel)
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                SizeToContent = SizeToContent.WidthAndHeight;
                InvalidateMeasure();
            };

            DataContext = layerSettingsViewModel;

            layerSettingsViewModel.CloseWindow += (s, result) =>
            {
                DialogResult = result;
                Close();
            };
        }
    }
}
