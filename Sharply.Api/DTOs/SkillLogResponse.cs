using Sharply.Domain.Models;

namespace Sharply.Api.DTOs;

public class SkillLogResponse
{
    public int Id { get; set; }
    public DateTime PracticedAt { get; set; }
    public string? Notes { get; set; }
    public int SkillId { get; set; }

    // Sin .Include() en el repositorio, EF Core nunca puebla esta navegación:
    // hoy siempre sale null en el JSON real. Se preserva como campo del
    // contrato (tal como lo decidió el dueño del repo) sin acoplar el DTO
    // al tipo de entidad real.
    public object? Skill { get; set; }

    public static SkillLogResponse From(SkillLog l) => new()
    {
        Id = l.Id,
        PracticedAt = l.PracticedAt,
        Notes = l.Notes,
        SkillId = l.SkillId,
        Skill = null,
    };
}
