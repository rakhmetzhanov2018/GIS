using GIS.Classes.Main;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace GIS.Classes.ViewModels
{
    public class AttributeItem : INotifyPropertyChanged
    {
        private string value;
        public string Key { get; set; }
        public string Value
        {
            get => value;
            set { this.value = value; OnPropertyChanged(); }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class FeatureSettingsViewModel : ViewModelBase
    {
        private readonly Feature feature;
        private readonly Layer layer;
        private string featureName;
        public ObservableCollection<AttributeItem> Attributes { get; } = new();

        public string FeatureName
        {
            get => featureName;
            set => SetField(ref featureName, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler<bool> CloseWindow;

        public FeatureSettingsViewModel(Feature feature, Layer layer)
        {
            this.feature = feature;
            this.layer = layer;
            FeatureName = feature.Name;

            var propsOrder = this.layer.FeatureProperties?.Select(p => p.Name).ToList() ?? feature.props.Keys.ToList();
            foreach (var key in propsOrder)
            {
                if (feature.props.TryGetValue(key, out var value))
                {
                    Attributes.Add(new AttributeItem { Key = key, Value = value });
                }
                else
                {
                    Attributes.Add(new AttributeItem { Key = key, Value = "" });
                }
            }

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void Save()
        {
            feature.Name = FeatureName;

            foreach (var attr in Attributes)
            {
                feature.props[attr.Key] = attr.Value;
            }

            CloseWindow?.Invoke(this, true);
        }

        private void Cancel() => CloseWindow?.Invoke(this, false);
    }
}