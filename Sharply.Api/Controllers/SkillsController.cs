using Microsoft.AspNetCore.Mvc;
using Sharply.Domain.Enums;
using Sharply.Domain.Interfaces;
using Sharply.Domain.Models;

namespace Sharply.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SkillsController : ControllerBase
{
    private readonly ISkillRepository _skillRepository;

    public SkillsController(ISkillRepository skillRepository)
    {
        _skillRepository = skillRepository;
    }

    // GET /api/skills
    // GET /api/skills?priority=Alta
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Skill>>> GetAll([FromQuery] SkillPriority? priority)
    {
        var skills = await _skillRepository.GetAllAsync();

        if (priority.HasValue)
        {
            skills = skills.Where(s => s.Priority == priority.Value);
        }

        return Ok(skills);
    }

    // GET /api/skills/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Skill>> GetById(int id)
    {
        var skill = await _skillRepository.GetByIdAsync(id);

        if (skill is null)
        {
            return NotFound();
        }

        return Ok(skill);
    }
}