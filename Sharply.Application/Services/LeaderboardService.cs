using Sharply.Domain.Interfaces;
using Sharply.Domain.Models;

namespace Sharply.Application.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly IGroupMemberRepository _groupMemberRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMissionCompletionRepository _missionCompletionRepository;
        private readonly IStreakService _streakService;

        public LeaderboardService(
            IGroupMemberRepository groupMemberRepository,
            IUserRepository userRepository,
            IMissionCompletionRepository missionCompletionRepository,
            IStreakService streakService)
        {
            _groupMemberRepository = groupMemberRepository;
            _userRepository = userRepository;
            _missionCompletionRepository = missionCompletionRepository;
            _streakService = streakService;
        }

        public Task<IEnumerable<LeaderboardEntry>> GetWeeklyLeaderboardAsync(int groupId)
        {
            var startOfWeek = StartOfWeek(DateTime.UtcNow.Date);
            return BuildLeaderboardAsync(groupId, completion => completion.CompletedAt.Date >= startOfWeek);
        }

        public Task<IEnumerable<LeaderboardEntry>> GetAllTimeLeaderboardAsync(int groupId) =>
            BuildLeaderboardAsync(groupId, _ => true);

        private async Task<IEnumerable<LeaderboardEntry>> BuildLeaderboardAsync(
            int groupId, Func<MissionCompletion, bool> periodFilter)
        {
            var members = await _groupMemberRepository.GetByGroupIdAsync(groupId);
            var entries = new List<LeaderboardEntry>();

            foreach (var member in members)
            {
                var user = await _userRepository.GetByIdAsync(member.UserId);
                if (user is null) continue;

                var completions = (await _missionCompletionRepository.GetByUserIdAsync(member.UserId))
                    .Where(periodFilter)
                    .ToList();

                entries.Add(new LeaderboardEntry
                {
                    UserId = user.Id,
                    Name = user.Name,
                    Xp = completions.Sum(c => c.XpAwarded),
                    // La racha se muestra pero NO ordena el ranking (a propósito).
                    Streak = await _streakService.GetCurrentStreakAsync(member.UserId),
                    MissionsCompleted = completions.Count
                });
            }

            return entries.OrderByDescending(e => e.Xp).ToList();
        }

        private static DateTime StartOfWeek(DateTime date)
        {
            var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-diff);
        }
    }
}
