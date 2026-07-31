using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sharply.Domain.Enums;
using Sharply.Domain.Interfaces;
using Sharply.Web.ViewModels;
using System.Security.Claims;

namespace Sharply.Web.Controllers
{
    [Authorize]
    public class ChallengesController : Controller
    {
        private readonly IMissionService _missionService;
        private readonly ISkillLogRepository _skillLogRepository;
        private readonly IStreakService _streakService;

        public ChallengesController(
            IMissionService missionService,
            ISkillLogRepository skillLogRepository,
            IStreakService streakService)
        {
            _missionService = missionService;
            _skillLogRepository = skillLogRepository;
            _streakService = streakService;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        public async Task<IActionResult> Index()
        {
            var userId = CurrentUserId;

            var missions = (await _missionService.GetActiveMissionsAsync()).ToList();
            var completedMissionIds = (await _missionService.GetCompletionsForCurrentPeriodAsync(userId))
                .Select(c => c.MissionId)
                .ToHashSet();

            var streak = await _streakService.GetCurrentStreakAsync(userId);

            var logs = (await _skillLogRepository.GetByUserIdAsync(userId)).ToList();
            var today = DateTime.UtcNow.Date;
            var distinctSkillsToday = logs
                .Where(l => l.PracticedAt.Date == today)
                .Select(l => l.SkillId)
                .Distinct()
                .Count();

            var challenges = missions.Select(m =>
            {
                var isCompleted = completedMissionIds.Contains(m.Id);
                var progress = m.Type switch
                {
                    MissionType.DailyPractice => Math.Min(distinctSkillsToday, m.Target),
                    MissionType.KeepStreak => Math.Min(streak, m.Target),
                    MissionType.RescueRusty => isCompleted ? 1 : 0,
                    _ => 0
                };

                return new ChallengeViewModel
                {
                    Title = m.Title,
                    Description = m.Description,
                    XpReward = m.XpReward,
                    Progress = progress,
                    Target = m.Target,
                    IsCompleted = isCompleted,
                    Period = m.Period.ToString()
                };
            }).ToList();

            ViewData["Title"] = "Challenges";
            return View(challenges);
        }
    }
}
