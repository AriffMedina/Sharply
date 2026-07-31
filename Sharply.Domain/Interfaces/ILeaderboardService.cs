using Sharply.Domain.Models;

namespace Sharply.Domain.Interfaces
{
    public interface ILeaderboardService
    {
        Task<IEnumerable<LeaderboardEntry>> GetWeeklyLeaderboardAsync(int groupId);
        Task<IEnumerable<LeaderboardEntry>> GetAllTimeLeaderboardAsync(int groupId);
    }
}
