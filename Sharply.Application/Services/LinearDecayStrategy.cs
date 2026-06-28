using Sharply.Domain.Enums;
using Sharply.Domain.Interfaces;

namespace Sharply.Application.Services
{
    public class LinearDecayStrategy : IDecayStrategy
    {
        public double Calculate(double initialRetention, int daysInactive, MasteryLevel mastery, SkillPriority priority)
        {
            double decayPerDay = mastery switch
            {
                MasteryLevel.Sharp => 0.01,
                MasteryLevel.Intermediate => 0.02,
                MasteryLevel.Rusty => 0.05,
                _ => 0.02
            };

            return Math.Round(Math.Max(0, initialRetention - (decayPerDay * daysInactive)), 4);
        }
    }
}