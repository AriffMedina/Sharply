using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Sharply.Application.Services;
using Sharply.Domain.Enums;
using Sharply.Domain.Interfaces;

namespace Sharply.Infrastructure.AI
{
    /// <summary>Adaptador primario de sugerencias (Fase 4): Groq, API compatible con OpenAI (gratuita, rápida).</summary>
    public class GroqSuggestionAdapter : IPracticeSuggestionService
    {
        private const string Endpoint = "https://api.groq.com/openai/v1/chat/completions";

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public GroqSuggestionAdapter(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<string?> GenerateAsync(string skillName, Level level, CancellationToken ct)
        {
            var apiKey = _config["Groq:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("Falta Groq:ApiKey en la configuración");

            var model = _config["Groq:Model"] ?? "llama-3.1-8b-instant";
            var prompt = SuggestionPromptBuilder.Build(skillName, level);

            var payload = new
            {
                model,
                messages = new[] { new { role = "user", content = prompt } },
                max_tokens = 150,
                temperature = 0.7
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            using var response = await _httpClient.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(body);

            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
        }
    }
}
