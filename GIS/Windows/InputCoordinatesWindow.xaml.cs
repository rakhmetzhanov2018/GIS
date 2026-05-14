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
    /// Логика взаимодействия для InputCoordinatesWindow.xaml
    /// </summary>
    public partial class InputCoordinatesWindow : Window
    {
        public double Lon { get; private set; }
        public double Lat { get; private set; }
        public InputCoordinatesWindow()
        {
            InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(LonBox.Text, out double lon) && double.TryParse(LatBox.Text, out double lat))
            {
                Lon = lon;
                Lat = lat;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Введите корректные числа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
