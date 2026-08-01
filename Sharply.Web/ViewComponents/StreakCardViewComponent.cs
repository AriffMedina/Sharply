using Microsoft.AspNetCore.Mvc;
using Sharply.Domain.Interfaces;
using Sharply.Web.ViewModels;
using System.Security.Claims;

namespace Sharply.Web.ViewComponents
{
    public class StreakCardViewComponent : ViewComponent
    {
        private readonly IStreakService _streakService;
        private readonly ISkillLogRepository _skillLogRepository;

        public StreakCardViewComponent(IStreakService streakService, ISkillLogRepository skillLogRepository)
        {
            _streakService = streakService;
            _skillLogRepository = skillLogRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = int.Parse(((ClaimsPrincipal?)User)?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var streakDays = await _streakService.GetCurrentStreakAsync(userId);
            var bestStreakDays = await _streakService.GetBestStreakAsync(userId);

            var logs = await _skillLogRepository.GetByUserIdAsync(userId);
            var practiceDates = logs.Select(l => l.PracticedAt.Date).ToHashSet();

            var today = DateTime.UtcNow.Date;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var dayLabels = new[] { "S", "M", "T", "W", "T", "F", "S" };

            var weekActivity = Enumerable.Range(0, 7)
                .Select(i =>
                {
                    var day = startOfWeek.AddDays(i);
                    return new DayActivityViewModel
                    {
                        Label = dayLabels[i],
                        Practiced = practiceDates.Contains(day),
                        IsToday = day == today
                    };
                })
                .ToList();

            var model = new StreakCardViewModel
            {
                StreakDays = streakDays,
                BestStreakDays = bestStreakDays,
                WeekActivity = weekActivity
            };

            return View(model);
        }
    }
}
