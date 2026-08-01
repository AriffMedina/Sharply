using Sharply.Application.Services;
using Sharply.Domain.Enums;
using Sharply.Domain.Interfaces;
using Sharply.Domain.Models;

namespace Sharply.Tests
{
    public class MissionServiceTests
    {
        private class FakeMissionRepository : IMissionRepository
        {
            private readonly List<Mission> _missions;
            public FakeMissionRepository(IEnumerable<Mission> missions) => _missions = missions.ToList();
            public Task<IEnumerable<Mission>> GetAllAsync() => Task.FromResult<IEnumerable<Mission>>(_missions);
        }

        private class FakeMissionCompletionRepository : IMissionCompletionRepository
        {
            public List<MissionCompletion> Completions { get; } = new();

            public Task<IEnumerable<MissionCompletion>> GetByUserIdAsync(int userId) =>
                Task.FromResult<IEnumerable<MissionCompletion>>(Completions.Where(c => c.UserId == userId).ToList());

            public Task AddAsync(MissionCompletion completion)
            {
                completion.Id = Completions.Count + 1;
                Completions.Add(completion);
                return Task.CompletedTask;
            }
        }

        private class FakeSkillLogRepository : ISkillLogRepository
        {
            private readonly List<SkillLog> _logs;
            public FakeSkillLogRepository(IEnumerable<SkillLog> logs) => _logs = logs.ToList();

            public Task<IEnumerable<SkillLog>> GetAllAsync() => Task.FromResult<IEnumerable<SkillLog>>(_logs);
            public Task<SkillLog?> GetByIdAsync(int id) => Task.FromResult(_logs.FirstOrDefault(l => l.Id == id));
            public Task<IEnumerable<SkillLog>> GetBySkillIdAsync(int skillId) =>
                Task.FromResult<IEnumerable<SkillLog>>(_logs.Where(l => l.SkillId == skillId).ToList());
            public Task<IEnumerable<SkillLog>> GetByUserIdAsync(int userId) => Task.FromResult<IEnumerable<SkillLog>>(_logs);
            public Task AddAsync(SkillLog log) => Task.CompletedTask;
        }

        private class FakeStreakService : IStreakService
        {
            private readonly int _streak;
            public FakeStreakService(int streak) => _streak = streak;
            public Task<int> GetCurrentStreakAsync(int userId) => Task.FromResult(_streak);
            public Task<int> GetBestStreakAsync(int userId) => Task.FromResult(_streak);
        }

        private class FakeUserRepository : IUserRepository
        {
            public User User { get; }
            public FakeUserRepository(User user) => User = user;

            public Task<User?> GetByIdAsync(int id) => Task.FromResult(id == User.Id ? User : null);
            public Task<User?> GetByEmailAsync(string email) => Task.FromResult<User?>(null);
            public Task AddAsync(User user) => Task.CompletedTask;
            public Task UpdateAsync(User user) => Task.CompletedTask;
            public Task DeleteAsync(int id) => Task.CompletedTask;
            public Task<bool> EmailExistsAsync(string email) => Task.FromResult(false);
        }

        private static Mission DailyPracticeMission => new()
        {
            Id = 1,
            Title = "Practica 3 skills hoy",
            XpReward = 15,
            Type = MissionType.DailyPractice,
            Target = 3,
            Period = MissionPeriod.Daily
        };

        private static Mission KeepStreakMission => new()
        {
            Id = 3,
            Title = "Manten tu racha 7 dias",
            XpReward = 50,
            Type = MissionType.KeepStreak,
            Target = 7,
            Period = MissionPeriod.Weekly
        };

        private static Mission RescueRustyMission => new()
        {
            Id = 2,
            Title = "Rescata una skill en riesgo",
            XpReward = 25,
            Type = MissionType.RescueRusty,
            Target = 1,
            Period = MissionPeriod.Daily
        };

        private static MissionService BuildService(
            IEnumerable<Mission> missions,
            FakeMissionCompletionRepository completions,
            IEnumerable<SkillLog> logs,
            int streak,
            User user) =>
            new(
                new FakeMissionRepository(missions),
                completions,
                new FakeSkillLogRepository(logs),
                new FakeStreakService(streak),
                new FakeUserRepository(user));

        [Fact]
        public async Task EvaluateMissionsAsync_DailyPracticeTargetReached_AwardsXpAndUpdatesUser()
        {
            var today = DateTime.UtcNow;
            var logs = new[]
            {
                new SkillLog { SkillId = 1, PracticedAt = today },
                new SkillLog { SkillId = 2, PracticedAt = today },
                new SkillLog { SkillId = 3, PracticedAt = today },
            };
            var completions = new FakeMissionCompletionRepository();
            var user = new User { Id = 1, TotalXp = 0 };
            var service = BuildService(new[] { DailyPracticeMission }, completions, logs, streak: 0, user);

            await service.EvaluateMissionsAsync(userId: 1, skillWasAtRisk: false);

            Assert.Single(completions.Completions);
            Assert.Equal(15, completions.Completions[0].XpAwarded);
            Assert.Equal(15, user.TotalXp);
        }

        [Fact]
        public async Task EvaluateMissionsAsync_AlreadyCompletedThisPeriod_DoesNotAwardXpTwice()
        {
            var today = DateTime.UtcNow;
            var logs = new[]
            {
                new SkillLog { SkillId = 1, PracticedAt = today },
                new SkillLog { SkillId = 2, PracticedAt = today },
                new SkillLog { SkillId = 3, PracticedAt = today },
            };
            var completions = new FakeMissionCompletionRepository();
            completions.Completions.Add(new MissionCompletion
            {
                Id = 99,
                UserId = 1,
                MissionId = 1,
                CompletedAt = today,
                XpAwarded = 15
            });
            var user = new User { Id = 1, TotalXp = 15 };
            var service = BuildService(new[] { DailyPracticeMission }, completions, logs, streak: 0, user);

            await service.EvaluateMissionsAsync(userId: 1, skillWasAtRisk: false);

            Assert.Single(completions.Completions);
            Assert.Equal(15, user.TotalXp);
        }

        [Fact]
        public async Task EvaluateMissionsAsync_StreakMeetsTarget_CompletesKeepStreakMission()
        {
            var completions = new FakeMissionCompletionRepository();
            var user = new User { Id = 1, TotalXp = 0 };
            var service = BuildService(new[] { KeepStreakMission }, completions, Array.Empty<SkillLog>(), streak: 7, user);

            await service.EvaluateMissionsAsync(userId: 1, skillWasAtRisk: false);

            Assert.Single(completions.Completions);
            Assert.Equal(MissionType.KeepStreak, KeepStreakMission.Type);
            Assert.Equal(50, user.TotalXp);
        }

        [Fact]
        public async Task EvaluateMissionsAsync_StreakBelowTarget_DoesNotCompleteKeepStreakMission()
        {
            var completions = new FakeMissionCompletionRepository();
            var user = new User { Id = 1, TotalXp = 0 };
            var service = BuildService(new[] { KeepStreakMission }, completions, Array.Empty<SkillLog>(), streak: 6, user);

            await service.EvaluateMissionsAsync(userId: 1, skillWasAtRisk: false);

            Assert.Empty(completions.Completions);
            Assert.Equal(0, user.TotalXp);
        }

        [Fact]
        public async Task EvaluateMissionsAsync_SkillWasAtRisk_CompletesRescueRustyMission()
        {
            var completions = new FakeMissionCompletionRepository();
            var user = new User { Id = 1, TotalXp = 0 };
            var service = BuildService(new[] { RescueRustyMission }, completions, Array.Empty<SkillLog>(), streak: 0, user);

            await service.EvaluateMissionsAsync(userId: 1, skillWasAtRisk: true);

            Assert.Single(completions.Completions);
            Assert.Equal(25, user.TotalXp);
        }
    }
}
