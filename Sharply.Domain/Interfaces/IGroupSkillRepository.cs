using Sharply.Domain.Models;

namespace Sharply.Domain.Interfaces
{
    public interface IGroupSkillRepository
    {
        Task<IEnumerable<GroupSkill>> GetByGroupIdAsync(int groupId);
        Task AddAsync(GroupSkill groupSkill);
        Task DeleteByGroupIdAsync(int groupId);
    }
}
