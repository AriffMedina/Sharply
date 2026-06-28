using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sharply.Application.Services;
using Sharply.Domain.Interfaces;
using Sharply.Domain.Models;
using Sharply.Models;
using Sharply.Web.ViewModels;
using System.Diagnostics;
using System.Security.Claims;

namespace Sharply.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IEmailService _emailService;
        private readonly ISkillRepository _skillRepository;
        private readonly ISkillLogRepository _skillLogRepository;
        private readonly ISkillDecayService _skillDecayService;

        public HomeController(
            IEmailService emailService,
            ISkillRepository skillRepository,
            ISkillLogRepository skillLogRepository,
            ISkillDecayService skillDecayService)
        {
            _emailService = emailService;
            _skillRepository = skillRepository;
            _skillLogRepository = skillLogRepository;
            _skillDecayService = skillDecayService;
        }

        public async Task<IActionResult> Index()
        {
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

        private async Task<DashboardViewModel> BuildDashboardAsync()
        {
            var model = BuildSampleDashboard();
            model.UserName = User.FindFirstValue(ClaimTypes.Name) ?? model.UserName;

            var skills = (await _skillRepository.GetByUserIdAsync(CurrentUserId)).ToList();

            if (skills.Count > 0)
            {
                var cards = new List<SkillCardViewModel>();
                foreach (var skill in skills)
                    cards.Add(await MapSkillToCardAsync(skill));

                model.Skills = cards;
                model.AvgRetention = Math.Round(cards.Average(c => c.RetentionPercent), 1);
                model.RetentionDeltaVsLastWeek = 0;
            }

            return model;
        }

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
                MasteryLevel = skill.MasteryLevel.ToString(),
                RetentionPercent = retentionPercent,
                DaysAgo = daysAgo,
                Note = string.IsNullOrWhiteSpace(latestNote)
                    ? "No practice notes yet. Log a session to track your progress."
                    : latestNote!
            };
        }

        private static DashboardViewModel BuildSampleDashboard()
        {
            return new DashboardViewModel
            {
                UserName = "Jordan Hayes",
                UserRole = "Pro Learner",
                StreakDays = 14,
                AvgRetention = 78.4,
                RetentionDeltaVsLastWeek = 2.4,
                Skills = new List<SkillCardViewModel>
                {
                    new() { Id = 0, Name = "React Fundamentals", Priority = "High", MasteryLevel = "Sharp", RetentionPercent = 94, DaysAgo = 1,
                        Note = "Strong grasp on hooks and reconciliation. Need to review Server Components." },
                    new() { Id = 0, Name = "UI/UX Micro-Interactions", Priority = "Medium", MasteryLevel = "Intermediate", RetentionPercent = 72, DaysAgo = 4,
                        Note = "Developing intuition for easing curves and visual feedback loops." },
                    new() { Id = 0, Name = "System Design", Priority = "High", MasteryLevel = "Rusty", RetentionPercent = 45, DaysAgo = 12,
                        Note = "Knowledge of load balancing and sharding is fading. Schedule a deep dive." },
                    new() { Id = 0, Name = "TypeScript Advanced", Priority = "Low", MasteryLevel = "Intermediate", RetentionPercent = 88, DaysAgo = 2,
                        Note = "Excellent at generics and utility types. Solid production performance." },
                    new() { Id = 0, Name = "Database Optimization", Priority = "Medium", MasteryLevel = "Intermediate", RetentionPercent = 61, DaysAgo = 5,
                        Note = "Indexing strategies are clear; query profiling needs more hands-on work." },
                    new() { Id = 0, Name = "Motion Design", Priority = "Low", MasteryLevel = "Rusty", RetentionPercent = 32, DaysAgo = 21,
                        Note = "Basic keyframing understood. Advanced physics-based motion is currently rusty." },
                },
                MostConsistent = new List<LeaderboardEntryViewModel>
                {
                    new() { Name = "Alex Rivera", Value = "242d" },
                    new() { Name = "Sarah Chen", Value = "189d" },
                    new() { Name = "Marcus Volt", Value = "156d" },
                },
                TopContributors = new List<LeaderboardEntryViewModel>
                {
                    new() { Name = "Elena Krups", Value = "12k" },
                    new() { Name = "James Wilson", Value = "9.4k" },
                }
            };
        }
    }
}
