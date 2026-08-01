using Sharply.Domain.Enums;

namespace Sharply.Domain.Interfaces
{
    public interface IDecayStrategyResolver
    {
        IDecayStrategy Resolve(DecayStrategyType strategyType);
    }
}
