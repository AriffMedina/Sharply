using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sharply.Application.Services;
using Sharply.Domain.Interfaces;
using Sharply.Models;
using Sharply.Web.ViewModels;
using System.Diagnostics;
using System.Security.Claims;

namespace Sharply.Controllers
{
    public class HomeController : Controller
    {
        private readonly ISkillRepository _skillRepository;
        private readonly ISkillDecayService _decayService;

        public HomeController(ISkillRepository skillRepository, ISkillDecayService decayService)
        {
            _skillRepository = skillRepository;
            _decayService = decayService;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var skills = await _skillRepository.GetByUserIdAsync(GetUserId());
            var skillViewModels = new List<SkillViewModel>();

            foreach (var skill in skills)
            {
                var retention = await _decayService.CalculateRetentionAsync(skill);
                var days = await _decayService.GetDaysInactiveAsync(skill);
                skillViewModels.Add(new SkillViewModel
                {
                    Id = skill.Id,
                    Name = skill.Name,
                    MasteryLevel = skill.MasteryLevel.ToString(),
                    Priority = skill.Priority.ToString(),
                    RetentionPercent = retention * 100,
                    DaysInactive = days,
                    LastPracticedAt = skill.LastPracticedAt,
                    CreatedAt = skill.CreatedAt
                });
            }

            var streak = skillViewModels.Any(s => s.DaysInactive == 0) ? 1 : 0;

            var model = new DashboardViewModel
            {
                UserName = User.FindFirstValue(ClaimTypes.Name) ?? "User",
                UserRole = "Developer",
                Streak = streak,
                Skills = skillViewModels
            };

            return View(model);
        }

        [AllowAnonymous]
        public IActionResult About() => View();

        [AllowAnonymous]
        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}