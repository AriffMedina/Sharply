namespace Sharply.Web.ViewModels
{
    public class HistoryEntryViewModel
    {
        public string SkillName { get; set; } = string.Empty;
        public DateTime PracticedAt { get; set; }
        public string? Notes { get; set; }
    }
}
