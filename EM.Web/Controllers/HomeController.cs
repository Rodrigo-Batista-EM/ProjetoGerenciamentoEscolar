using EM.Web.Models;
using EM.Repository;
using Microsoft.AspNetCore.Mvc;

namespace EM.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly RepositorioAluno _repositorioAluno;
        private readonly RepositorioCidade _repositorioCidade;

        public HomeController()
        {
            _repositorioAluno = new RepositorioAluno();
            _repositorioCidade = new RepositorioCidade();
        }

        public IActionResult Index()
        {
            ViewBag.TotalAlunos = _repositorioAluno.Count();
            ViewBag.TotalCidades = _repositorioCidade.Count();
            ViewBag.UltimasCidades = _repositorioCidade.GetAll()
                .OrderByDescending(c => c.Codigo)
                .Take(5)
                .ToList();

            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Sistema de Gestão Escolar";
            ViewData["Description"] = "Sistema desenvolvido para gerenciamento de alunos e cidades.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Informações de contato";
            ViewData["Email"] = "contato@escola.com";
            ViewData["Phone"] = "(11) 99999-9999";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}