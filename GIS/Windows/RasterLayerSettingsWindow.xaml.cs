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
                Close();
                (Owner as MainWindow).StartCalibrationProcess(viewModel);
            };
        }
    }
}
