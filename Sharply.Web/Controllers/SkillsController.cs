using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sharply.Domain.Enums;
using Sharply.Domain.Interfaces;
using Sharply.Domain.Models;
using Sharply.Web.ViewModels;
using System.Security.Claims;

namespace Sharply.Web.Controllers
{
    [Authorize]
    public class SkillsController : Controller
    {
        private readonly ISkillRepository _skillRepository;
        private readonly ISkillLogRepository _skillLogRepository;
        private readonly ISkillDecayService _skillDecayService;
        private readonly IMissionService _missionService;

        public SkillsController(
            ISkillRepository skillRepository,
            ISkillLogRepository skillLogRepository,
            ISkillDecayService skillDecayService,
            IMissionService missionService)
        {
            _skillRepository = skillRepository;
            _skillLogRepository = skillLogRepository;
            _skillDecayService = skillDecayService;
            _missionService = missionService;
        }

        // ── LIST ──────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var skills = (await _skillRepository.GetByUserIdAsync(CurrentUserId))
                .Where(s => s.GroupId is null);
            var cards = new List<SkillCardViewModel>();
            foreach (var skill in skills)
                cards.Add(await MapSkillToCardAsync(skill));
            return View(cards);
        }

        // ── CREATE ────────────────────────────────────────────
        [HttpGet]
        public IActionResult Create() => View(new SkillFormViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SkillFormViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var skill = new Skill
            {
                Name = model.Name.Trim(),
                Priority = Enum.TryParse<SkillPriority>(model.Priority, out var p) ? p : SkillPriority.Medium,
                Level = model.Level,
                InitialRetention = 1.0,
                LastPracticedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UserId = CurrentUserId
            };
            await _skillRepository.AddAsync(skill);

            if (!string.IsNullOrWhiteSpace(model.Description))
            {
                await _skillLogRepository.AddAsync(new SkillLog
                {
                    SkillId = skill.Id,
                    Notes = model.Description!.Trim(),
                    PracticedAt = DateTime.UtcNow
                });

                await _missionService.EvaluateMissionsAsync(CurrentUserId, skillWasAtRisk: false);
            }

            TempData["SkillAdded"] = skill.Name;
            return RedirectToAction("Index", "Home");
        }

        // ── EDIT ──────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var skill = await _skillRepository.GetByIdAsync(id);
            if (skill is null || skill.UserId != CurrentUserId) return NotFound();

            var vm = new SkillFormViewModel
            {
                Name = skill.Name,
                Priority = skill.Priority.ToString(),
                Level = skill.Level
            };
            ViewBag.SkillId = id;
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SkillFormViewModel model)
        {
            if (!ModelState.IsValid) { ViewBag.SkillId = id; return View(model); }

            var skill = await _skillRepository.GetByIdAsync(id);
            if (skill is null || skill.UserId != CurrentUserId) return NotFound();

            skill.Name = model.Name.Trim();
            skill.Priority = Enum.TryParse<SkillPriority>(model.Priority, out var p) ? p : SkillPriority.Medium;
            skill.Level = model.Level;
            await _skillRepository.UpdateAsync(skill);

            TempData["SkillUpdated"] = skill.Name;
            return RedirectToAction("Index");
        }

        // ── DELETE ────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var skill = await _skillRepository.GetByIdAsync(id);
            if (skill is null || skill.UserId != CurrentUserId) return NotFound();

            await _skillRepository.DeleteAsync(id);
            TempData["SkillDeleted"] = skill.Name;
            return RedirectToAction("Index");
        }

        // ── LOG PRACTICE ──────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> LogPractice(int id)
        {
            var skill = await _skillRepository.GetByIdAsync(id);
            if (skill is null || skill.UserId != CurrentUserId) return NotFound();

            var retentionBeforePractice = await _skillDecayService.CalculateRetentionAsync(skill);
            var wasAtRisk = retentionBeforePractice < skill.User.DecayRetentionThreshold;

            skill.LastPracticedAt = DateTime.UtcNow;
            skill.InitialRetention = 1.0;
            skill.CurrentSuggestion = null;
            skill.SuggestionGeneratedAt = null;

            if (skill.Level < Level.Advanced)
                skill.Level++;

            await _skillRepository.UpdateAsync(skill);

            await _skillLogRepository.AddAsync(new SkillLog
            {
                SkillId = skill.Id,
                Notes = "Practice session logged.",
                PracticedAt = DateTime.UtcNow
            });

            await _missionService.EvaluateMissionsAsync(CurrentUserId, wasAtRisk);

            return RedirectToAction("Index", "Home");
        }

        // ── HELPERS ───────────────────────────────────────────
        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        private async Task<SkillCardViewModel> MapSkillToCardAsync(Skill skill)
        {
            var daysAgo = await _skillDecayService.GetDaysInactiveAsync(skill);
            var retention = await _skillDecayService.CalculateRetentionAsync(skill);
            var logs = await _skillLogRepository.GetBySkillIdAsync(skill.Id);
            var latestNote = logs.OrderByDescending(l => l.PracticedAt).FirstOrDefault()?.Notes;

            return new SkillCardViewModel
            {
                Id = skill.Id,
                Name = skill.Name,
                Priority = skill.Priority.ToString(),
                Level = skill.Level.ToString(),
                RetentionPercent = (int)Math.Round(retention * 100),
                DaysAgo = daysAgo,
                Note = string.IsNullOrWhiteSpace(latestNote)
                    ? "No practice notes yet."
                    : latestNote!,
                Suggestion = skill.CurrentSuggestion
            };
        }
    }
}
