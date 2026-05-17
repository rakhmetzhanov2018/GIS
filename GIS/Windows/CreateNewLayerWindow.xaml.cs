using GIS.Classes.ViewModels;
using System.Windows;

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
