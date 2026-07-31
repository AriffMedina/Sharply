using Sharply.Domain.Models;

namespace Sharply.Domain.Interfaces
{
    public interface IMissionService
    {
        Task<IEnumerable<Mission>> GetActiveMissionsAsync();
        Task<IEnumerable<MissionCompletion>> GetCompletionsForCurrentPeriodAsync(int userId);

        /// <summary>
        /// Evalúa las misiones pendientes del usuario tras una práctica y otorga XP a las que se completaron.
        /// </summary>
        /// <param name="skillWasAtRisk">Si la skill recién practicada estaba en riesgo antes de este log (dispara "Rescata una skill en riesgo").</param>
        Task EvaluateMissionsAsync(int userId, bool skillWasAtRisk);
    }
}
