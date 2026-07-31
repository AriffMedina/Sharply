using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sharply.Domain.Interfaces;
using Sharply.Domain.Models;
using Sharply.Web.Models;
using Sharply.Web.ViewModels;
using System.Diagnostics;
using System.Globalization;
using System.Security.Claims;

namespace Sharply.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IEmailService _emailService;
        private readonly ISkillRepository _skillRepository;
        private readonly ISkillLogRepository _skillLogRepository;
        private readonly ISkillDecayService _skillDecayService;
        private readonly IStreakService _streakService;
        private readonly IGroupService _groupService;

        public HomeController(
            IEmailService emailService,
            ISkillRepository skillRepository,
            ISkillLogRepository skillLogRepository,
            ISkillDecayService skillDecayService,
            IStreakService streakService,
            IGroupService groupService)
        {
            _emailService = emailService;
            _skillRepository = skillRepository;
            _skillLogRepository = skillLogRepository;
            _skillDecayService = skillDecayService;
            _streakService = streakService;
            _groupService = groupService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                ViewData["Title"] = "Sharply";
                return View("Landing");
            }

            ViewData["Title"] = "Dashboard";
            var model = await BuildDashboardAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendTestEmail(string testEmail)
        {
            var model = await BuildDashboardAsync();
            model.LastTestEmail = testEmail;

            if (string.IsNullOrWhiteSpace(testEmail))
            {
                model.EmailSendSuccess = false;
                model.EmailStatusMessage = "Please enter a valid email address before sending.";
                ViewData["Title"] = "Dashboard";
                return View("Index", model);
            }

            try
            {
                await _emailService.SendDecayAlarmAsync(testEmail, "React Fundamentals", 12);
                model.EmailSendSuccess = true;
                model.EmailStatusMessage = $"Email sent to {testEmail}. Check your inbox (or spam).";
            }
            catch (Exception ex)
            {
                model.EmailSendSuccess = false;
                model.EmailStatusMessage = $"Could not send email: {ex.Message}";
            }

            ViewData["Title"] = "Dashboard";
            return View("Index", model);
        }

        public async Task<IActionResult> History()
        {
            var entries = (await _skillLogRepository.GetByUserIdAsync(CurrentUserId))
                .OrderByDescending(l => l.PracticedAt)
                .Select(l => new HistoryEntryViewModel
                {
                    SkillName = l.Skill.Name,
                    PracticedAt = l.PracticedAt,
                    Notes = l.Notes
                })
                .ToList();

            ViewData["Title"] = "History";
            return View(entries);
        }

        [AllowAnonymous]
        public IActionResult Privacy() => View();

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        // Hitos de racha para la card "Next Goal": el próximo no alcanzado marca el objetivo.
        private static readonly int[] StreakMilestones = { 3, 7, 14, 30, 60, 100 };

        private async Task<DashboardViewModel> BuildDashboardAsync()
        {
            var userId = CurrentUserId;
            var allSkills = (await _skillRepository.GetByUserIdAsync(userId)).ToList();
            var skills = allSkills.Where(s => s.GroupId is null).ToList();
            var logs = (await _skillLogRepository.GetByUserIdAsync(userId)).ToList();

            var cards = new List<SkillCardViewModel>();
            foreach (var skill in skills)
                cards.Add(await MapSkillToCardAsync(skill));

            var group = await _groupService.GetGroupForUserAsync(userId);
            var groupCards = new List<SkillCardViewModel>();
            if (group is not null)
            {
                var groupSkills = allSkills.Where(s => s.GroupId == group.Id);
                foreach (var skill in groupSkills)
                    groupCards.Add(await MapSkillToCardAsync(skill));
            }

            var streakDays = await _streakService.GetCurrentStreakAsync(userId);
            var bestStreakDays = await _streakService.GetBestStreakAsync(userId);
            var weekly = BuildWeeklyActivity(logs);
            var nextGoalTarget = StreakMilestones.FirstOrDefault(m => m > streakDays);
            if (nextGoalTarget == 0)
                nextGoalTarget = streakDays + 50 - (streakDays % 50);

            return new DashboardViewModel
            {
                UserName = User.FindFirstValue(ClaimTypes.Name) ?? "Learner",
                AvgRetention = cards.Count > 0 ? Math.Round(cards.Average(c => c.RetentionPercent), 1) : 0,
                WeeklyActivityPoints = weekly.Points,
                WeeklyActivityStartLabel = weekly.StartLabel,
                WeeklyActivityEndLabel = weekly.EndLabel,
                WeeklyInsightMessage = weekly.InsightMessage,
                NextGoalTarget = nextGoalTarget,
                NextGoalProgress = Math.Min(streakDays, nextGoalTarget),
                NextGoalLabel = $"Practice {nextGoalTarget} days in a row",
                Achievements = BuildAchievements(logs.Count > 0, bestStreakDays),
                Skills = cards,
                GroupName = group?.Name,
                GroupSkills = groupCards
            };
        }

        private static (string Points, string StartLabel, string EndLabel, string InsightMessage) BuildWeeklyActivity(List<SkillLog> logs)
        {
            var today = DateTime.UtcNow.Date;

            var dailyCounts = Enumerable.Range(0, 7)
                .Select(offset => today.AddDays(offset - 6))
                .Select(day => logs.Count(l => l.PracticedAt.Date == day))
                .ToList();

            var maxCount = Math.Max(dailyCounts.Max(), 1);
            const double width = 220, topMargin = 6, baseline = 60;

            var points = string.Join(" ", dailyCounts.Select((count, index) =>
            {
                var x = index * (width / (dailyCounts.Count - 1));
                var y = baseline - (count / (double)maxCount) * (baseline - topMargin);
                return $"{x.ToString("F1", CultureInfo.InvariantCulture)},{y.ToString("F1", CultureInfo.InvariantCulture)}";
            }));

            var startLabel = today.AddDays(-6).ToString("ddd", CultureInfo.InvariantCulture).ToUpperInvariant();
            var endLabel = today.ToString("ddd", CultureInfo.InvariantCulture).ToUpperInvariant();

            var thisWeekTotal = dailyCounts.Sum();
            var lastWeekTotal = Enumerable.Range(0, 7)
                .Select(offset => today.AddDays(offset - 13))
                .Sum(day => logs.Count(l => l.PracticedAt.Date == day));

            var insightMessage = thisWeekTotal == 0
                ? "No activity yet this week. Log a session to get going."
                : thisWeekTotal > lastWeekTotal
                    ? "Great job! You practiced more this week."
                    : thisWeekTotal == lastWeekTotal
                        ? "Steady pace — same activity as last week."
                        : "A bit quieter than last week. Let's pick it back up.";

            return (points, startLabel, endLabel, insightMessage);
        }

        private static List<AchievementViewModel> BuildAchievements(bool hasPracticed, int bestStreakDays) => new()
        {
            new() { Title = "Getting Started", Description = "Log your first practice session", Icon = "star", Unlocked = hasPracticed },
            new() { Title = "Consistent", Description = "Practice 3 days in a row", Icon = "shield", Unlocked = bestStreakDays >= 3 },
            new() { Title = "Dedicated", Description = "Reach a 7 day streak", Icon = "heart", Unlocked = bestStreakDays >= 7 },
        };

        private async Task<SkillCardViewModel> MapSkillToCardAsync(Skill skill)
        {
            var daysAgo = await _skillDecayService.GetDaysInactiveAsync(skill);
            var retention = await _skillDecayService.CalculateRetentionAsync(skill);
            var retentionPercent = (int)Math.Round(retention * 100);

            var logs = await _skillLogRepository.GetBySkillIdAsync(skill.Id);
            var latestNote = logs
                .OrderByDescending(l => l.PracticedAt)
                .FirstOrDefault()?.Notes;

            return new SkillCardViewModel
            {
                Id = skill.Id,
                Name = skill.Name,
                Priority = skill.Priority.ToString(),
                Level = skill.Level.ToString(),
                RetentionPercent = retentionPercent,
                DaysAgo = daysAgo,
                Note = string.IsNullOrWhiteSpace(latestNote)
                    ? "No practice notes yet. Log a session to track your progress."
                    : latestNote!
            };
        }

    }
}
