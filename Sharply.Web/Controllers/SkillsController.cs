using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sharply.Application.Services;
using Sharply.Domain.Enums;
using Sharply.Domain.Interfaces;
using Sharply.Domain.Models;
using Sharply.Web.ViewModels;
using System.Security.Claims;

namespace Sharply.Controllers
{
    [Authorize]
    public class SkillsController : Controller
    {
        private readonly ISkillRepository _skillRepository;
        private readonly ISkillLogRepository _skillLogRepository;
        private readonly ISkillDecayService _skillDecayService;

        public SkillsController(
            ISkillRepository skillRepository,
            ISkillLogRepository skillLogRepository,
            ISkillDecayService skillDecayService)
        {
            _skillRepository = skillRepository;
            _skillLogRepository = skillLogRepository;
            _skillDecayService = skillDecayService;
        }

        // ── LIST ──────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var skills = await _skillRepository.GetByUserIdAsync(CurrentUserId);
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
                MasteryLevel = MapMastery(model.InitialMasteryPercent),
                InitialRetention = model.InitialMasteryPercent / 100.0,
                LastPracticedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UserId = CurrentUserId
            };
            await _skillRepository.AddAsync(skill);

            if (!string.IsNullOrWhiteSpace(model.Description))
                await _skillLogRepository.AddAsync(new SkillLog
                {
                    SkillId = skill.Id,
                    Notes = model.Description!.Trim(),
                    PracticedAt = DateTime.UtcNow
                });

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
                InitialMasteryPercent = (int)Math.Round(skill.InitialRetention * 100)
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
            skill.MasteryLevel = MapMastery(model.InitialMasteryPercent);
            skill.InitialRetention = model.InitialMasteryPercent / 100.0;
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

            skill.LastPracticedAt = DateTime.UtcNow;
            skill.InitialRetention = 1.0;

            if (skill.MasteryLevel < MasteryLevel.Sharp)
                skill.MasteryLevel++;

            await _skillRepository.UpdateAsync(skill);

            await _skillLogRepository.AddAsync(new SkillLog
            {
                SkillId = skill.Id,
                Notes = "Practice session logged.",
                PracticedAt = DateTime.UtcNow
            });

            return RedirectToAction("Index", "Home");
        }

        // ── HELPERS ───────────────────────────────────────────
        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        private static MasteryLevel MapMastery(int percent) => percent switch
        {
            < 40 => MasteryLevel.Rusty,
            < 75 => MasteryLevel.Intermediate,
            _ => MasteryLevel.Sharp
        };

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
                MasteryLevel = skill.MasteryLevel.ToString(),
                RetentionPercent = (int)Math.Round(retention * 100),
                DaysAgo = daysAgo,
                Note = string.IsNullOrWhiteSpace(latestNote)
                    ? "No practice notes yet."
                    : latestNote!
            };
        }
    }
}
