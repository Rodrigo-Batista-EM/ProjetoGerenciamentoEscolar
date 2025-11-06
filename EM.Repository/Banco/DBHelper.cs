using FirebirdSql.Data.FirebirdClient;
using System.Data;

namespace EM.Repository.Banco
{
    public static class DBHelper
    {
        private static string connectionString =
            "User=SYSDBA;Password=masterkey;" +
            "Database=C:\\WorkRodrigo\\Projeto - Copia\\Projeto\\BANCO.FDB;" +
            "DataSource=localhost;Port=3055;Dialect=3;Charset=UTF8;ServerType=0;";

        public static void Configure(string conn)
        {
            if (!string.IsNullOrWhiteSpace(conn))
            {
                connectionString = conn;
            }
        }

        public static class Instancia
        {
            public static IDbConnection CrieConexao()
            {
                return new FbConnection(connectionString);
            }

            // MÉTODO CORRIGIDO - não é mais extension method
            public static IDbCommand CreateCommand(IDbConnection connection)
            {
                var cmd = connection.CreateCommand();
                cmd.CommandType = CommandType.Text;
                return cmd;
            }
        }
    }
}