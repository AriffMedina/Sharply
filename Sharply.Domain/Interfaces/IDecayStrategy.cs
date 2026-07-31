using Sharply.Domain.Enums;

namespace Sharply.Domain.Interfaces
{
    public interface IDecayStrategy
    {
        double Calculate(double initialRetention, int daysInactive, Level level, SkillPriority priority);
    }
}