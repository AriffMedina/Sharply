using Sharply.Domain.Models;

namespace Sharply.Domain.Interfaces
{
    public interface IGroupRepository
    {
        Task<Group?> GetByIdAsync(int id);
        Task<Group?> GetByInviteCodeAsync(string inviteCode);
        Task AddAsync(Group group);
        Task DeleteAsync(int id);
    }
}
