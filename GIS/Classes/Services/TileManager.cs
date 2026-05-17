using System.Net.Http;
using System.Windows.Media.Imaging;

namespace GIS.Classes.Services
{
    public static class TileManager
    {
        private static readonly HttpClient httpClient = new();
        static TileManager()
        {
            httpClient.DefaultRequestHeaders.Add("User-Agent", "GIS_Learning_App/1.0 student project (wortwerd82@gmail.com)");
        }
        public static int LonToTileX(double lon, int z)
        {
            return (int)Math.Floor((lon + 180) / 360 * Math.Pow(2, z));
        }
        public static int LatToTileY(double lat, int z)
        {
            double latInRadiants = lat * Math.PI / 180;
            double sinLat = Math.Sin(latInRadiants);
            double y = 0.5 - Math.Log((1 + sinLat) / (1 - sinLat)) / (4 * Math.PI);
            return (int)Math.Floor(y * Math.Pow(2, z));
        }
        public static double TileXToLon(int x, int z)
        {
            return x / (double)Math.Pow(2, z) * 360 - 180;
        }
        public static double TileYToLat(int y, int z)
        {
            double n = Math.PI - 2 * Math.PI * y / Math.Pow(2, z);
            return 180 / Math.PI * Math.Atan(0.5 * (Math.Exp(n) - Math.Exp(-n)));
        }
        public static string GetTileUrl(int x, int y, int z, OSMTileType layerType)
        {
            if (layerType == OSMTileType.Street)
                return $"https://tile.openstreetmap.org/{z}/{x}/{y}.png";
            else if (layerType == OSMTileType.Satellite)
                return $"https://mt1.google.com/vt/lyrs=s&x={x}&y={y}&z={z}";
            else
                throw new Exception("Что-то не так с типами OSM");
        }
        public static async Task<BitmapImage> LoadTileAsyncFunction(string url, CancellationToken token)
        {
            try
            {
                var response = await httpClient.GetAsync(url, token);
                if (!response.IsSuccessStatusCode) return null;

                byte[] data = await response.Content.ReadAsByteArrayAsync(token);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = new System.IO.MemoryStream(data);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                return bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки {url}: {ex.Message}");
                return null;
            }
        }
    }
}