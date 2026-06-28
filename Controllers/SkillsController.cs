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
        private readonly ISkillDecayService _decayService;

        public SkillsController(ISkillRepository skillRepository, ISkillLogRepository skillLogRepository, ISkillDecayService decayService)
        {
            _skillRepository = skillRepository;
            _skillLogRepository = skillLogRepository;
            _decayService = decayService;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public async Task<IActionResult> Index()
        {
            var skills = await _skillRepository.GetByUserIdAsync(GetUserId());
            var viewModels = new List<SkillViewModel>();

            foreach (var skill in skills)
            {
                var retention = await _decayService.CalculateRetentionAsync(skill);
                var days = await _decayService.GetDaysInactiveAsync(skill);
                viewModels.Add(new SkillViewModel
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

            return View(viewModels);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(string name, string mastery, string priority)
        {
            var skill = new Skill
            {
                Name = name,
                MasteryLevel = Enum.Parse<MasteryLevel>(mastery),
                Priority = Enum.Parse<SkillPriority>(priority),
                InitialRetention = 1.0,
                LastPracticedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UserId = GetUserId()
            };

            await _skillRepository.AddAsync(skill);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var skill = await _skillRepository.GetByIdAsync(id);
            if (skill == null || skill.UserId != GetUserId()) return NotFound();
            return View(skill);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, string name, string mastery, string priority)
        {
            var skill = await _skillRepository.GetByIdAsync(id);
            if (skill == null || skill.UserId != GetUserId()) return NotFound();

            skill.Name = name;
            skill.MasteryLevel = Enum.Parse<MasteryLevel>(mastery);
            skill.Priority = Enum.Parse<SkillPriority>(priority);

            await _skillRepository.UpdateAsync(skill);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var skill = await _skillRepository.GetByIdAsync(id);
            if (skill == null || skill.UserId != GetUserId()) return NotFound();

            await _skillRepository.DeleteAsync(id);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> LogPractice(int id)
        {
            var skill = await _skillRepository.GetByIdAsync(id);
            if (skill == null || skill.UserId != GetUserId()) return NotFound();

            skill.LastPracticedAt = DateTime.UtcNow;
            skill.InitialRetention = 1.0;
            await _skillRepository.UpdateAsync(skill);

            var log = new SkillLog
            {
                SkillId = skill.Id,
                PracticedAt = DateTime.UtcNow,
                Notes = "Practice session logged."
            };
            await _skillLogRepository.AddAsync(log);

            return RedirectToAction("Index", "Home");
        }
    }
}