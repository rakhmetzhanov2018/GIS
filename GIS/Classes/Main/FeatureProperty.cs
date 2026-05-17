namespace GIS.Classes.Main
{
    public class FeatureProperty
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public string? DefaultValue { get; set; }

        public static string DefineDataType(object value)
        {
            if (value == null) return "NULL";

            string strValue = value.ToString();

            if (bool.TryParse(strValue, out _))
                return "Boolean";

            if (int.TryParse(strValue, out _))
                return "Integer";

            if (double.TryParse(strValue, out _))
                return "Double";

            if (DateTime.TryParse(strValue, out _))
                return "DateTime";

            return "String";
        }
    }
}
