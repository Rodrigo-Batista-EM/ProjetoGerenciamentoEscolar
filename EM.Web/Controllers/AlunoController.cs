using Microsoft.AspNetCore.Mvc;
using EM.Domain;
using EM.Domain.Utilitarios;
using EM.Repository;
using Em.Web.Models;

namespace EM.Web.Controllers
{
    public class AlunoController : Controller
    {
        private readonly RepositorioAluno _repositorioAluno;
        private readonly RepositorioCidade _repositorioCidade;

        public AlunoController()
        {
            _repositorioAluno = new RepositorioAluno();
            _repositorioCidade = new RepositorioCidade();
        }

        // GET: Aluno
        public IActionResult Index()
        {
            var alunos = _repositorioAluno.GetAll()
                .Select(a => new AlunoViewModel
                {
                    Matricula = a.Matricula,
                    Nome = a.Nome,
                    CPF = a.CPF,
                    Nascimento = a.Nascimento,
                    Sexo = a.Sexo,
                    CidadeCodigo = a.CidadeCodigo,
                    CidadeNome = a.CidadeNome,
                    UF = a.UF
                }).ToList();

            ViewBag.SearchTypes = new[] { "nome", "matricula" };
            return View(alunos);
        }

        // GET: Aluno/Create
        public IActionResult Create()
        {
            ViewBag.Cidades = _repositorioCidade.GetAll()
                .OrderBy(c => c.Nome)
                .ToList();
            ViewBag.Sexos = Enum.GetValues(typeof(EnumeradorSexo))
                .Cast<EnumeradorSexo>()
                .ToList();

            return View(new AlunoViewModel());
        }

        // POST: Aluno/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(AlunoViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Valida CPF se informado
                if (!string.IsNullOrWhiteSpace(model.CPF))
                {
                    if (!Validations.ValidarCPF(model.CPF))
                    {
                        ModelState.AddModelError("CPF", "CPF inválido");
                        CarregarViewBags();
                        return View(model);
                    }

                    // Verifica se CPF já existe
                    if (_repositorioAluno.CPFExiste(model.CPF))
                    {
                        ModelState.AddModelError("CPF", "CPF já cadastrado");
                        CarregarViewBags();
                        return View(model);
                    }
                }

                // Valida nome
                if (!Validations.ValidarNome(model.Nome))
                {
                    ModelState.AddModelError("Nome", "Nome deve ter entre 3 e 100 caracteres");
                    CarregarViewBags();
                    return View(model);
                }

                var aluno = new Aluno
                {
                    Nome = model.Nome?.Trim(),
                    CPF = model.CPF?.Replace(".", "").Replace("-", ""),
                    Nascimento = model.Nascimento,
                    Sexo = model.Sexo,
                    CidadeCodigo = model.CidadeCodigo
                };

                try
                {
                    _repositorioAluno.Add(aluno);
                    TempData["Success"] = "Aluno cadastrado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Erro ao cadastrar aluno: {ex.Message}");
                    CarregarViewBags();
                    return View(model);
                }
            }

            CarregarViewBags();
            return View(model);
        }

        // GET: Aluno/Report
        [HttpGet]
        public async Task<IActionResult> Report(string? searchType, string? searchValue, string? format = "csv")
        {
            try
            {
                // Coleta dados conforme filtros
                IEnumerable<Aluno> alunos;

                var type = (searchType ?? string.Empty).ToLowerInvariant();
                var value = (searchValue ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(value))
                {
                    alunos = _repositorioAluno.GetAll();
                }
                else
                {
                    alunos = type switch
                    {
                        "matricula" when int.TryParse(value, out var matricula) =>
                            new[] { _repositorioAluno.GetByMatricula(matricula) }.Where(a => a != null)!,
                        "cpf" =>
                            new[] { _repositorioAluno.GetByCPF(value) }.Where(a => a != null)!,
                        "sexo" when Enum.TryParse<EnumeradorSexo>(value, true, out var sexo) =>
                            _repositorioAluno.GetBySexo(sexo),
                        "cidade" =>
                            BuscarPorCidades(value),
                        _ => _repositorioAluno.GetByConteudoNoNome(value)
                    };
                }

                // Enriquecer dados com informação da cidade quando disponível
                var alunosEnriquecidos = alunos.Select(a => PreencherCidade(a)).ToList();

                if (alunosEnriquecidos.Count == 0)
                {
                    return BadRequest("Nenhum dado encontrado para os filtros informados.");
                }

                // Geração do relatório
                switch ((format ?? "csv").ToLowerInvariant())
                {
                    case "csv":
                        var csvBytes = GerarCsv(alunosEnriquecidos);
                        var fileName = $"Relatorio_Alunos_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                        RegistrarEmissao("CSV", alunosEnriquecidos.Count, type, value);
                        return File(csvBytes, "text/csv", fileName);
                    default:
                        return BadRequest("Formato não suportado. Use 'csv'.");
                }
            }
            catch (Exception ex)
            {
                // Tratamento de erros
                return StatusCode(500, $"Falha na geração do relatório: {ex.Message}");
            }
        }

        private IEnumerable<Aluno> BuscarPorCidades(string nomeCidade)
        {
            var cidades = _repositorioCidade.GetByNome(nomeCidade)?.ToList() ?? new List<Cidade>();
            if (cidades.Count == 0) return Enumerable.Empty<Aluno>();

            var alunos = new List<Aluno>();
            foreach (var cidade in cidades)
            {
                alunos.AddRange(_repositorioAluno.GetByCidade(cidade.Codigo));
            }
            return alunos;
        }

        private Aluno PreencherCidade(Aluno a)
        {
            if (a?.CidadeCodigo is int cod)
            {
                var c = _repositorioCidade.GetByCodigo(cod);
                if (c != null)
                {
                    a.CidadeNome = c.Nome;
                    a.UF = c.UF;
                }
            }
            return a!;
        }

        private byte[] GerarCsv(IEnumerable<Aluno> alunos)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Matricula;Nome;CPF;Nascimento;Sexo;Cidade;UF");
            foreach (var a in alunos)
            {
                var nascimento = a.Nascimento == default ? string.Empty : a.Nascimento.ToString("yyyy-MM-dd");
                var sexo = a.Sexo.ToString();
                sb.AppendLine(string.Join(";", new[]
                {
                    a.Matricula.ToString(),
                    EscaparCsv(a.Nome),
                    a.CPF ?? string.Empty,
                    nascimento,
                    sexo,
                    EscaparCsv(a.CidadeNome ?? string.Empty),
                    a.UF ?? string.Empty
                }));
            }
            return System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        }

        private string EscaparCsv(string? valor)
        {
            if (string.IsNullOrEmpty(valor)) return string.Empty;
            var precisaAspas = valor.Contains(';') || valor.Contains('"') || valor.Contains('\n');
            var v = valor.Replace("\"", "\"\"");
            return precisaAspas ? $"\"{v}\"" : v;
        }

        private void RegistrarEmissao(string formato, int total, string? searchType, string? searchValue)
        {
            // Se houver módulo de auditoria/log, integrar aqui.
            // Por enquanto, registrar de forma simples.
            Console.WriteLine($"[AUDIT] Relatório de Alunos emitido | Formato={formato} | Total={total} | Filtro={searchType}:{searchValue} | Data={DateTime.Now:O}");
        }

        // GET: Aluno/Edit/5
        public IActionResult Edit(int id)
        {
            var aluno = _repositorioAluno.GetByMatricula(id);
            if (aluno == null)
            {
                TempData["Error"] = "Aluno não encontrado";
                return RedirectToAction(nameof(Index));
            }

            var model = new AlunoViewModel
            {
                Matricula = aluno.Matricula,
                Nome = aluno.Nome,
                CPF = aluno.CPF,
                Nascimento = aluno.Nascimento,
                Sexo = aluno.Sexo,
                CidadeCodigo = aluno.CidadeCodigo
            };

            CarregarViewBags();
            return View(model);
        }

        // POST: Aluno/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, AlunoViewModel model)
        {
            if (id != model.Matricula)
            {
                TempData["Error"] = "Aluno não encontrado";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrWhiteSpace(model.CPF))
                {
                    if (!Validations.ValidarCPF(model.CPF))
                    {
                        ModelState.AddModelError("CPF", "CPF inválido");
                        CarregarViewBags();
                        return View(model);
                    }

                    // Verifica se CPF já existe (excluindo o aluno atual)
                    if (_repositorioAluno.CPFExiste(model.CPF, model.Matricula))
                    {
                        ModelState.AddModelError("CPF", "CPF já cadastrado para outro aluno");
                        CarregarViewBags();
                        return View(model);
                    }
                }

                if (!Validations.ValidarNome(model.Nome))
                {
                    ModelState.AddModelError("Nome", "Nome deve ter entre 3 e 100 caracteres");
                    CarregarViewBags();
                    return View(model);
                }

                var aluno = new Aluno
                {
                    Matricula = model.Matricula,
                    Nome = model.Nome?.Trim(),
                    CPF = model.CPF?.Replace(".", "").Replace("-", ""),
                    Nascimento = model.Nascimento,
                    Sexo = model.Sexo,
                    CidadeCodigo = model.CidadeCodigo
                };

                try
                {
                    _repositorioAluno.Update(aluno);
                    TempData["Success"] = "Aluno atualizado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Erro ao atualizar aluno: {ex.Message}");
                    CarregarViewBags();
                    return View(model);
                }
            }

            CarregarViewBags();
            return View(model);
        }

        // GET: Aluno/Details/5
        public IActionResult Details(int id)
        {
            var aluno = _repositorioAluno.GetByMatricula(id);
            if (aluno == null)
            {
                TempData["Error"] = "Aluno não encontrado";
                return RedirectToAction(nameof(Index));
            }

            // Carrega dados da cidade se houver
            if (aluno.CidadeCodigo.HasValue)
            {
                var cidade = _repositorioCidade.GetByCodigo(aluno.CidadeCodigo.Value);
                if (cidade != null)
                {
                    aluno.CidadeNome = cidade.Nome;
                    aluno.UF = cidade.UF;
                }
            }

            var model = new AlunoViewModel
            {
                Matricula = aluno.Matricula,
                Nome = aluno.Nome,
                CPF = aluno.CPF,
                Nascimento = aluno.Nascimento,
                Sexo = aluno.Sexo,
                CidadeCodigo = aluno.CidadeCodigo,
                CidadeNome = aluno.CidadeNome,
                UF = aluno.UF
            };

            return View(model);
        }

        // POST: Aluno/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                var aluno = _repositorioAluno.GetByMatricula(id);
                if (aluno != null)
                {
                    _repositorioAluno.Remove(aluno);
                    TempData["Success"] = "Aluno excluído com sucesso!";
                }
                else
                {
                    TempData["Error"] = "Aluno não encontrado";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erro ao excluir aluno: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Aluno/Search
        public IActionResult Search(string searchType, string searchValue)
        {
            if (string.IsNullOrWhiteSpace(searchValue))
                return RedirectToAction(nameof(Index));

            try
            {
                var alunos = searchType?.ToLower() switch
                {
                    "matricula" when int.TryParse(searchValue, out int matricula) =>
                        new[] { _repositorioAluno.GetByMatricula(matricula) }.Where(a => a != null),
                    "cpf" =>
                        new[] { _repositorioAluno.GetByCPF(searchValue) }.Where(a => a != null),
                    "sexo" when Enum.TryParse<EnumeradorSexo>(searchValue, true, out var sexo) =>
                        _repositorioAluno.GetBySexo(sexo),
                    _ => _repositorioAluno.GetByConteudoNoNome(searchValue)
                };

                var model = alunos.Select(a => new AlunoViewModel
                {
                    Matricula = a.Matricula,
                    Nome = a.Nome,
                    CPF = a.CPF,
                    Nascimento = a.Nascimento,
                    Sexo = a.Sexo,
                    CidadeCodigo = a.CidadeCodigo,
                    CidadeNome = a.CidadeNome,
                    UF = a.UF
                }).ToList();

                ViewBag.SearchTypes = new[] { "nome", "matricula", "cpf", "sexo" };
                ViewBag.SearchType = searchType;
                ViewBag.SearchValue = searchValue;

                return View("Index", model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erro na pesquisa: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        private void CarregarViewBags()
        {
            ViewBag.Cidades = _repositorioCidade.GetAll()
                .OrderBy(c => c.Nome)
                .ToList();
            ViewBag.Sexos = Enum.GetValues(typeof(EnumeradorSexo))
                .Cast<EnumeradorSexo>()
                .ToList();
        }
    }
}