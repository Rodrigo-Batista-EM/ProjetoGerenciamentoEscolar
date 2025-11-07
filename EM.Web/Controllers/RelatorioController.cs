using EM.Domain;
using EM.Repository;
using EM.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EM.Web.Controllers
{
    public class RelatorioController : Controller
    {
        private readonly IRelatorioService _relatorioService;
        private readonly RepositorioAluno _repoAluno;
        private readonly RepositorioCidade _repoCidade;

        public RelatorioController(IRelatorioService relatorioService, RepositorioAluno repoAluno, RepositorioCidade repoCidade)
        {
            _relatorioService = relatorioService;
            _repoAluno = repoAluno;
            _repoCidade = repoCidade;
        }

        // GET: /Relatorio/AlunosPDF
        [HttpGet]
        public IActionResult AlunosPDF(string? searchType, string? searchValue)
        {
            try
            {
                IEnumerable<Aluno> alunos;

                var type = (searchType ?? string.Empty).ToLowerInvariant();
                var value = (searchValue ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(value))
                {
                    alunos = _repoAluno.GetAll();
                }
                else
                {
                    alunos = type switch
                    {
                        "matricula" when int.TryParse(value, out var matricula) =>
                            new[] { _repoAluno.GetByMatricula(matricula) }.Where(a => a != null).Cast<Aluno>(),
                        "cpf" =>
                            new[] { _repoAluno.GetByCPF(value) }.Where(a => a != null).Cast<Aluno>(),
                        "sexo" when Enum.TryParse<EnumeradorSexo>(value, true, out var sexo) =>
                            _repoAluno.GetBySexo(sexo),
                        "cidade" =>
                            BuscarPorCidades(value),
                        _ => _repoAluno.GetByConteudoNoNome(value)
                    };
                }

                // Preencher dados de cidade (Nome/UF) antes de gerar o PDF
                alunos = (alunos ?? Enumerable.Empty<Aluno>())
                    .Select(PreencherCidade);

                // Verificar se há alunos
                var alunosList = alunos.ToList();
                
                // Gerar PDF e retornar inline
                var bytes = _relatorioService.GerarRelatorioAlunosPDF(alunosList);
                Response.Headers["Content-Disposition"] = "inline; filename=Relatorio_Alunos.pdf";
                return File(bytes, "application/pdf");
            }
            catch (Exception ex)
            {
                // Em caso de erro, retornar mensagem detalhada
                return StatusCode(500, $"Erro ao gerar relatório: {ex.Message}\nStack: {ex.StackTrace}");
            }
        }

        private IEnumerable<Aluno> BuscarPorCidades(string value)
        {
            // O repositório de alunos não tem busca direta por nome da cidade,
            // então vamos buscar por conteúdo no nome e deixar o filtro por cidade
            // a cargo da tela se necessário. Mantemos comportamento semelhante ao atual.
            return _repoAluno.GetByConteudoNoNome(value);
        }

        private Aluno PreencherCidade(Aluno a)
        {
            if (a?.CidadeCodigo is int cod)
            {
                var c = _repoCidade.GetByCodigo(cod);
                if (c != null)
                {
                    a.CidadeNome = c.Nome;
                    a.UF = c.UF;
                }
            }
            return a!;
        }
    }
}