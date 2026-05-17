using GIS.Classes.ViewModels;
using System.Windows;

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
