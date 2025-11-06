using EM.Domain.Interface;
using EM.Domain.Utilitarios;

namespace EM.Domain
{
    public class Aluno : IEntidade
    {
        public int Matricula { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string CPF { get; set; } = string.Empty;
        public DateTime Nascimento { get; set; }
        public EnumeradorSexo Sexo { get; set; }
        public int? CidadeCodigo { get; set; }
        public string CidadeNome { get; set; } = string.Empty;
        public string UF { get; set; } = string.Empty;

        public Aluno()
        {
            // Constructor vazio para serialização
        }

        public Aluno(int matricula, string nome, string cpf, DateTime nascimento, EnumeradorSexo sexo)
        {
            if (!Validations.ValidarNome(nome))
                throw new ArgumentException("Nome inválido");

            if (!Validations.ValidarCPF(cpf))
                throw new ArgumentException("CPF inválido");

            Matricula = matricula;
            Nome = nome;
            CPF = cpf;
            Nascimento = nascimento;
            Sexo = sexo;
        }

        public override bool Equals(object obj)
        {
            return obj is Aluno aluno && Matricula == aluno.Matricula;
        }

        public override int GetHashCode()
        {
            return Matricula.GetHashCode();
        }

        public override string ToString()
        {
            return $"Matrícula: {Matricula} - Nome: {Nome}";
        }
    }
}