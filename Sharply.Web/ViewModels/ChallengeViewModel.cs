namespace Sharply.Web.ViewModels
{
    public class ChallengeViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int XpReward { get; set; }
        public int Progress { get; set; }
        public int Target { get; set; }
        public bool IsCompleted { get; set; }
        public string Period { get; set; } = string.Empty;
    }
}
