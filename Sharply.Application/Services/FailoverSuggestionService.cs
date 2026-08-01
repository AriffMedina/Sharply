using Sharply.Domain.Enums;
using Sharply.Domain.Interfaces;

namespace Sharply.Application.Services
{
    /// <summary>Composite de failover (Fase 4): intenta el proveedor primario, si falla o no
    /// devuelve nada intenta el secundario, y si ambos fallan degrada a null sin romper el flujo
    /// de decay (nunca truena, el email/dashboard simplemente quedan sin sugerencia).</summary>
    public class FailoverSuggestionService : IPracticeSuggestionService
    {
        private readonly IPracticeSuggestionService _primary;
        private readonly IPracticeSuggestionService _secondary;

        public FailoverSuggestionService(IPracticeSuggestionService primary, IPracticeSuggestionService secondary)
        {
            _primary = primary;
            _secondary = secondary;
        }

        public async Task<string?> GenerateAsync(string skillName, Level level, CancellationToken ct)
        {
            var fromPrimary = await TryGenerateAsync(_primary, skillName, level, ct);
            if (!string.IsNullOrWhiteSpace(fromPrimary))
                return fromPrimary;

            var fromSecondary = await TryGenerateAsync(_secondary, skillName, level, ct);
            return string.IsNullOrWhiteSpace(fromSecondary) ? null : fromSecondary;
        }

        private static async Task<string?> TryGenerateAsync(
            IPracticeSuggestionService provider, string skillName, Level level, CancellationToken ct)
        {
            try
            {
                return await provider.GenerateAsync(skillName, level, ct);
            }
            catch
            {
                return null;
            }
        }
    }
}
