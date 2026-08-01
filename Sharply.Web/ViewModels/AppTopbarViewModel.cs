namespace Sharply.Web.ViewModels
{
    public class AppTopbarViewModel
    {
        public bool ShowSearch { get; set; } = true;
        public string UserName { get; set; } = string.Empty;
        public string UserRole { get; set; } = "Member";
        public int StreakDays { get; set; }
        public int TotalXp { get; set; }
        public int PlayerLevel { get; set; } = 1;
    }
}
