using Sharply.Application.Services;
using Sharply.Domain.Interfaces;
using Sharply.Domain.Models;

namespace Sharply.Tests
{
    public class StreakServiceTests
    {
        private class FakeSkillLogRepository : ISkillLogRepository
        {
            private readonly List<SkillLog> _logs;

            public FakeSkillLogRepository(IEnumerable<SkillLog> logs) => _logs = logs.ToList();

            public Task<IEnumerable<SkillLog>> GetAllAsync() => Task.FromResult<IEnumerable<SkillLog>>(_logs);

            public Task<SkillLog?> GetByIdAsync(int id) => Task.FromResult(_logs.FirstOrDefault(l => l.Id == id));

            public Task<IEnumerable<SkillLog>> GetBySkillIdAsync(int skillId) =>
                Task.FromResult<IEnumerable<SkillLog>>(_logs.Where(l => l.SkillId == skillId).ToList());

            public Task<IEnumerable<SkillLog>> GetByUserIdAsync(int userId) =>
                Task.FromResult<IEnumerable<SkillLog>>(_logs);

            public Task AddAsync(SkillLog log) => Task.CompletedTask;
        }

        private static SkillLog LogOn(DateTime date) => new() { PracticedAt = date };

        [Fact]
        public async Task GetCurrentStreakAsync_WithConsecutiveDaysIncludingToday_ReturnsFullStreak()
        {
            var today = DateTime.UtcNow.Date;
            var logs = new[]
            {
                LogOn(today),
                LogOn(today.AddDays(-1)),
                LogOn(today.AddDays(-2)),
            };
            var service = new StreakService(new FakeSkillLogRepository(logs));

            var streak = await service.GetCurrentStreakAsync(userId: 1);

            Assert.Equal(3, streak);
        }

        [Fact]
        public async Task GetCurrentStreakAsync_WithGapBeforeToday_StopsAtTheGap()
        {
            var today = DateTime.UtcNow.Date;
            var logs = new[]
            {
                LogOn(today),
                LogOn(today.AddDays(-1)),
                // hueco en today-2: la racha no debe seguir de largo hasta los logs mas viejos
                LogOn(today.AddDays(-3)),
                LogOn(today.AddDays(-4)),
            };
            var service = new StreakService(new FakeSkillLogRepository(logs));

            var streak = await service.GetCurrentStreakAsync(userId: 1);

            Assert.Equal(2, streak);
        }

        [Fact]
        public async Task GetCurrentStreakAsync_WithoutPracticeToday_StillCountsStreakThroughYesterday()
        {
            var today = DateTime.UtcNow.Date;
            var logs = new[]
            {
                LogOn(today.AddDays(-1)),
                LogOn(today.AddDays(-2)),
            };
            var service = new StreakService(new FakeSkillLogRepository(logs));

            var streak = await service.GetCurrentStreakAsync(userId: 1);

            Assert.Equal(2, streak);
        }

        [Fact]
        public async Task GetCurrentStreakAsync_WithNoLogs_ReturnsZero()
        {
            var service = new StreakService(new FakeSkillLogRepository(Array.Empty<SkillLog>()));

            var streak = await service.GetCurrentStreakAsync(userId: 1);

            Assert.Equal(0, streak);
        }
    }
}
