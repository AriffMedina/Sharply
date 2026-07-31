using System.Collections.Generic;

namespace Sharply.Web.ViewModels
{
    public class SkillCardViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Priority { get; set; } = "Medium";
        public string Level { get; set; } = "Intermediate";
        public int RetentionPercent { get; set; }
        public int DaysAgo { get; set; }
        public string Note { get; set; } = string.Empty;
    }

    public class LeaderboardEntryViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class DashboardViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public int StreakDays { get; set; }
        public int TotalXp { get; set; }
        public int PlayerLevel { get; set; } = 1;

        public double AvgRetention { get; set; }

        public string WeeklyActivityPoints { get; set; } = string.Empty;
        public string WeeklyActivityStartLabel { get; set; } = string.Empty;
        public string WeeklyActivityEndLabel { get; set; } = string.Empty;

        public List<SkillCardViewModel> Skills { get; set; } = new();
        public List<LeaderboardEntryViewModel> MostConsistent { get; set; } = new();
        public List<LeaderboardEntryViewModel> TopContributors { get; set; } = new();

        public string? EmailStatusMessage { get; set; }
        public bool EmailSendSuccess { get; set; }
        public string? LastTestEmail { get; set; }
    }
}
