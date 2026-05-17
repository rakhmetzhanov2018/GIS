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
    public enum OSMTileType
    {
        None,
        Satellite,
        Street
    }
}
