using GIS.Classes.ViewModels;
using System.Windows;

namespace GIS.Windows
{
    /// <summary>
    /// Логика взаимодействия для BufferAnalysisWindow.xaml
    /// </summary>
    public partial class BufferAnalysisWindow : Window
    {
        public BufferAnalysisWindow(BufferAnalysisViewModel viewModel)
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