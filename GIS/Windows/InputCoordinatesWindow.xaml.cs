using System.Windows;

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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.SizeToContent = SizeToContent.Height;
        }
    }
}
