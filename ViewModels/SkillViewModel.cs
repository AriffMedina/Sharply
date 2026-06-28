namespace Sharply.Web.ViewModels
{
    public class SkillViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string MasteryLevel { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public double RetentionPercent { get; set; }
        public int DaysInactive { get; set; }
        public DateTime LastPracticedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}