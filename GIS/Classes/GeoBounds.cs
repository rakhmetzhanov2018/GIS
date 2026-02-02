using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GIS.Classes
{
    public struct GeoBounds
    {
        public double MinLon { get; set; }
        public double MaxLon { get; set; }
        public double MinLat { get; set; }
        public double MaxLat { get; set; }

        public GeoBounds()
        {
            MinLon = double.MaxValue;
            MaxLon = double.MinValue;
            MinLat = double.MaxValue;
            MaxLat = double.MinValue;
        }

        public GeoBounds(double minLon, double maxLon, double minLat, double maxLat)
        {
            MinLon = minLon;
            MaxLon = maxLon;
            MinLat = minLat;
            MaxLat = maxLat;
        }

        public void Update(double lon, double lat)
        {
            MinLon = Math.Min(MinLon, lon);
            MaxLon = Math.Max(MaxLon, lon);
            MinLat = Math.Min(MinLat, lat);
            MaxLat = Math.Max(MaxLat, lat);
        }

        public void Update(GeoBounds otherBounds)
        {
            MinLon = Math.Min(MinLon, otherBounds.MinLon);
            MaxLon = Math.Max(MaxLon, otherBounds.MaxLon);
            MinLat = Math.Min(MinLat, otherBounds.MinLat);
            MaxLat = Math.Max(MaxLat, otherBounds.MaxLat);
        
        }
        public override string ToString()
        {
            return $"MinLon: {MinLon}; MaxLon: {MaxLon}; MinLat: {MinLat}; MaxLat: {MaxLat}";
        }
    }
}
