using Sharply.Domain.Enums;

namespace Sharply.Domain.Interfaces
{
    public interface IPracticeSuggestionService
    {
        Task<string?> GenerateAsync(string skillName, Level level, CancellationToken ct);
    }
}
