using GIS.Classes.Main;
using System.Windows;

namespace GIS.Windows
{
    /// <summary>
    /// Логика взаимодействия для MapImageSettingsWindow.xaml
    /// </summary>
    public partial class MapImageSettingsWindow : Window
    {
        public GeoBounds ImageBounds { get; private set; }
        public MapImageSettingsWindow()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                SizeToContent = SizeToContent.WidthAndHeight;
                InvalidateMeasure();
            };
        }

        private void LayerSettingsApplyButton_Click(object sender, RoutedEventArgs e)
        {
            ImageBounds = new GeoBounds
            {
                MinLon = Convert.ToInt32(ImageBounds.MinLon),
                MaxLon = Convert.ToInt32(ImageBounds.MaxLon),
                MinLat = Convert.ToInt32(ImageBounds.MinLat),
                MaxLat = Convert.ToInt32(ImageBounds.MaxLat)
            };

            DialogResult = true;
            Close();
        }

        private void LayerSettingsCancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
