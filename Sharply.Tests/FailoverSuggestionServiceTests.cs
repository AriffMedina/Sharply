using Sharply.Application.Services;
using Sharply.Domain.Enums;
using Sharply.Domain.Interfaces;

namespace Sharply.Tests
{
    public class FailoverSuggestionServiceTests
    {
        private class FakeSuggestionProvider : IPracticeSuggestionService
        {
            private readonly string? _result;
            private readonly bool _throws;

            public int CallCount { get; private set; }

            public FakeSuggestionProvider(string? result = null, bool throws = false)
            {
                _result = result;
                _throws = throws;
            }

            public Task<string?> GenerateAsync(string skillName, Level level, CancellationToken ct)
            {
                CallCount++;
                if (_throws)
                    throw new InvalidOperationException("Provider failure (simulated)");
                return Task.FromResult(_result);
            }
        }

        [Fact]
        public async Task GenerateAsync_PrimarySucceeds_ReturnsPrimaryResultAndNeverCallsSecondary()
        {
            var primary = new FakeSuggestionProvider(result: "Practica un for loop.");
            var secondary = new FakeSuggestionProvider(result: "No debería llegar acá.");
            var service = new FailoverSuggestionService(primary, secondary);

            var result = await service.GenerateAsync("C#", Level.Beginner, CancellationToken.None);

            Assert.Equal("Practica un for loop.", result);
            Assert.Equal(1, primary.CallCount);
            Assert.Equal(0, secondary.CallCount);
        }

        [Fact]
        public async Task GenerateAsync_PrimaryThrows_FallsBackToSecondary()
        {
            var primary = new FakeSuggestionProvider(throws: true);
            var secondary = new FakeSuggestionProvider(result: "Repasá closures hoy.");
            var service = new FailoverSuggestionService(primary, secondary);

            var result = await service.GenerateAsync("JavaScript", Level.Advanced, CancellationToken.None);

            Assert.Equal("Repasá closures hoy.", result);
            Assert.Equal(1, secondary.CallCount);
        }

        [Fact]
        public async Task GenerateAsync_PrimaryReturnsEmpty_FallsBackToSecondary()
        {
            var primary = new FakeSuggestionProvider(result: "   ");
            var secondary = new FakeSuggestionProvider(result: "Practica queries SQL.");
            var service = new FailoverSuggestionService(primary, secondary);

            var result = await service.GenerateAsync("SQL", Level.Intermediate, CancellationToken.None);

            Assert.Equal("Practica queries SQL.", result);
        }

        [Fact]
        public async Task GenerateAsync_BothFail_ReturnsNullWithoutThrowing()
        {
            var primary = new FakeSuggestionProvider(throws: true);
            var secondary = new FakeSuggestionProvider(throws: true);
            var service = new FailoverSuggestionService(primary, secondary);

            var result = await service.GenerateAsync("Rust", Level.Advanced, CancellationToken.None);

            Assert.Null(result);
        }
    }
}
