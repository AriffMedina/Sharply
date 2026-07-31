using Sharply.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sharply.Domain.Models
{
    public class Skill
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Level Level { get; set; } = Level.Intermediate;
        public SkillPriority Priority { get; set; } = SkillPriority.Medium;
        public DateTime LastPracticedAt { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public double InitialRetention { get; set; } = 1.0;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        // Fase 3: si viene de un grupo (instanciada desde un GroupSkill), null = skill personal.
        public int? GroupId { get; set; }

        public ICollection<SkillLog> Logs { get; set; } = new List<SkillLog>();
    }
}
