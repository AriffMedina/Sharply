using Sharply.Domain.Enums;
using Sharply.Domain.Interfaces;

namespace Sharply.Application.Services
{
    public class DecayStrategyResolver : IDecayStrategyResolver
    {
        public IDecayStrategy Resolve(DecayStrategyType strategyType) => strategyType switch
        {
            DecayStrategyType.Linear => new LinearDecayStrategy(),
            _ => new EbbinghausDecayStrategy()
        };
    }
}
