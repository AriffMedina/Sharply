using System.Collections.Generic;

namespace Sharply.Web.ViewModels
{
    public class StreakCardViewModel
    {
        public int StreakDays { get; set; }
        public int BestStreakDays { get; set; }
        public List<DayActivityViewModel> WeekActivity { get; set; } = new();
    }
}
