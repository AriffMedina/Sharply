using Sharply.Domain.Enums;

namespace Sharply.Application.Services
{
    /// <summary>Arma el prompt para el generador de sugerencias de práctica (Fase 4).
    /// Solo manda nombre de skill + nivel — nunca datos personales del usuario.</summary>
    public static class SuggestionPromptBuilder
    {
        public static string Build(string skillName, Level level)
        {
            var difficulty = level switch
            {
                Level.Beginner => "beginner",
                Level.Advanced => "advanced",
                _ => "intermediate"
            };

            return
                $"You are a technical practice mentor. Give ONE short, concrete practice suggestion " +
                $"(max 2-3 sentences) to reinforce the skill '{skillName}', aimed at a {difficulty} level. " +
                "Phrase it as a suggestion, not an obligation. Do not add greetings or extra explanations, " +
                "return only the suggestion text.";
        }
    }
}
