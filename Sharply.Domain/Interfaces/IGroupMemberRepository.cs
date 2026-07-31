using Sharply.Domain.Models;

namespace Sharply.Domain.Interfaces
{
    public interface IGroupMemberRepository
    {
        Task<GroupMember?> GetByUserIdAsync(int userId);
        Task<IEnumerable<GroupMember>> GetByGroupIdAsync(int groupId);
        Task AddAsync(GroupMember member);
        Task DeleteAsync(int id);
        Task DeleteByGroupIdAsync(int groupId);
    }
}
