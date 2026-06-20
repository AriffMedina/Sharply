using Microsoft.AspNetCore.Mvc;
using Sharply.Domain.Interfaces;
using Sharply.Domain.Models;

namespace Sharply.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SkillLogsController : ControllerBase
{
    private readonly ISkillLogRepository _skillLogRepository;

    public SkillLogsController(ISkillLogRepository skillLogRepository)
    {
        _skillLogRepository = skillLogRepository;
    }

    // GET /api/skilllogs
    // GET /api/skilllogs?skillId=3
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SkillLog>>> GetAll([FromQuery] int? skillId)
    {
        if (skillId.HasValue)
        {
            var filtered = await _skillLogRepository.GetBySkillIdAsync(skillId.Value);
            return Ok(filtered);
        }

        var logs = await _skillLogRepository.GetAllAsync();
        return Ok(logs);
    }

    // GET /api/skilllogs/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<SkillLog>> GetById(int id)
    {
        var log = await _skillLogRepository.GetByIdAsync(id);

        if (log is null)
        {
            return NotFound();
        }

        return Ok(log);
    }
}