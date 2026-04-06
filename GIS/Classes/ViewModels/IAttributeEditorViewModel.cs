using GIS.Classes.Main;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace GIS.Classes.ViewModels
{
    public interface IAttributeEditorViewModel
    {
        ObservableCollection<FeatureProperty> Attributes { get; set; }
        FeatureProperty SelectedAttribute { get; set; }
        string NewAttributeName { get; set; }
        AttributeDataType NewAttributeType { get; set; }
        string NewAttributeDefaultValue { get; set; }
        IEnumerable<AttributeDataType> DTypes { get; }
        ICommand AddAttributeCommand { get; }
        ICommand RemoveSelectedAttributeCommand { get; }
    }
}