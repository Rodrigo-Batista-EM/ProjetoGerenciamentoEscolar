using EM.Repository.Banco;
using Microsoft.AspNetCore.Mvc;

namespace EM.Web.Controllers
{
    [Route("health")]
    public class HealthController : Controller
    {
        [HttpGet("db")]
        public IActionResult Db()
        {
            try
            {
                using var cn = DBHelper.Instancia.CrieConexao();
                cn.Open();
                return Ok(new { status = "ok" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }
    }
}