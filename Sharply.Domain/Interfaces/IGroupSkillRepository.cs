using Sharply.Domain.Models;

namespace Sharply.Domain.Interfaces
{
    public interface IGroupSkillRepository
    {
        Task<GroupSkill?> GetByIdAsync(int id);
        Task<IEnumerable<GroupSkill>> GetByGroupIdAsync(int groupId);
        Task AddAsync(GroupSkill groupSkill);
        Task UpdateAsync(GroupSkill groupSkill);
        Task DeleteAsync(int id);
        Task DeleteByGroupIdAsync(int groupId);
    }
}
