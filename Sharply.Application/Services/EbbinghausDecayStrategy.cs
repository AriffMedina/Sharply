using Sharply.Domain.Enums;
using Sharply.Domain.Interfaces;

namespace Sharply.Application.Services
{
    public class EbbinghausDecayStrategy : IDecayStrategy
    {
        public double Calculate(double initialRetention, int daysInactive, MasteryLevel mastery, SkillPriority priority)
        {
            var stability = GetStabilityConstant(mastery, priority);
            var retention = initialRetention * Math.Exp(-(double)daysInactive / stability);
            return Math.Round(retention, 4);
        }

        private static double GetStabilityConstant(MasteryLevel mastery, SkillPriority priority)
        {
            double baseStability = mastery switch
            {
                MasteryLevel.Sharp => 60,
                MasteryLevel.Intermediate => 30,
                MasteryLevel.Rusty => 10,
                _ => 30
            };

            double multiplier = priority switch
            {
                SkillPriority.High => 1.5,
                SkillPriority.Low => 0.7,
                _ => 1.0
            };

            return baseStability * multiplier;
        }
    }
}