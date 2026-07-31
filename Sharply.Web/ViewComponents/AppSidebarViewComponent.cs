using Microsoft.AspNetCore.Mvc;
using Sharply.Domain.Interfaces;
using Sharply.Web.ViewModels;
using System.Security.Claims;

namespace Sharply.Web.ViewComponents
{
    public class AppSidebarViewComponent : ViewComponent
    {
        private readonly IUserRepository _userRepository;

        // XP lineal por nivel de jugador: 100 XP = 1 nivel, sin techo.
        // Mismo valor que HomeController — si algún día se mueve a Domain, unificar ahí.
        private const int XpPerLevel = 100;

        public AppSidebarViewComponent(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync(string active)
        {
            var userId = int.Parse(((ClaimsPrincipal?)User)?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var user = await _userRepository.GetByIdAsync(userId);
            var totalXp = user?.TotalXp ?? 0;
            var playerLevel = (totalXp / XpPerLevel) + 1;
            var xpIntoLevel = totalXp % XpPerLevel;

            var model = new AppSidebarViewModel
            {
                Active = active,
                PlayerLevel = playerLevel,
                XpIntoLevel = xpIntoLevel,
                XpToNextLevel = XpPerLevel - xpIntoLevel
            };

            return View(model);
        }
    }
}
