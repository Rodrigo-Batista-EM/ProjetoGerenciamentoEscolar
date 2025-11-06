using FirebirdSql.Data.FirebirdClient;
using System.Data;

namespace EM.Repository.Banco
{
    public static class ParameterExtensions
    {
        public static void CreateParameter(this IDbCommand command, string parameterName, object value)
        {
            // Converter valor para o tipo adequado para o Firebird
            object dbValue = value ?? DBNull.Value;

            var parameter = new FbParameter(parameterName, dbValue);
            command.Parameters.Add(parameter);
        }
    }
}