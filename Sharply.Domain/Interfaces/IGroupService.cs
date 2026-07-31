using Sharply.Domain.Enums;
using Sharply.Domain.Models;

namespace Sharply.Domain.Interfaces
{
    public interface IGroupService
    {
        Task<Group?> GetGroupForUserAsync(int userId);
        Task<IEnumerable<GroupMember>> GetMembersAsync(int groupId);
        Task<IEnumerable<GroupSkill>> GetGroupSkillsAsync(int groupId);

        Task<Group> CreateGroupAsync(int ownerUserId, string groupName);

        /// <returns>false si el código de invitación no existe o el usuario ya pertenece a un grupo.</returns>
        Task<bool> JoinGroupAsync(int userId, string inviteCode);

        Task AddGroupSkillAsync(int groupId, string name, Level level, SkillPriority priority);

        /// <summary>Si quien sale es el dueño, se borra el grupo entero (los miembros quedan, sus skills pasan a ser personales).</summary>
        Task LeaveGroupAsync(int userId);
    }
}
