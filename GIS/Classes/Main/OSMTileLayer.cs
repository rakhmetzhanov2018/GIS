using GIS.Classes.Main;
using GIS.Classes.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GIS.Classes.Layers
{
    public class OsmTileLayer : Layer, IDisposable
    {
        private readonly Canvas mapCanvas;
        private readonly Dictionary<String, Image> allTiles = new();
        private readonly Dictionary<String, CancellationTokenSource> loadingTasks = new();
        private int curZ = -1;
        private OSMTileType currentOSMLayerType = OSMTileType.None;
        private bool isDisposed = false;

        public OsmTileLayer(Canvas mapCanvas)
        {
            this.mapCanvas = mapCanvas;
            IsVisible = true;
            ShowInTree = false;
        }
        public void SetLayerType(OSMTileType layerType)
        {
            if (currentOSMLayerType == layerType) return;
            currentOSMLayerType = layerType;
            ForceClearAll();
            curZ = -1;
            UpdateTiles();
        }
        public override void UpdateAll()
        {
            UpdateTilePositions();
            UpdateTiles();
        }
        private void UpdateTilePositions()
        {
            if (currentOSMLayerType == OSMTileType.None) return;

            foreach (var kvp in allTiles)
            {
                string key = kvp.Key;
                var img = kvp.Value;
                var parts = key.Split('_');
                if (parts.Length != 3) continue;
                int x = int.Parse(parts[0]);
                int y = int.Parse(parts[1]);
                int z = int.Parse(parts[2]);

                if (z != curZ) continue;

                double minLon = TileManager.TileXToLon(x, z);
                double maxLon = TileManager.TileXToLon(x + 1, z);
                double minLat = TileManager.TileYToLat(y + 1, z);
                double maxLat = TileManager.TileYToLat(y, z);
                

                var topLeft = MapToCanvasTranslator.TranslateFromGeoToCanvasFinal(minLon, maxLat);
                var bottomRight = MapToCanvasTranslator.TranslateFromGeoToCanvasFinal(maxLon, minLat);

                double width = bottomRight.X - topLeft.X;
                double height = bottomRight.Y - topLeft.Y;

                if (width > 0 && height > 0)
                {
                    Canvas.SetLeft(img, topLeft.X);
                    Canvas.SetTop(img, topLeft.Y);
                    img.Width = width;
                    img.Height = height;
                }
            }
        }
        private void UpdateTiles()
        {
            if (currentOSMLayerType == OSMTileType.None)
            {
                ForceClearAll();
                return;
            }

            var visibleBounds = MapToCanvasTranslator.GetVisibleGeoBounds();
            if (visibleBounds.MinLon == double.MaxValue || visibleBounds.MinLon == visibleBounds.MaxLon)
                return;

            double degPerPixel = (visibleBounds.MaxLon - visibleBounds.MinLon) / MapToCanvasTranslator.CanvasSize.Width;
            int newZ = (int)Math.Round(Math.Log(360.0 / (degPerPixel * 256.0), 2));
            newZ = Math.Clamp(newZ, 0, 19);
            if (newZ != curZ)
            {
                ForceClearAll();
                curZ = newZ;
            }

            int minTileX = TileManager.LonToTileX(visibleBounds.MinLon, curZ) - 1;
            int maxTileX = TileManager.LonToTileX(visibleBounds.MaxLon, curZ) + 1;
            int minTileY = TileManager.LatToTileY(visibleBounds.MaxLat, curZ) - 1;
            int maxTileY = TileManager.LatToTileY(visibleBounds.MinLat, curZ) + 1;

            var keysToCancel = loadingTasks.Keys.Where(key =>
            {
                var parts = key.Split('_');
                if (parts.Length != 3) return false;
                int x = int.Parse(parts[0]);
                int y = int.Parse(parts[1]);
                return x < minTileX || x > maxTileX || y < minTileY || y > maxTileY;
            }).ToList();

            foreach (var key in keysToCancel)
            {
                if (loadingTasks.TryGetValue(key, out var cts))
                {
                    cts.Cancel();
                    loadingTasks.Remove(key);
                }
            }

            var toRemove = allTiles.Keys.Where(key =>
            {
                var parts = key.Split('_');
                if (parts.Length != 3) return false;
                int x = int.Parse(parts[0]);
                int y = int.Parse(parts[1]);
                return x < minTileX || x > maxTileX || y < minTileY || y > maxTileY;
            }).ToList();

            foreach (var key in toRemove)
            {
                mapCanvas.Children.Remove(allTiles[key]);
                allTiles.Remove(key);
            }

            for (int x = minTileX; x <= maxTileX; x++)
            {
                for (int y = minTileY; y <= maxTileY; y++)
                {
                    string key = $"{x}_{y}_{curZ}";
                    if (allTiles.ContainsKey(key) || loadingTasks.ContainsKey(key)) continue;

                    string url = TileManager.GetTileUrl(x, y, curZ, currentOSMLayerType);
                    LoadAndPositionTile(url, key, x, y, curZ);
                }
            }
        }
        private async void LoadAndPositionTile(string url, string key, int x, int y, int z)
        {
            if (curZ != z) return;

            var cts = new CancellationTokenSource();
            loadingTasks[key] = cts;

            try
            {
                var bitmap = await TileManager.LoadTileAsyncFunction(url, cts.Token);
                if (bitmap == null) return;

                if (curZ != z) return;

                var img = new Image
                {
                    Source = bitmap,
                    Width = 256,
                    Height = 256,
                    Stretch = Stretch.Fill
                };

                double leftLon = TileManager.TileXToLon(x, z);
                double rightLon = TileManager.TileXToLon(x + 1, z);
                double topLat = TileManager.TileYToLat(y, z);
                double bottomLat = TileManager.TileYToLat(y + 1, z);

                var topLeft = MapToCanvasTranslator.TranslateFromGeoToCanvasFinal(leftLon, topLat);
                var bottomRight = MapToCanvasTranslator.TranslateFromGeoToCanvasFinal(rightLon, bottomLat);

                double width = bottomRight.X - topLeft.X;
                double height = bottomRight.Y - topLeft.Y;

                if (width <= 0 || height <= 0) return;

                Canvas.SetLeft(img, topLeft.X);
                Canvas.SetTop(img, topLeft.Y);
                img.Width = width;
                img.Height = height;
                Canvas.SetZIndex(img, -1000);

                mapCanvas.Dispatcher.Invoke(() =>
                {
                    if (!mapCanvas.Children.Contains(img))
                    {
                        mapCanvas.Children.Add(img);
                        allTiles[key] = img;
                    }
                });
            }
            catch (OperationCanceledException) {}
            finally
            {
                loadingTasks.Remove(key);
            }
        }
        private void ForceClearAll()
        {
            foreach (var cts in loadingTasks.Values)
                cts.Cancel();
            loadingTasks.Clear();

            foreach (var img in allTiles.Values)
                mapCanvas.Children.Remove(img);
            allTiles.Clear();
        }
        public override void UpdateVisibility()
        {
            foreach (var img in allTiles.Values)
                img.Visibility = IsVisible ? Visibility.Visible : Visibility.Collapsed;
        }
        public void Dispose()
        {
            if (isDisposed) return;
            isDisposed = true;
            ForceClearAll();
        }
        public void ForceRefresh()
        {
            curZ = -1;
            UpdateAll();
        }
    }
}