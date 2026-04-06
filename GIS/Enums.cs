using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GIS
{
    public enum AttributeDataType
    {
        Integer,
        Double,
        String,
        Boolean,
        DateTime
    }
    public enum MapMode
    {
        Move,
        Select,
        Draw
    }

    public enum GeometryType
    {
        Point,
        LineString,
        Polygon
    }
}
