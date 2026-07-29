using System.Threading.Tasks;
using Sharply.Domain.Models;

namespace Sharply.Domain.Interfaces
{
    public interface IAuthService
    {
        Task<User?> AuthenticateAsync(string email, string password);
        Task<bool> RegisterAsync(User user, string password);
        Task<bool> EmailExistsAsync(string email);
    }
}