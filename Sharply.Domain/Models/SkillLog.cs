using System;
using System.Collections.Generic;
using System.Text;

namespace Sharply.Domain.Models
{
    public class SkillLog
    {
        public int Id { get; set; }
        public DateTime PracticedAt { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }

        public int SkillId { get; set; }
        public Skill Skill { get; set; } = null!;
    }
}
