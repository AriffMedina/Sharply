using Microsoft.AspNetCore.Mvc;

namespace Sharply.Api.Controllers
{
    public class SkillController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
