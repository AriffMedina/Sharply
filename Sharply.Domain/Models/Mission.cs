using Sharply.Domain.Enums;

namespace Sharply.Domain.Models
{
    public class Mission
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int XpReward { get; set; }
        public MissionType Type { get; set; }
        public int Target { get; set; }
        public MissionPeriod Period { get; set; }

        // Fase 3: misión propia de un grupo/squad. Null = misión global (todos la ven).
        public int? GroupId { get; set; }
    }
}
