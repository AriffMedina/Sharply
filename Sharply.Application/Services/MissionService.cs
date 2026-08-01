using Sharply.Domain.Enums;
using Sharply.Domain.Interfaces;
using Sharply.Domain.Models;

namespace Sharply.Application.Services
{
    public class MissionService : IMissionService
    {
        private readonly IMissionRepository _missionRepository;
        private readonly IMissionCompletionRepository _completionRepository;
        private readonly ISkillLogRepository _skillLogRepository;
        private readonly IStreakService _streakService;
        private readonly IUserRepository _userRepository;

        public MissionService(
            IMissionRepository missionRepository,
            IMissionCompletionRepository completionRepository,
            ISkillLogRepository skillLogRepository,
            IStreakService streakService,
            IUserRepository userRepository)
        {
            _missionRepository = missionRepository;
            _completionRepository = completionRepository;
            _skillLogRepository = skillLogRepository;
            _streakService = streakService;
            _userRepository = userRepository;
        }

        public Task<IEnumerable<Mission>> GetActiveMissionsAsync() => _missionRepository.GetAllAsync();

        public async Task<IEnumerable<MissionCompletion>> GetCompletionsForCurrentPeriodAsync(int userId)
        {
            var completions = await _completionRepository.GetByUserIdAsync(userId);
            var missionsById = (await _missionRepository.GetAllAsync()).ToDictionary(m => m.Id);

            return completions.Where(c =>
                missionsById.TryGetValue(c.MissionId, out var mission) &&
                IsInCurrentPeriod(c.CompletedAt, mission.Period));
        }

        public async Task EvaluateMissionsAsync(int userId, bool skillWasAtRisk)
        {
            var missions = await _missionRepository.GetAllAsync();
            var existingCompletions = (await _completionRepository.GetByUserIdAsync(userId)).ToList();

            foreach (var mission in missions)
            {
                var alreadyCompletedThisPeriod = existingCompletions.Any(c =>
                    c.MissionId == mission.Id && IsInCurrentPeriod(c.CompletedAt, mission.Period));

                if (alreadyCompletedThisPeriod)
                    continue;

                if (!await IsMissionCompletedAsync(mission, userId, skillWasAtRisk))
                    continue;

                await _completionRepository.AddAsync(new MissionCompletion
                {
                    UserId = userId,
                    MissionId = mission.Id,
                    CompletedAt = DateTime.UtcNow,
                    XpAwarded = mission.XpReward
                });

                var user = await _userRepository.GetByIdAsync(userId);
                if (user is not null)
                {
                    user.TotalXp += mission.XpReward;
                    await _userRepository.UpdateAsync(user);
                }
            }
        }

        private async Task<bool> IsMissionCompletedAsync(Mission mission, int userId, bool skillWasAtRisk) =>
            mission.Type switch
            {
                MissionType.RescueRusty => skillWasAtRisk,
                MissionType.KeepStreak => await _streakService.GetCurrentStreakAsync(userId) >= mission.Target,
                MissionType.DailyPractice => await CountDistinctSkillsPracticedTodayAsync(userId) >= mission.Target,
                _ => false
            };

        private async Task<int> CountDistinctSkillsPracticedTodayAsync(int userId)
        {
            var logs = await _skillLogRepository.GetByUserIdAsync(userId);
            var today = DateTime.UtcNow.Date;

            return logs
                .Where(l => l.PracticedAt.Date == today)
                .Select(l => l.SkillId)
                .Distinct()
                .Count();
        }

        private static bool IsInCurrentPeriod(DateTime completedAt, MissionPeriod period) => period switch
        {
            MissionPeriod.Daily => completedAt.Date == DateTime.UtcNow.Date,
            MissionPeriod.Weekly => completedAt.Date >= StartOfWeek(DateTime.UtcNow.Date),
            _ => false
        };

        private static DateTime StartOfWeek(DateTime date)
        {
            // La semana arranca el lunes (mismo criterio que el leaderboard semanal de la Fase 3).
            var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-diff);
        }
    }
}
