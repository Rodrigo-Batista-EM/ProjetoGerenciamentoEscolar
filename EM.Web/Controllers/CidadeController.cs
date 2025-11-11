using Em.Web.Models;
using EM.Domain;
using EM.Domain.Utilitarios;
using EM.Repository;
using Microsoft.AspNetCore.Mvc;

namespace EM.Web.Controllers
{
    public class CidadeController : Controller
    {
        private readonly RepositorioCidade _repositorioCidade;
        private readonly RepositorioAluno _repositorioAluno;

        public CidadeController()
        {
            _repositorioCidade = new RepositorioCidade();
            _repositorioAluno = new RepositorioAluno();
        }

        public IActionResult Index()
        {
            var cidades = _repositorioCidade.GetAll().OrderBy(c => c.Codigo)
                .Select(c => new CidadeViewModel
                {
                    Codigo = c.Codigo,
                    Nome = c.Nome,
                    UF = c.UF
                }).ToList();

            ViewBag.UFs = _repositorioCidade.GetUFs();
            return View(cidades);
        }

        public IActionResult Create()
        {
            ViewBag.UFs = _repositorioCidade.GetUFs();
            return View(new CidadeViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CidadeViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (!Validations.ValidarNome(model.Nome))
                {
                    ModelState.AddModelError("Nome", "Nome deve ter entre 3 e 100 caracteres");
                    ViewBag.UFs = _repositorioCidade.GetUFs();
                    return View(model);
                }

                var cidade = new Cidade
                {
                    Nome = model.Nome?.Trim(),
                    UF = model.UF?.ToUpper()
                };

                try
                {
                    _repositorioCidade.Add(cidade);
                    TempData["Success"] = "Cidade cadastrada com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Erro ao cadastrar cidade: {ex.Message}");
                    ViewBag.UFs = _repositorioCidade.GetUFs();
                    return View(model);
                }
            }

            ViewBag.UFs = _repositorioCidade.GetUFs();
            return View(model);
        }

        public IActionResult Edit(int id)
        {
            var cidade = _repositorioCidade.GetByCodigo(id);
            if (cidade == null)
            {
                TempData["Error"] = "Cidade não encontrada";
                return RedirectToAction(nameof(Index));
            }

            var model = new CidadeViewModel
            {
                Codigo = cidade.Codigo,
                Nome = cidade.Nome,
                UF = cidade.UF
            };

            ViewBag.UFs = _repositorioCidade.GetUFs();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, CidadeViewModel model)
        {
            if (id != model.Codigo)
            {
                TempData["Error"] = "Cidade não encontrada";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                if (!Validations.ValidarNome(model.Nome))
                {
                    ModelState.AddModelError("Nome", "Nome deve ter entre 3 e 100 caracteres");
                    ViewBag.UFs = _repositorioCidade.GetUFs();
                    return View(model);
                }

                var cidade = new Cidade
                {
                    Codigo = model.Codigo,
                    Nome = model.Nome?.Trim(),
                    UF = model.UF?.ToUpper()
                };

                try
                {
                    _repositorioCidade.Update(cidade);
                    TempData["Success"] = "Cidade atualizada com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Erro ao atualizar cidade: {ex.Message}");
                    ViewBag.UFs = _repositorioCidade.GetUFs();
                    return View(model);
                }
            }

            ViewBag.UFs = _repositorioCidade.GetUFs();
            return View(model);
        }

        // GET: Cidade/Details/5
        public IActionResult Details(int id)
        {
            var cidade = _repositorioCidade.GetByCodigo(id);
            if (cidade == null)
            {
                TempData["Error"] = "Cidade não encontrada";
                return RedirectToAction(nameof(Index));
            }

            var model = new CidadeViewModel
            {
                Codigo = cidade.Codigo,
                Nome = cidade.Nome,
                UF = cidade.UF
            };

            // Carrega alunos da cidade
            ViewBag.Alunos = _repositorioAluno.GetByCidade(id);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                // Verifica se a cidade tem alunos vinculados
                if (_repositorioCidade.CidadeTemAlunos(id))
                {
                    TempData["Error"] = "Não é possível excluir a cidade pois existem alunos vinculados a ela.";
                    return RedirectToAction(nameof(Index));
                }

                var cidade = _repositorioCidade.GetByCodigo(id);
                if (cidade != null)
                {
                    _repositorioCidade.Remove(cidade);
                    TempData["Success"] = "Cidade excluída com sucesso!";
                }
                else
                {
                    TempData["Error"] = "Cidade não encontrada";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erro ao excluir cidade: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Cidade/Search
        public IActionResult Search(string searchType, string searchValue)
        {
            if (string.IsNullOrWhiteSpace(searchValue))
                return RedirectToAction(nameof(Index));

            try
            {
                var cidades = searchType?.ToLower() switch
                {
                    "uf" => _repositorioCidade.GetByUF(searchValue.ToUpper()),
                    _ => _repositorioCidade.GetByNome(searchValue)
                };

                var model = cidades.Select(c => new CidadeViewModel
                {
                    Codigo = c.Codigo,
                    Nome = c.Nome,
                    UF = c.UF
                }).ToList();

                ViewBag.UFs = _repositorioCidade.GetUFs();
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
    }
}