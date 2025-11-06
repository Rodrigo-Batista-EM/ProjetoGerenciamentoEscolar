public static class DataReaderExtensions
{
    public static int? ToIntNullable(this object value)
    {
        if (value == null || value == DBNull.Value)
            return null;

        if (int.TryParse(value.ToString(), out int result))
            return result;

        return null;
    }

    public static string ToStringSafe(this object value)
    {
        return value?.ToString() ?? string.Empty;
    }
}