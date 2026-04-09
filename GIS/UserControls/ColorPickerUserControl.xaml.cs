using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GIS.UserControls
{
    public partial class ColorPickerUserControl : UserControl
    {
        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register("SelectedColor", typeof(Color),
                typeof(ColorPickerUserControl),
                new FrameworkPropertyMetadata(Colors.Black,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnSelectedColorChanged));

        public Color SelectedColor
        {
            get => (Color)GetValue(SelectedColorProperty);
            set => SetValue(SelectedColorProperty, value);
        }

        public ColorPickerUserControl()
        {
            InitializeComponent();
            Loaded += ColorPickerControl_Loaded;
        }

        private void ColorPickerControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeColorPalette();
        }

        private void InitializeColorPalette()
        {
            var colors = new List<Color>
            {
                Colors.Black, Colors.Gray, Colors.Silver, Colors.White,
                Colors.Red, Colors.DarkRed, Colors.Orange, Colors.Yellow,
                Colors.Green, Colors.DarkGreen, Colors.Lime, Colors.Olive,
                Colors.Blue, Colors.DarkBlue, Colors.Cyan, Colors.Teal,
                Colors.Purple, Colors.Magenta, Colors.Pink, Colors.Brown,
                Colors.Beige, Colors.Coral, Colors.Gold, Colors.Indigo,
                Colors.Khaki, Colors.Lavender, Colors.Maroon, Colors.Navy,
                Colors.Olive, Colors.Orchid, Colors.Plum, Colors.RosyBrown,
                Colors.SkyBlue, Colors.Tomato, Colors.Violet, Colors.Wheat
            };

            ColorsPalette.Children.Clear();

            foreach (var color in colors)
            {
                var colorRect = new Border
                {
                    Width = 25,
                    Height = 25,
                    Margin = new Thickness(2),
                    CornerRadius = new CornerRadius(3),
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    Background = new SolidColorBrush(color),
                    Cursor = Cursors.Hand,
                    Tag = color
                };

                colorRect.MouseLeftButtonDown += ColorRect_MouseLeftButtonDown;
                ColorsPalette.Children.Add(colorRect);
            }
        }

        private void ColorRect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is Color color)
            {
                SelectedColor = color;
                UpdatePreviewColor();
                ColorPopup.IsOpen = false;
            }
        }

        private void ColorPreview_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ColorPopup.IsOpen = true;
            UpdateSliderValues();
        }

        private void UpdateSliderValues()
        {
            RedSlider.Value = SelectedColor.R;
            GreenSlider.Value = SelectedColor.G;
            BlueSlider.Value = SelectedColor.B;
        }

        private void UpdatePreviewColor()
        {
            PreviewColor.Background = new SolidColorBrush(SelectedColor);
            ColorPreview.Background = new SolidColorBrush(SelectedColor);
        }

        private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ColorPickerUserControl;
            if (control != null && e.NewValue is Color newColor)
            {
                control.UpdatePreviewColor();
                control.UpdateSliderValues();
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedColor = Color.FromRgb(
                (byte)RedSlider.Value,
                (byte)GreenSlider.Value,
                (byte)BlueSlider.Value
            );
            ColorPopup.IsOpen = false;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ColorPopup.IsOpen = false;
        }
    }
}