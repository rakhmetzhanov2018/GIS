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
    /// <summary>
    /// Логика взаимодействия для SpatialQueryWindow.xaml
    /// </summary>
    public partial class SpatialQueryWindow : Window
    {
        public SpatialQueryWindow(SpatialQueryViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.CloseWindow += (s, result) =>
            {
                DialogResult = result;
                Close();
            };
        }

        private void SourceLayer_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            (DataContext as SpatialQueryViewModel)?.UpdateSelectedCount();
        }
    }
}
