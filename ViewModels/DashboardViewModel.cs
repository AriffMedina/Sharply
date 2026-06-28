namespace Sharply.Web.ViewModels
{
    public class DashboardViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public string UserRole { get; set; } = "Developer";
        public int Streak { get; set; }
        public List<SkillViewModel> Skills { get; set; } = new();
        public string LastTestEmail { get; set; } = string.Empty;
        public string EmailStatusMessage { get; set; } = string.Empty;
        public bool EmailSendSuccess { get; set; }
    }
}