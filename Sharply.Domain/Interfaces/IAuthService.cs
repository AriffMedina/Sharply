using System.Threading.Tasks;
using Sharply.Domain.Models;

namespace Sharply.Application.Interfaces
{
    public interface IAuthService
    {
        Task<User?> AuthenticateAsync(string email, string password);
        Task<bool> RegisterAsync(User user, string password);
    }
}