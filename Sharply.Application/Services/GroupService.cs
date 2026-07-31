using Sharply.Domain.Enums;
using Sharply.Domain.Interfaces;
using Sharply.Domain.Models;

namespace Sharply.Application.Services
{
    public class GroupService : IGroupService
    {
        private const string InviteCodeChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // sin 0/O ni 1/I/L

        private readonly IGroupRepository _groupRepository;
        private readonly IGroupMemberRepository _groupMemberRepository;
        private readonly IGroupSkillRepository _groupSkillRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly IUserRepository _userRepository;

        public GroupService(
            IGroupRepository groupRepository,
            IGroupMemberRepository groupMemberRepository,
            IGroupSkillRepository groupSkillRepository,
            ISkillRepository skillRepository,
            IUserRepository userRepository)
        {
            _groupRepository = groupRepository;
            _groupMemberRepository = groupMemberRepository;
            _groupSkillRepository = groupSkillRepository;
            _skillRepository = skillRepository;
            _userRepository = userRepository;
        }

        public async Task<Group?> GetGroupForUserAsync(int userId)
        {
            var membership = await _groupMemberRepository.GetByUserIdAsync(userId);
            return membership is null ? null : await _groupRepository.GetByIdAsync(membership.GroupId);
        }

        public Task<IEnumerable<GroupMember>> GetMembersAsync(int groupId) =>
            _groupMemberRepository.GetByGroupIdAsync(groupId);

        public Task<IEnumerable<GroupSkill>> GetGroupSkillsAsync(int groupId) =>
            _groupSkillRepository.GetByGroupIdAsync(groupId);

        public async Task<Group> CreateGroupAsync(int ownerUserId, string groupName)
        {
            var existingMembership = await _groupMemberRepository.GetByUserIdAsync(ownerUserId);
            if (existingMembership is not null)
                throw new InvalidOperationException("Ya pertenecés a un grupo. Salí del actual antes de crear uno nuevo.");

            var group = new Group
            {
                Name = groupName.Trim(),
                OwnerUserId = ownerUserId,
                InviteCode = await GenerateUniqueInviteCodeAsync(),
                CreatedAt = DateTime.UtcNow
            };
            await _groupRepository.AddAsync(group);

            await _groupMemberRepository.AddAsync(new GroupMember
            {
                GroupId = group.Id,
                UserId = ownerUserId,
                JoinedAt = DateTime.UtcNow
            });

            var owner = await _userRepository.GetByIdAsync(ownerUserId);
            if (owner is not null)
            {
                owner.Role = UserRole.Owner;
                await _userRepository.UpdateAsync(owner);
            }

            return group;
        }

        public async Task<bool> JoinGroupAsync(int userId, string inviteCode)
        {
            var existingMembership = await _groupMemberRepository.GetByUserIdAsync(userId);
            if (existingMembership is not null)
                return false;

            var group = await _groupRepository.GetByInviteCodeAsync(inviteCode.Trim());
            if (group is null)
                return false;

            await _groupMemberRepository.AddAsync(new GroupMember
            {
                GroupId = group.Id,
                UserId = userId,
                JoinedAt = DateTime.UtcNow
            });

            // Propagación retroactiva: el nuevo miembro recibe todas las GroupSkills que ya existían.
            var groupSkills = await _groupSkillRepository.GetByGroupIdAsync(group.Id);
            foreach (var groupSkill in groupSkills)
            {
                await _skillRepository.AddAsync(new Skill
                {
                    Name = groupSkill.Name,
                    Level = groupSkill.Level,
                    Priority = groupSkill.Priority,
                    InitialRetention = 1.0,
                    LastPracticedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UserId = userId,
                    GroupId = group.Id
                });
            }

            return true;
        }

        public async Task AddGroupSkillAsync(int groupId, string name, Level level, SkillPriority priority)
        {
            var groupSkill = new GroupSkill
            {
                GroupId = groupId,
                Name = name.Trim(),
                Level = level,
                Priority = priority
            };
            await _groupSkillRepository.AddAsync(groupSkill);

            var members = await _groupMemberRepository.GetByGroupIdAsync(groupId);
            foreach (var member in members)
            {
                await _skillRepository.AddAsync(new Skill
                {
                    Name = groupSkill.Name,
                    Level = groupSkill.Level,
                    Priority = groupSkill.Priority,
                    InitialRetention = 1.0,
                    LastPracticedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UserId = member.UserId,
                    GroupId = groupId
                });
            }
        }

        public async Task LeaveGroupAsync(int userId)
        {
            var membership = await _groupMemberRepository.GetByUserIdAsync(userId);
            if (membership is null) return;

            var group = await _groupRepository.GetByIdAsync(membership.GroupId);
            if (group is null) return;

            if (group.OwnerUserId == userId)
            {
                // El dueño se va: se borra el grupo entero. Las skills de TODOS los miembros
                // pasan a ser personales (no se pierde curva ni historial de nadie).
                var groupSkillInstances = await _skillRepository.GetByGroupIdAsync(group.Id);
                foreach (var skill in groupSkillInstances)
                {
                    skill.GroupId = null;
                    await _skillRepository.UpdateAsync(skill);
                }

                await _groupSkillRepository.DeleteByGroupIdAsync(group.Id);
                await _groupMemberRepository.DeleteByGroupIdAsync(group.Id);
                await _groupRepository.DeleteAsync(group.Id);

                var owner = await _userRepository.GetByIdAsync(userId);
                if (owner is not null)
                {
                    owner.Role = UserRole.Member;
                    await _userRepository.UpdateAsync(owner);
                }
            }
            else
            {
                var memberSkills = (await _skillRepository.GetByUserIdAsync(userId))
                    .Where(s => s.GroupId == group.Id);

                foreach (var skill in memberSkills)
                {
                    skill.GroupId = null;
                    await _skillRepository.UpdateAsync(skill);
                }

                await _groupMemberRepository.DeleteAsync(membership.Id);
            }
        }

        private async Task<string> GenerateUniqueInviteCodeAsync()
        {
            var random = Random.Shared;

            while (true)
            {
                var code = new string(Enumerable.Range(0, 8).Select(_ => InviteCodeChars[random.Next(InviteCodeChars.Length)]).ToArray());
                var existing = await _groupRepository.GetByInviteCodeAsync(code);
                if (existing is null)
                    return code;
            }
        }
    }
}
