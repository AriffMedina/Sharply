using Microsoft.AspNetCore.Mvc;

namespace Sharply.Api.Controllers
{
    public class SkillLogController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
