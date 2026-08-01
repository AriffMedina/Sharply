using Sharply.Domain.Models;

namespace Sharply.Domain.Interfaces
{
    public interface IMissionCompletionRepository
    {
        Task<IEnumerable<MissionCompletion>> GetByUserIdAsync(int userId);
        Task AddAsync(MissionCompletion completion);
    }
}
