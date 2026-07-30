using Sharply.Application.Services;
using Sharply.Domain.Enums;

namespace Sharply.Tests
{
    public class EbbinghausDecayStrategyTests
    {
        [Fact]
        public void Calculate_WithZeroDaysInactive_ReturnsInitialRetention()
        {
            var strategy = new EbbinghausDecayStrategy();

            var retention = strategy.Calculate(
                initialRetention: 0.85,
                daysInactive: 0,
                mastery: MasteryLevel.Rusty,
                priority: SkillPriority.Low);

            Assert.Equal(0.85, retention);
        }
    }
}
