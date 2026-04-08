using GIS.Classes.Main;
using System.Collections.ObjectModel;
using System.Linq;

namespace GIS.Services
{
    public class LayerManager
    {
        public ObservableCollection<Layer> layersList { get; } = new();

        private Layer _selectedLayer;
        public Layer SelectedLayer
        {
            get => _selectedLayer;
            set => _selectedLayer = value;
        }

        public void AddLayer(Layer layer)
        {
            layersList.Add(layer);
        }

        public void RemoveLayer(Layer layer)
        {
            layersList.Remove(layer);
        }

        public void MoveLayerUp(Layer layer)
        {
            int index = layersList.IndexOf(layer);
            if (index > 0)
                layersList.Move(index, index - 1);
        }

        public void MoveLayerDown(Layer layer)
        {
            int index = layersList.IndexOf(layer);
            if (index < layersList.Count - 1)
                layersList.Move(index, index + 1);
        }

        public Layer FindLayerByFeature(Feature feature)
        {
            foreach (Layer layer in layersList)
            {
                if (layer.ObjectList.Contains(feature))
                    return layer;
            }
            throw new Exception("Попытка удалить уже удалённый объект.");
        }
    }
}