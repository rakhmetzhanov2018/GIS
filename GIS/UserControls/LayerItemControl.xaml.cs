using System.Windows.Controls;
using System.Windows.Input;

namespace GIS
{
    /// <summary>
    /// Логика взаимодействия для LayerItemControl.xaml
    /// </summary>
    public partial class LayerItemControl : UserControl
    {
        public string LayerName { get; set; }
        public bool IsLayerVisible { get; set; }

        public ICommand StyleSettingsCommand { get; set; }
        public ICommand AttributeTableCommand { get; set; }
        public ICommand VisibilityToggleCommand { get; set; }

        public event EventHandler StyleButtonClicked;
        public event EventHandler AttributeButtonClicked;
        public LayerItemControl()
        {
            InitializeComponent();
        }
    }
}
