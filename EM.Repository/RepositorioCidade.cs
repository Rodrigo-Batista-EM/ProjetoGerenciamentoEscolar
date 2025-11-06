using System.Data;
using EM.Domain;
using EM.Domain.Utilitarios;
using EM.Repository.Banco;

namespace EM.Repository
{
    public class RepositorioCidade : RepositorioAbstrato<Cidade>
    {
        protected override string TableName => "TBCIDADE";
        protected override string PrimaryKeyColumn => "CIDCODIGO";

        protected override string GetInsertCommand()
        {
            return @"INSERT INTO TBCIDADE (CIDNOME, CIDUF) 
                    VALUES (@Nome, @UF)";
        }

        protected override string GetUpdateCommand()
        {
            return @"UPDATE TBCIDADE SET 
                    CIDNOME = @Nome, 
                    CIDUF = @UF 
                    WHERE CIDCODIGO = @Codigo";
        }

        protected override void AddInsertParameters(IDbCommand cmd, Cidade cidade)
        {
            cmd.CreateParameter("@Nome", cidade.Nome);
            cmd.CreateParameter("@UF", cidade.UF?.ToUpper());
        }

        protected override void AddUpdateParameters(IDbCommand cmd, Cidade cidade)
        {
            AddInsertParameters(cmd, cidade);
            cmd.CreateParameter("@Codigo", cidade.Codigo);
        }

        protected override void AddDeleteParameters(IDbCommand cmd, Cidade cidade)
        {
            cmd.CreateParameter("@Id", cidade.Codigo);
        }

        protected override Cidade MapFromReader(IDataReader reader)
        {
            return new Cidade
            {
                Codigo = reader["CIDCODIGO"].ToObject<int>(),
                Nome = reader["CIDNOME"].ToObject<string>(),
                UF = reader["CIDUF"].ToObject<string>()
            };
        }

        public Cidade GetByCodigo(int codigo)
        {
            return GetById(codigo);
        }

        public IEnumerable<Cidade> GetByUF(string uf)
        {
            return Get(c => c.UF == uf.ToUpper());
        }

        public IEnumerable<Cidade> GetByNome(string nome)
        {
            var lista = new List<Cidade>();

            using var cn = DBHelper.Instancia.CrieConexao();
            cn.Open();
            using var cmd = cn.CreateCommand();

            cmd.CommandText = "SELECT * FROM TBCIDADE WHERE UPPER(CIDNOME) CONTAINING UPPER(@Nome) ORDER BY CIDNOME";
            cmd.CreateParameter("@Nome", nome);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(MapFromReader(reader));
            }

            return lista;
        }

        public IEnumerable<string> GetUFs()
        {
            var ufs = new List<string>();

            using var cn = DBHelper.Instancia.CrieConexao();
            cn.Open();
            using var cmd = cn.CreateCommand();

            cmd.CommandText = "SELECT DISTINCT CIDUF FROM TBCIDADE ORDER BY CIDUF";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                ufs.Add(reader["CIDUF"].ToObject<string>());
            }

            return ufs;
        }

        public bool CidadeTemAlunos(int codigoCidade)
        {
            using var cn = DBHelper.Instancia.CrieConexao();
            cn.Open();
            using var cmd = cn.CreateCommand();

            cmd.CommandText = "SELECT 1 FROM TBALUNO WHERE ALUCODCIDADE = @CodigoCidade";
            cmd.CreateParameter("@CodigoCidade", codigoCidade);

            using var reader = cmd.ExecuteReader();
            return reader.Read();
        }
    }
}