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
using System.Windows.Navigation;
using System.Windows.Shapes;

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
