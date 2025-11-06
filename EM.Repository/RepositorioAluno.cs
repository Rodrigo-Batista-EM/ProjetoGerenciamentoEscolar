using System.Data;
using EM.Domain;
using EM.Domain.Utilitarios;
using EM.Repository.Banco;

namespace EM.Repository
{
    public class RepositorioAluno : RepositorioAbstrato<Aluno>
    {
        protected override string TableName => "TBALUNO";
        protected override string PrimaryKeyColumn => "ALUMATRICULA";

        protected override string GetInsertCommand()
        {
            return @"INSERT INTO TBALUNO 
                    (ALUNOME, ALUCPF, ALUNASCIMENTO, ALUSEXO, ALUCODCIDADE) 
                    VALUES (@Nome, @CPF, @Nascimento, @Sexo, @CidadeCodigo)";
        }

        protected override string GetUpdateCommand()
        {
            return @"UPDATE TBALUNO SET 
                    ALUNOME = @Nome, 
                    ALUCPF = @CPF, 
                    ALUNASCIMENTO = @Nascimento, 
                    ALUSEXO = @Sexo, 
                    ALUCODCIDADE = @CidadeCodigo 
                    WHERE ALUMATRICULA = @Matricula";
        }

        protected override void AddInsertParameters(IDbCommand cmd, Aluno aluno)
        {
            cmd.CreateParameter("@Nome", aluno.Nome);
            cmd.CreateParameter("@CPF", aluno.CPF);
            cmd.CreateParameter("@Nascimento", aluno.Nascimento);
            cmd.CreateParameter("@Sexo", (int)aluno.Sexo);
            cmd.CreateParameter("@CidadeCodigo", aluno.CidadeCodigo);
        }

        protected override void AddUpdateParameters(IDbCommand cmd, Aluno aluno)
        {
            AddInsertParameters(cmd, aluno);
            cmd.CreateParameter("@Matricula", aluno.Matricula);
        }

        protected override void AddDeleteParameters(IDbCommand cmd, Aluno aluno)
        {
            cmd.CreateParameter("@Id", aluno.Matricula);
        }

        protected override Aluno MapFromReader(IDataReader reader)
        {
            return new Aluno
            {
                Matricula = reader["ALUMATRICULA"].ToObject<int>(),
                Nome = reader["ALUNOME"].ToObject<string>(),
                CPF = reader["ALUCPF"].ToObject<string>(),
                Nascimento = reader["ALUNASCIMENTO"].ToObject<DateTime>(),
                Sexo = (EnumeradorSexo)reader["ALUSEXO"].ToObject<int>(),
                CidadeCodigo = reader["ALUCODCIDADE"] as int?  // ← SOLUÇÃO SIMPLES
            };
        }

        public Aluno GetByMatricula(int matricula)
        {
            return GetById(matricula);
        }

        public IEnumerable<Aluno> GetByConteudoNoNome(string parteDoNome)
        {
            var lista = new List<Aluno>();

            using var cn = DBHelper.Instancia.CrieConexao();
            cn.Open();
            using var cmd = cn.CreateCommand();

            cmd.CommandText = @"
                SELECT A.*, C.CIDNOME, C.CIDUF 
                FROM TBALUNO A
                LEFT JOIN TBCIDADE C ON A.ALUCODCIDADE = C.CIDCODIGO
                WHERE UPPER(A.ALUNOME) CONTAINING UPPER(@Nome)
                ORDER BY A.ALUNOME";

            cmd.CreateParameter("@Nome", parteDoNome);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var aluno = MapFromReader(reader);
                aluno.CidadeNome = reader["CIDNOME"]?.ToObject<string>() ?? string.Empty;
                aluno.UF = reader["CIDUF"]?.ToObject<string>() ?? string.Empty;
                lista.Add(aluno);
            }

            return lista;
        }

        public Aluno? GetByCPF(string cpf)
        {
            using var cn = DBHelper.Instancia.CrieConexao();
            cn.Open();
            using var cmd = cn.CreateCommand();

            cmd.CommandText = "SELECT * FROM TBALUNO WHERE ALUCPF = @CPF";
            cmd.CreateParameter("@CPF", cpf);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return MapFromReader(reader);
            }

            return null;
        }

        public IEnumerable<Aluno> GetBySexo(EnumeradorSexo sexo)
        {
            return Get(a => a.Sexo == sexo);
        }

        public IEnumerable<Aluno> GetByCidade(int codigoCidade)
        {
            return Get(a => a.CidadeCodigo == codigoCidade);
        }

        public bool CPFExiste(string cpf, int? matriculaExcluir = null)
        {
            using var cn = DBHelper.Instancia.CrieConexao();
            cn.Open();
            using var cmd = cn.CreateCommand();

            if (matriculaExcluir.HasValue)
            {
                cmd.CommandText = "SELECT 1 FROM TBALUNO WHERE ALUCPF = @CPF AND ALUMATRICULA != @Matricula";
                cmd.CreateParameter("@Matricula", matriculaExcluir.Value);
            }
            else
            {
                cmd.CommandText = "SELECT 1 FROM TBALUNO WHERE ALUCPF = @CPF";
            }

            cmd.CreateParameter("@CPF", cpf);

            using var reader = cmd.ExecuteReader();
            return reader.Read();
        }
    }
}