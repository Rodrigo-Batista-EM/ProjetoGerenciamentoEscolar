using EM.Domain;

namespace EM.Web.Interfaces
{
    public interface IRelatorioService
    {
        byte[] GerarRelatorioAlunosPDF(IEnumerable<Aluno> alunos);
        byte[] GerarRelatorioCidadesPDF(IEnumerable<Cidade> cidades);
    }
}