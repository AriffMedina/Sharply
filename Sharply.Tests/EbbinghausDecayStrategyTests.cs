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
                level: Level.Beginner,
                priority: SkillPriority.Low);

            Assert.Equal(0.85, retention);
        }

        [Fact]
        public void Calculate_WithIntermediateMediumAtStabilityBoundary_ReturnsExponentialDecayConstant()
        {
            var strategy = new EbbinghausDecayStrategy();

            // Con level=Intermediate (stability=30) y priority=Medium (multiplicador=1.0),
            // daysInactive=30 hace que daysInactive/stability = 1, quedando retention = e^-1.
            var retention = strategy.Calculate(
                initialRetention: 1.0,
                daysInactive: 30,
                level: Level.Intermediate,
                priority: SkillPriority.Medium);

            Assert.Equal(0.3679, retention, precision: 4);
        }

        [Fact]
        public void Calculate_ComparesAdvancedHighVsBeginnerLow_HigherLevelAndPriorityDecaySlower()
        {
            var strategy = new EbbinghausDecayStrategy();
            const int daysInactive = 15;

            var advancedHighRetention = strategy.Calculate(1.0, daysInactive, Level.Advanced, SkillPriority.High);
            var beginnerLowRetention = strategy.Calculate(1.0, daysInactive, Level.Beginner, SkillPriority.Low);

            Assert.True(advancedHighRetention > beginnerLowRetention);
        }
    }
}
