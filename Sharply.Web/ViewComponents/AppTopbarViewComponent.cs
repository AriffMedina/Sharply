using Microsoft.AspNetCore.Mvc;
using Sharply.Domain.Interfaces;
using Sharply.Web.ViewModels;
using System.Security.Claims;

namespace Sharply.Web.ViewComponents
{
    public class AppTopbarViewComponent : ViewComponent
    {
        private readonly IUserRepository _userRepository;
        private readonly IStreakService _streakService;

        private const int XpPerLevel = 100;

        public AppTopbarViewComponent(IUserRepository userRepository, IStreakService streakService)
        {
            _userRepository = userRepository;
            _streakService = streakService;
        }

        public async Task<IViewComponentResult> InvokeAsync(bool showSearch = true)
        {
            var claimsUser = (ClaimsPrincipal?)User;
            var userId = int.Parse(claimsUser?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var user = await _userRepository.GetByIdAsync(userId);
            var totalXp = user?.TotalXp ?? 0;

            var model = new AppTopbarViewModel
            {
                ShowSearch = showSearch,
                UserName = claimsUser?.FindFirstValue(ClaimTypes.Name) ?? "Learner",
                UserRole = (user?.Role ?? Domain.Enums.UserRole.Member).ToString(),
                StreakDays = await _streakService.GetCurrentStreakAsync(userId),
                TotalXp = totalXp,
                PlayerLevel = (totalXp / XpPerLevel) + 1
            };

            return View(model);
        }
    }
}
