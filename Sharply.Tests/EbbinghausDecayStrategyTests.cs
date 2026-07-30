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

        [Fact]
        public void Calculate_WithIntermediateMediumAtStabilityBoundary_ReturnsExponentialDecayConstant()
        {
            var strategy = new EbbinghausDecayStrategy();

            // Con mastery=Intermediate (stability=30) y priority=Medium (multiplicador=1.0),
            // daysInactive=30 hace que daysInactive/stability = 1, quedando retention = e^-1.
            var retention = strategy.Calculate(
                initialRetention: 1.0,
                daysInactive: 30,
                mastery: MasteryLevel.Intermediate,
                priority: SkillPriority.Medium);

            Assert.Equal(0.3679, retention, precision: 4);
        }

        [Fact]
        public void Calculate_ComparesSharpHighVsRustyLow_HigherMasteryAndPriorityDecaySlower()
        {
            var strategy = new EbbinghausDecayStrategy();
            const int daysInactive = 15;

            var sharpHighRetention = strategy.Calculate(1.0, daysInactive, MasteryLevel.Sharp, SkillPriority.High);
            var rustyLowRetention = strategy.Calculate(1.0, daysInactive, MasteryLevel.Rusty, SkillPriority.Low);

            Assert.True(sharpHighRetention > rustyLowRetention);
        }
    }
}
