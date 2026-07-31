using Sharply.Domain.Enums;
using Sharply.Domain.Models;

namespace Sharply.Api.DTOs;

public class SkillResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Level Level { get; set; }
    public SkillPriority Priority { get; set; }
    public DateTime LastPracticedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public double InitialRetention { get; set; }
    public int UserId { get; set; }

    // Sin .Include() en el repositorio, EF Core nunca puebla estas navegaciones:
    // hoy siempre salen null/[] en el JSON real. Se preservan como campos del
    // contrato (tal como lo decidió el dueño del repo) sin acoplar el DTO a
    // los tipos de entidad reales.
    public object? User { get; set; }
    public IEnumerable<object> Logs { get; set; } = Array.Empty<object>();

    public static SkillResponse From(Skill s) => new()
    {
        Id = s.Id,
        Name = s.Name,
        Level = s.Level,
        Priority = s.Priority,
        LastPracticedAt = s.LastPracticedAt,
        CreatedAt = s.CreatedAt,
        InitialRetention = s.InitialRetention,
        UserId = s.UserId,
        User = null,
        Logs = Array.Empty<object>(),
    };
}
