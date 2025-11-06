
namespace EM.Domain.Utilitarios
{
    public static class Uteis
    {
        public static object ToBD(this object valor)
        {
            if (valor == null ||
                (valor is string str && string.IsNullOrWhiteSpace(str)) ||
                (valor is int i && i == 0) ||
                (valor is DateTime dt && dt == DateTime.MinValue))
            {
                return DBNull.Value;
            }
            return valor;
        }

        public static T ToObject<T>(this object valor)
        {
            if (valor == null || valor == DBNull.Value)
                return default(T);

            return (T)Convert.ChangeType(valor, typeof(T));
        }

        public static DateTime? ToDateTimeNullable(this object valor)
        {
            if (valor == null || valor == DBNull.Value)
                return null;

            return Convert.ToDateTime(valor);
        }
    }
}