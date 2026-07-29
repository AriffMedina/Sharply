using BCrypt.Net;
using Sharply.Domain.Interfaces;
using Sharply.Domain.Models;

namespace Sharply.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null) return null;

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

            if (!isPasswordValid) return null;

            return user;
        }

        public async Task<bool> RegisterAsync(User user, string password)
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

            await _userRepository.AddAsync(user);

            return true;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _userRepository.EmailExistsAsync(email);
        }
    }
}
