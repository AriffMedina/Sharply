namespace Sharply.Domain.Interfaces
{
    public interface IStreakService
    {
        Task<int> GetCurrentStreakAsync(int userId);
    }
}
