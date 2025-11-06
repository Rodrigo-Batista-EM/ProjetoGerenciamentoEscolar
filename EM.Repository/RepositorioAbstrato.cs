using System.Data;
using System.Linq.Expressions;
using EM.Domain.Interface;
using EM.Repository.Banco;

namespace EM.Repository
{
    public abstract class RepositorioAbstrato<T> where T : IEntidade
    {
        protected abstract string TableName { get; }
        protected abstract string PrimaryKeyColumn { get; }
        protected abstract T MapFromReader(IDataReader reader);
        protected abstract void AddInsertParameters(IDbCommand cmd, T objeto);
        protected abstract void AddUpdateParameters(IDbCommand cmd, T objeto);

        public virtual void Add(T objeto)
        {
            using var cn = DBHelper.Instancia.CrieConexao();
            cn.Open();
            using var cmd = DBHelper.Instancia.CreateCommand(cn); // CORRIGIDO

            cmd.CommandText = GetInsertCommand();
            AddInsertParameters(cmd, objeto);
            cmd.ExecuteNonQuery();
        }

        public virtual void Remove(T objeto)
        {
            using var cn = DBHelper.Instancia.CrieConexao();
            cn.Open();
            using var cmd = DBHelper.Instancia.CreateCommand(cn); // CORRIGIDO

            cmd.CommandText = $"DELETE FROM {TableName} WHERE {PrimaryKeyColumn} = @Id";
            AddDeleteParameters(cmd, objeto);
            cmd.ExecuteNonQuery();
        }

        public virtual void Update(T objeto)
        {
            using var cn = DBHelper.Instancia.CrieConexao();
            cn.Open();
            using var cmd = DBHelper.Instancia.CreateCommand(cn); // CORRIGIDO

            cmd.CommandText = GetUpdateCommand();
            AddUpdateParameters(cmd, objeto);
            cmd.ExecuteNonQuery();
        }

        public virtual IEnumerable<T> GetAll()
        {
            var lista = new List<T>();

            using var cn = DBHelper.Instancia.CrieConexao();
            cn.Open();
            using var cmd = DBHelper.Instancia.CreateCommand(cn); // CORRIGIDO

            cmd.CommandText = $"SELECT * FROM {TableName}";
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(MapFromReader(reader));
            }

            return lista;
        }

        public virtual IEnumerable<T> Get(Expression<Func<T, bool>> predicate)
        {
            return GetAll().AsQueryable().Where(predicate);
        }

        public virtual T GetById(int id)
        {
            using var cn = DBHelper.Instancia.CrieConexao();
            cn.Open();
            using var cmd = DBHelper.Instancia.CreateCommand(cn); // CORRIGIDO

            cmd.CommandText = $"SELECT * FROM {TableName} WHERE {PrimaryKeyColumn} = @Id";
            cmd.CreateParameter("@Id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return MapFromReader(reader);
            }

            return default(T);
        }

        public virtual bool Exists(int id)
        {
            using var cn = DBHelper.Instancia.CrieConexao();
            cn.Open();
            using var cmd = DBHelper.Instancia.CreateCommand(cn); // CORRIGIDO

            cmd.CommandText = $"SELECT 1 FROM {TableName} WHERE {PrimaryKeyColumn} = @Id";
            cmd.CreateParameter("@Id", id);

            using var reader = cmd.ExecuteReader();
            return reader.Read();
        }

        public virtual int Count()
        {
            using var cn = DBHelper.Instancia.CrieConexao();
            cn.Open();
            using var cmd = DBHelper.Instancia.CreateCommand(cn); // CORRIGIDO

            cmd.CommandText = $"SELECT COUNT(*) FROM {TableName}";
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        protected abstract string GetInsertCommand();
        protected abstract string GetUpdateCommand();
        protected abstract void AddDeleteParameters(IDbCommand cmd, T objeto);
    }
}