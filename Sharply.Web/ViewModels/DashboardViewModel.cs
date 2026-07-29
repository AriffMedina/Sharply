using System.Collections.Generic;

namespace Sharply.Web.ViewModels
{
    public class SkillCardViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Priority { get; set; } = "Medium";
        public string MasteryLevel { get; set; } = "Intermediate";
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
        public string UserName { get; set; } = "Jordan Hayes";
        public string UserRole { get; set; } = "Pro Learner";
        public int StreakDays { get; set; } = 14;

        public double AvgRetention { get; set; } = 78.4;
        public double RetentionDeltaVsLastWeek { get; set; } = 2.4;

        public List<SkillCardViewModel> Skills { get; set; } = new();
        public List<LeaderboardEntryViewModel> MostConsistent { get; set; } = new();
        public List<LeaderboardEntryViewModel> TopContributors { get; set; } = new();

        public string? EmailStatusMessage { get; set; }
        public bool EmailSendSuccess { get; set; }
        public string? LastTestEmail { get; set; }
    }
}
