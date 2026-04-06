using GIS.Classes.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace GIS.UserControls
{
    public partial class FeatureAttributesCreationUserControl : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(IAttributeEditorViewModel),
                                                    typeof(FeatureAttributesCreationUserControl),
                                                    new PropertyMetadata(null, OnViewModelChanged));

        public IAttributeEditorViewModel ViewModel
        {
            get => (IAttributeEditorViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (FeatureAttributesCreationUserControl)d;
            if (e.NewValue != null)
            {
                control.DataContext = e.NewValue;
            }
        }

        public FeatureAttributesCreationUserControl()
        {
            InitializeComponent();
        }
    }
}