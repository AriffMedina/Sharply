using Sharply.Domain.Interfaces;

namespace Sharply.Application.Services
{
    public class StreakService : IStreakService
    {
        private readonly ISkillLogRepository _skillLogRepository;

        public StreakService(ISkillLogRepository skillLogRepository)
        {
            _skillLogRepository = skillLogRepository;
        }

        public async Task<int> GetCurrentStreakAsync(int userId)
        {
            var logs = await _skillLogRepository.GetByUserIdAsync(userId);

            var practiceDates = logs
                .Select(l => l.PracticedAt.Date)
                .Distinct()
                .ToHashSet();

            if (practiceDates.Count == 0)
                return 0;

            var cursor = DateTime.UtcNow.Date;

            // Si todavía no practicaste hoy, la racha sigue viva mientras ayer tenga registro.
            if (!practiceDates.Contains(cursor))
                cursor = cursor.AddDays(-1);

            var streak = 0;
            while (practiceDates.Contains(cursor))
            {
                streak++;
                cursor = cursor.AddDays(-1);
            }

            return streak;
        }
    }
}
