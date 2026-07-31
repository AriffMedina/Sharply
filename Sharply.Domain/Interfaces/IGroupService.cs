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

        /// <summary>Actualiza nombre/nivel/prioridad de la GroupSkill y propaga el cambio a la Skill de cada miembro.</summary>
        Task UpdateGroupSkillAsync(int groupId, int groupSkillId, string name, Level level, SkillPriority priority);

        /// <summary>Borra la GroupSkill y la Skill propagada de cada miembro (no queda ni como skill personal).</summary>
        Task DeleteGroupSkillAsync(int groupId, int groupSkillId);

        /// <summary>Si quien sale es el dueño, se borra el grupo entero. En ambos casos (dueño o miembro),
        /// las skills que venían del grupo se borran por completo — no pasan a ser personales.</summary>
        Task LeaveGroupAsync(int userId);
    }
}
