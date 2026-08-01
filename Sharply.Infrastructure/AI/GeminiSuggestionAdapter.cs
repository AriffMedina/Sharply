using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Sharply.Application.Services;
using Sharply.Domain.Enums;
using Sharply.Domain.Interfaces;

namespace Sharply.Infrastructure.AI
{
    /// <summary>Adaptador de respaldo de sugerencias (Fase 4): Gemini (Google AI Studio, tier gratuito).</summary>
    public class GeminiSuggestionAdapter : IPracticeSuggestionService
    {
        private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/models";

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public GeminiSuggestionAdapter(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<string?> GenerateAsync(string skillName, Level level, CancellationToken ct)
        {
            var apiKey = _config["Gemini:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("Falta Gemini:ApiKey en la configuración");

            var model = _config["Gemini:Model"] ?? "gemini-1.5-flash";
            var prompt = SuggestionPromptBuilder.Build(skillName, level);

            var payload = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                }
            };

            var url = $"{BaseUrl}/{model}:generateContent?key={apiKey}";
            using var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            using var response = await _httpClient.PostAsync(url, content, ct);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(body);

            return doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();
        }
    }
}
