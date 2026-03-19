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
    /// <summary>
    /// Логика взаимодействия для CreateNewLayerWindow.xaml
    /// </summary>
    public partial class CreateNewLayerWindow : Window
    {
        public CreateNewLayerWindow(CreateNewLayerViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;

            viewModel.CloseWindow += (s, result) =>
            {
                DialogResult = result;
                Close();
            };
        }
    }
}
