using Microsoft.AspNetCore.Mvc;
using Sharply.Api.DTOs;
using Sharply.Domain.Interfaces;

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
    public async Task<ActionResult<IEnumerable<SkillLogResponse>>> GetAll([FromQuery] int? skillId)
    {
        if (skillId.HasValue)
        {
            var filtered = await _skillLogRepository.GetBySkillIdAsync(skillId.Value);
            return Ok(filtered.Select(SkillLogResponse.From));
        }

        var logs = await _skillLogRepository.GetAllAsync();
        return Ok(logs.Select(SkillLogResponse.From));
    }

    // GET /api/skilllogs/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<SkillLogResponse>> GetById(int id)
    {
        var log = await _skillLogRepository.GetByIdAsync(id);

        if (log is null)
        {
            return NotFound();
        }

        return Ok(SkillLogResponse.From(log));
    }
}