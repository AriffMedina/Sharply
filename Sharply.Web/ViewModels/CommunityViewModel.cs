namespace Sharply.Web.ViewModels
{
    public class LeaderboardRowViewModel
    {
        public string Name { get; set; } = string.Empty;
        public int Xp { get; set; }
        public int Streak { get; set; }
        public int MissionsCompleted { get; set; }
    }

    public class GroupSkillRowViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
    }

    public class CommunityViewModel
    {
        public bool HasGroup { get; set; }
        public bool IsOwner { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string InviteCode { get; set; } = string.Empty;
        public int MemberCount { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<LeaderboardRowViewModel> WeeklyLeaderboard { get; set; } = new();
        public List<LeaderboardRowViewModel> AllTimeLeaderboard { get; set; } = new();
        public List<GroupSkillRowViewModel> GroupSkills { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public string? ErrorSource { get; set; }
    }
}
