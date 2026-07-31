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
        private readonly IUserRepository _userRepository;

        public HomeController(
            IEmailService emailService,
            ISkillRepository skillRepository,
            ISkillLogRepository skillLogRepository,
            ISkillDecayService skillDecayService,
            IStreakService streakService,
            IUserRepository userRepository)
        {
            _emailService = emailService;
            _skillRepository = skillRepository;
            _skillLogRepository = skillLogRepository;
            _skillDecayService = skillDecayService;
            _streakService = streakService;
            _userRepository = userRepository;
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
        public IActionResult About() => View();

        [AllowAnonymous]
        public IActionResult Privacy() => View();

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        // XP lineal por nivel de jugador: 100 XP = 1 nivel, sin techo.
        private const int XpPerLevel = 100;

        private async Task<DashboardViewModel> BuildDashboardAsync()
        {
            var userId = CurrentUserId;
            var skills = (await _skillRepository.GetByUserIdAsync(userId)).ToList();
            var user = await _userRepository.GetByIdAsync(userId);
            var totalXp = user?.TotalXp ?? 0;

            var cards = new List<SkillCardViewModel>();
            foreach (var skill in skills)
                cards.Add(await MapSkillToCardAsync(skill));

            var (weeklyPoints, weeklyStartLabel, weeklyEndLabel) = await BuildWeeklyActivityAsync(userId);

            return new DashboardViewModel
            {
                UserName = User.FindFirstValue(ClaimTypes.Name) ?? "Learner",
                UserRole = (user?.Role ?? Domain.Enums.UserRole.Member).ToString(),
                StreakDays = await _streakService.GetCurrentStreakAsync(userId),
                TotalXp = totalXp,
                PlayerLevel = (totalXp / XpPerLevel) + 1,
                AvgRetention = cards.Count > 0 ? Math.Round(cards.Average(c => c.RetentionPercent), 1) : 0,
                WeeklyActivityPoints = weeklyPoints,
                WeeklyActivityStartLabel = weeklyStartLabel,
                WeeklyActivityEndLabel = weeklyEndLabel,
                Skills = cards,
                // MostConsistent/TopContributors siguen siendo datos de muestra a propósito:
                // el leaderboard real de grupo llega en la Fase 3 (Squads).
                MostConsistent = SampleMostConsistent(),
                TopContributors = SampleTopContributors()
            };
        }

        private async Task<(string Points, string StartLabel, string EndLabel)> BuildWeeklyActivityAsync(int userId)
        {
            var logs = (await _skillLogRepository.GetByUserIdAsync(userId)).ToList();
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

            return (points, startLabel, endLabel);
        }

        private static List<LeaderboardEntryViewModel> SampleMostConsistent() => new()
        {
            new() { Name = "Alex Rivera", Value = "242d" },
            new() { Name = "Sarah Chen", Value = "189d" },
            new() { Name = "Marcus Volt", Value = "156d" },
        };

        private static List<LeaderboardEntryViewModel> SampleTopContributors() => new()
        {
            new() { Name = "Elena Krups", Value = "12k" },
            new() { Name = "James Wilson", Value = "9.4k" },
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
