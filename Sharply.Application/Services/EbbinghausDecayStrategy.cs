using Sharply.Domain.Enums;
using Sharply.Domain.Interfaces;

namespace Sharply.Application.Services
{
    public class EbbinghausDecayStrategy : IDecayStrategy
    {
        public double Calculate(double initialRetention, int daysInactive, Level level, SkillPriority priority)
        {
            var stability = GetStabilityConstant(level, priority);
            var retention = initialRetention * Math.Exp(-(double)daysInactive / stability);
            return Math.Round(retention, 4);
        }

        private static double GetStabilityConstant(Level level, SkillPriority priority)
        {
            double baseStability = level switch
            {
                Level.Advanced => 60,
                Level.Intermediate => 30,
                Level.Beginner => 10,
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