using Sharply.Domain.Enums;
using Sharply.Domain.Interfaces;

namespace Sharply.Application.Services
{
    /// <summary>Implementación de referencia del patrón Strategy — hoy no está registrada en ningún Program.cs.</summary>
    public class LinearDecayStrategy : IDecayStrategy
    {
        public double Calculate(double initialRetention, int daysInactive, Level level, SkillPriority priority)
        {
            double decayPerDay = level switch
            {
                Level.Advanced => 0.01,
                Level.Intermediate => 0.02,
                Level.Beginner => 0.05,
                _ => 0.02
            };

            return Math.Round(Math.Max(0, initialRetention - (decayPerDay * daysInactive)), 4);
        }
    }
}