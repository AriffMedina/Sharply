namespace Sharply.Web.ViewModels
{
    public class AppSidebarViewModel
    {
        public string Active { get; set; } = string.Empty;
        public int PlayerLevel { get; set; } = 1;
        public int XpIntoLevel { get; set; }
        public int XpToNextLevel { get; set; } = 100;
    }
}
