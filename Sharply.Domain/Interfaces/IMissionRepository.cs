using Sharply.Domain.Models;

namespace Sharply.Domain.Interfaces
{
    public interface IMissionRepository
    {
        Task<IEnumerable<Mission>> GetAllAsync();
    }
}
