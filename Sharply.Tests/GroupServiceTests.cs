using Sharply.Application.Services;
using Sharply.Domain.Enums;
using Sharply.Domain.Interfaces;
using Sharply.Domain.Models;

namespace Sharply.Tests
{
    public class GroupServiceTests
    {
        private class FakeGroupRepository : IGroupRepository
        {
            public List<Group> Groups { get; } = new();
            private int _nextId = 1;

            public Task<Group?> GetByIdAsync(int id) => Task.FromResult(Groups.FirstOrDefault(g => g.Id == id));
            public Task<Group?> GetByInviteCodeAsync(string inviteCode) =>
                Task.FromResult(Groups.FirstOrDefault(g => g.InviteCode == inviteCode));

            public Task AddAsync(Group group)
            {
                group.Id = _nextId++;
                Groups.Add(group);
                return Task.CompletedTask;
            }

            public Task DeleteAsync(int id) { Groups.RemoveAll(g => g.Id == id); return Task.CompletedTask; }
        }

        private class FakeGroupMemberRepository : IGroupMemberRepository
        {
            public List<GroupMember> Members { get; } = new();
            private int _nextId = 1;

            public Task<GroupMember?> GetByUserIdAsync(int userId) =>
                Task.FromResult(Members.FirstOrDefault(m => m.UserId == userId));

            public Task<IEnumerable<GroupMember>> GetByGroupIdAsync(int groupId) =>
                Task.FromResult<IEnumerable<GroupMember>>(Members.Where(m => m.GroupId == groupId).ToList());

            public Task AddAsync(GroupMember member)
            {
                member.Id = _nextId++;
                Members.Add(member);
                return Task.CompletedTask;
            }

            public Task DeleteAsync(int id) { Members.RemoveAll(m => m.Id == id); return Task.CompletedTask; }
            public Task DeleteByGroupIdAsync(int groupId) { Members.RemoveAll(m => m.GroupId == groupId); return Task.CompletedTask; }
        }

        private class FakeGroupSkillRepository : IGroupSkillRepository
        {
            public List<GroupSkill> GroupSkills { get; } = new();
            private int _nextId = 1;

            public Task<IEnumerable<GroupSkill>> GetByGroupIdAsync(int groupId) =>
                Task.FromResult<IEnumerable<GroupSkill>>(GroupSkills.Where(gs => gs.GroupId == groupId).ToList());

            public Task AddAsync(GroupSkill groupSkill)
            {
                groupSkill.Id = _nextId++;
                GroupSkills.Add(groupSkill);
                return Task.CompletedTask;
            }

            public Task DeleteByGroupIdAsync(int groupId) { GroupSkills.RemoveAll(gs => gs.GroupId == groupId); return Task.CompletedTask; }
        }

        private class FakeSkillRepository : ISkillRepository
        {
            public List<Skill> Skills { get; } = new();
            private int _nextId = 1;

            public Task<IEnumerable<Skill>> GetAllAsync() => Task.FromResult<IEnumerable<Skill>>(Skills);
            public Task<Skill?> GetByIdAsync(int id) => Task.FromResult(Skills.FirstOrDefault(s => s.Id == id));
            public Task<IEnumerable<Skill>> GetByUserIdAsync(int userId) =>
                Task.FromResult<IEnumerable<Skill>>(Skills.Where(s => s.UserId == userId).ToList());
            public Task<IEnumerable<Skill>> GetByGroupIdAsync(int groupId) =>
                Task.FromResult<IEnumerable<Skill>>(Skills.Where(s => s.GroupId == groupId).ToList());

            public Task AddAsync(Skill skill)
            {
                skill.Id = _nextId++;
                Skills.Add(skill);
                return Task.CompletedTask;
            }

            public Task UpdateAsync(Skill skill) => Task.CompletedTask;
            public Task DeleteAsync(int id) { Skills.RemoveAll(s => s.Id == id); return Task.CompletedTask; }
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

        private static GroupService BuildService(
            FakeGroupRepository groups, FakeGroupMemberRepository members,
            FakeGroupSkillRepository groupSkills, FakeSkillRepository skills, FakeUserRepository users) =>
            new(groups, members, groupSkills, skills, users);

        [Fact]
        public async Task AddGroupSkillAsync_PropagatesToAllCurrentMembers()
        {
            var groups = new FakeGroupRepository();
            var members = new FakeGroupMemberRepository();
            var groupSkills = new FakeGroupSkillRepository();
            var skills = new FakeSkillRepository();
            var users = new FakeUserRepository(new[] { new User { Id = 1, Name = "Owner" }, new User { Id = 2, Name = "Member" } });
            var service = BuildService(groups, members, groupSkills, skills, users);

            var group = await service.CreateGroupAsync(ownerUserId: 1, "Squad A");
            members.Members.Add(new GroupMember { Id = 99, GroupId = group.Id, UserId = 2, JoinedAt = DateTime.UtcNow });

            await service.AddGroupSkillAsync(group.Id, "React", Level.Intermediate, SkillPriority.High);

            Assert.Equal(2, skills.Skills.Count(s => s.Name == "React" && s.GroupId == group.Id));
            Assert.Contains(skills.Skills, s => s.UserId == 1 && s.Name == "React");
            Assert.Contains(skills.Skills, s => s.UserId == 2 && s.Name == "React");
        }

        [Fact]
        public async Task JoinGroupAsync_PropagatesExistingGroupSkillsRetroactively()
        {
            var groups = new FakeGroupRepository();
            var members = new FakeGroupMemberRepository();
            var groupSkills = new FakeGroupSkillRepository();
            var skills = new FakeSkillRepository();
            var users = new FakeUserRepository(new[] { new User { Id = 1, Name = "Owner" }, new User { Id = 2, Name = "NewMember" } });
            var service = BuildService(groups, members, groupSkills, skills, users);

            var group = await service.CreateGroupAsync(ownerUserId: 1, "Squad B");
            await service.AddGroupSkillAsync(group.Id, "TypeScript", Level.Advanced, SkillPriority.Medium);
            await service.AddGroupSkillAsync(group.Id, "SQL", Level.Beginner, SkillPriority.Low);

            var joined = await service.JoinGroupAsync(userId: 2, group.InviteCode);

            Assert.True(joined);
            var newMemberSkills = skills.Skills.Where(s => s.UserId == 2).ToList();
            Assert.Equal(2, newMemberSkills.Count);
            Assert.Contains(newMemberSkills, s => s.Name == "TypeScript");
            Assert.Contains(newMemberSkills, s => s.Name == "SQL");
        }

        [Fact]
        public async Task JoinGroupAsync_UserAlreadyInAGroup_ReturnsFalseAndDoesNotJoin()
        {
            var groups = new FakeGroupRepository();
            var members = new FakeGroupMemberRepository();
            var groupSkills = new FakeGroupSkillRepository();
            var skills = new FakeSkillRepository();
            var users = new FakeUserRepository(new[]
            {
                new User { Id = 1, Name = "Owner A" },
                new User { Id = 2, Name = "Owner B" },
            });
            var service = BuildService(groups, members, groupSkills, skills, users);

            var groupA = await service.CreateGroupAsync(ownerUserId: 1, "Squad A");
            var groupB = await service.CreateGroupAsync(ownerUserId: 2, "Squad B");

            // El usuario 1 ya es dueño (y miembro) de Squad A; intenta unirse a Squad B.
            var joined = await service.JoinGroupAsync(userId: 1, groupB.InviteCode);

            Assert.False(joined);
            var membership = Assert.Single(members.Members, m => m.UserId == 1);
            Assert.Equal(groupA.Id, membership.GroupId);
        }

        [Fact]
        public async Task CreateGroupAsync_UserAlreadyInAGroup_Throws()
        {
            var groups = new FakeGroupRepository();
            var members = new FakeGroupMemberRepository();
            var groupSkills = new FakeGroupSkillRepository();
            var skills = new FakeSkillRepository();
            var users = new FakeUserRepository(new[] { new User { Id = 1, Name = "Owner" } });
            var service = BuildService(groups, members, groupSkills, skills, users);

            await service.CreateGroupAsync(ownerUserId: 1, "Squad A");

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CreateGroupAsync(ownerUserId: 1, "Squad C"));
        }
    }
}
