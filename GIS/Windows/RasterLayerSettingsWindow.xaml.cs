using GIS.Classes.ViewModels;
using System.Windows;

namespace GIS.Windows
{
    public partial class RasterLayerSettingsWindow : Window
    {
        private bool _closingForCalibration = false;
        public RasterLayerSettingsWindow(RasterLayerSettingsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Owner = Application.Current.MainWindow;

            viewModel.CloseWindow += (s, result) =>
            {
                if (!_closingForCalibration)
                {
                    DialogResult = result;
                }
                Close();
            };

            viewModel.StartCalibration += (s, e) =>
            {
                _closingForCalibration = true;
                var targetLayer = (s as RasterLayerSettingsViewModel)?.GetRasterLayer();
                Close();
                (Owner as MainWindow).StartCalibrationProcess(viewModel, targetLayer);
            };
        }
    }
}
