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

    public class DayActivityViewModel
    {
        public string Label { get; set; } = string.Empty;
        public bool Practiced { get; set; }
        public bool IsToday { get; set; }
    }

    public class AchievementViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = "star";
        public bool Unlocked { get; set; }
    }

    public class DashboardViewModel
    {
        public string UserName { get; set; } = string.Empty;

        public double AvgRetention { get; set; }

        public string WeeklyActivityPoints { get; set; } = string.Empty;
        public string WeeklyActivityStartLabel { get; set; } = string.Empty;
        public string WeeklyActivityEndLabel { get; set; } = string.Empty;
        public string WeeklyInsightMessage { get; set; } = string.Empty;

        public int NextGoalTarget { get; set; }
        public int NextGoalProgress { get; set; }
        public string NextGoalLabel { get; set; } = string.Empty;

        public List<AchievementViewModel> Achievements { get; set; } = new();

        public List<SkillCardViewModel> Skills { get; set; } = new();

        public string? EmailStatusMessage { get; set; }
        public bool EmailSendSuccess { get; set; }
        public string? LastTestEmail { get; set; }
    }
}
