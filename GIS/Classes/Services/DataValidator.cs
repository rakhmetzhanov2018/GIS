using System;

namespace GIS.Classes.Services
{
    public static class DataValidator
    {
        public static bool Validate(string value, string dataType, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(value))
            {
                if (dataType != "String")
                {
                    errorMessage = "Поле не может быть пустым";
                    return false;
                }
                return true;
            }

            switch (dataType)
            {
                case "Boolean":
                    if (!bool.TryParse(value, out _))
                        errorMessage = "Введите true или false";
                    break;
                case "Integer":
                    if (!int.TryParse(value, out _))
                        errorMessage = "Введите целое число";
                    break;
                case "Double":
                    if (!double.TryParse(value, out _))
                        errorMessage = "Введите число (разделитель точка)";
                    break;
                case "DateTime":
                    if (!DateTime.TryParse(value, out _))
                        errorMessage = "Введите дату (например, 01.01.2023)";
                    break;
                case "String":
                    return true;
                default:
                    errorMessage = "Неизвестный тип данных";
                    break;
            }
            return string.IsNullOrEmpty(errorMessage);
        }
    }
}