using GIS.Classes.ViewModels;
using System.Windows;

namespace GIS.Windows
{
    public partial class DrawnObjectPropertiesWindow : Window
    {
        public DrawnObjectPropertiesWindow(DrawnObjectPropertiesViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            Loaded += (s, e) =>
            {
                SizeToContent = SizeToContent.WidthAndHeight;
                InvalidateMeasure();
            };

            viewModel.CloseWindow += (s, result) =>
            {
                DialogResult = result;
                Close();
            };
        }
    }
}