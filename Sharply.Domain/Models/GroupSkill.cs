using Sharply.Domain.Enums;

namespace Sharply.Domain.Models
{
    /// <summary>Plantilla de skill de un grupo: el molde desde el que se instancia la Skill de cada miembro.</summary>
    public class GroupSkill
    {
        public int Id { get; set; }

        public int GroupId { get; set; }
        public Group Group { get; set; } = null!;

        public string Name { get; set; } = string.Empty;
        public Level Level { get; set; } = Level.Intermediate;
        public SkillPriority Priority { get; set; } = SkillPriority.Medium;
    }
}
