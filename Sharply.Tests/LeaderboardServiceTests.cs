using Sharply.Application.Services;
using Sharply.Domain.Interfaces;
using Sharply.Domain.Models;

namespace Sharply.Tests
{
    public class LeaderboardServiceTests
    {
        private class FakeGroupMemberRepository : IGroupMemberRepository
        {
            private readonly List<GroupMember> _members;
            public FakeGroupMemberRepository(IEnumerable<GroupMember> members) => _members = members.ToList();

            public Task<GroupMember?> GetByUserIdAsync(int userId) =>
                Task.FromResult(_members.FirstOrDefault(m => m.UserId == userId));

            public Task<IEnumerable<GroupMember>> GetByGroupIdAsync(int groupId) =>
                Task.FromResult<IEnumerable<GroupMember>>(_members.Where(m => m.GroupId == groupId).ToList());

            public Task AddAsync(GroupMember member) { _members.Add(member); return Task.CompletedTask; }
            public Task DeleteAsync(int id) { _members.RemoveAll(m => m.Id == id); return Task.CompletedTask; }
            public Task DeleteByGroupIdAsync(int groupId) { _members.RemoveAll(m => m.GroupId == groupId); return Task.CompletedTask; }
        }

        private class FakeUserRepository : IUserRepository
        {
            private readonly Dictionary<int, User> _users;
            public FakeUserRepository(IEnumerable<User> users) => _users = users.ToDictionary(u => u.Id);

            public Task<User?> GetByIdAsync(int id) => Task.FromResult(_users.GetValueOrDefault(id));
            public Task<User?> GetByEmailAsync(string email) => Task.FromResult<User?>(null);
            public Task AddAsync(User user) => Task.CompletedTask;
            public Task UpdateAsync(User user) => Task.CompletedTask;
            public Task DeleteAsync(int id) => Task.CompletedTask;
            public Task<bool> EmailExistsAsync(string email) => Task.FromResult(false);
        }

        private class FakeMissionCompletionRepository : IMissionCompletionRepository
        {
            private readonly List<MissionCompletion> _completions;
            public FakeMissionCompletionRepository(IEnumerable<MissionCompletion> completions) => _completions = completions.ToList();

            public Task<IEnumerable<MissionCompletion>> GetByUserIdAsync(int userId) =>
                Task.FromResult<IEnumerable<MissionCompletion>>(_completions.Where(c => c.UserId == userId).ToList());

            public Task AddAsync(MissionCompletion completion) { _completions.Add(completion); return Task.CompletedTask; }
        }

        private class FakeStreakService : IStreakService
        {
            private readonly Dictionary<int, int> _streaksByUserId;
            public FakeStreakService(Dictionary<int, int> streaksByUserId) => _streaksByUserId = streaksByUserId;
            public Task<int> GetCurrentStreakAsync(int userId) => Task.FromResult(_streaksByUserId.GetValueOrDefault(userId));
        }

        [Fact]
        public async Task GetAllTimeLeaderboardAsync_OrdersByXpDescending_StreakDoesNotAffectOrder()
        {
            var members = new[]
            {
                new GroupMember { Id = 1, GroupId = 1, UserId = 1 },
                new GroupMember { Id = 2, GroupId = 1, UserId = 2 },
            };
            var users = new[]
            {
                new User { Id = 1, Name = "Bajo XP Alta Racha" },
                new User { Id = 2, Name = "Alto XP Baja Racha" },
            };
            var completions = new[]
            {
                new MissionCompletion { UserId = 1, XpAwarded = 10, CompletedAt = DateTime.UtcNow },
                new MissionCompletion { UserId = 2, XpAwarded = 90, CompletedAt = DateTime.UtcNow },
            };
            // El usuario con MENOS xp tiene la racha más alta: si la racha ordenara, el resultado sería distinto.
            var streaks = new Dictionary<int, int> { [1] = 30, [2] = 1 };

            var service = new LeaderboardService(
                new FakeGroupMemberRepository(members),
                new FakeUserRepository(users),
                new FakeMissionCompletionRepository(completions),
                new FakeStreakService(streaks));

            var result = (await service.GetAllTimeLeaderboardAsync(groupId: 1)).ToList();

            Assert.Equal("Alto XP Baja Racha", result[0].Name);
            Assert.Equal(90, result[0].Xp);
            Assert.Equal(1, result[0].Streak);
            Assert.Equal("Bajo XP Alta Racha", result[1].Name);
            Assert.Equal(30, result[1].Streak);
        }

        [Fact]
        public async Task GetWeeklyLeaderboardAsync_OnlyCountsCompletionsFromTheCurrentWeek()
        {
            var members = new[] { new GroupMember { Id = 1, GroupId = 1, UserId = 1 } };
            var users = new[] { new User { Id = 1, Name = "Solo Member" } };
            var completions = new[]
            {
                new MissionCompletion { UserId = 1, XpAwarded = 10, CompletedAt = DateTime.UtcNow },
                new MissionCompletion { UserId = 1, XpAwarded = 100, CompletedAt = DateTime.UtcNow.AddDays(-14) },
            };

            var service = new LeaderboardService(
                new FakeGroupMemberRepository(members),
                new FakeUserRepository(users),
                new FakeMissionCompletionRepository(completions),
                new FakeStreakService(new Dictionary<int, int>()));

            var weekly = (await service.GetWeeklyLeaderboardAsync(groupId: 1)).Single();
            var allTime = (await service.GetAllTimeLeaderboardAsync(groupId: 1)).Single();

            Assert.Equal(10, weekly.Xp);
            Assert.Equal(110, allTime.Xp);
        }
    }
}
