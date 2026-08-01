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
                Level.Beginner => "básico",
                Level.Advanced => "avanzado",
                _ => "intermedio"
            };

            return
                $"Sos un mentor de práctica técnica. Dá UNA sola sugerencia de práctica breve y concreta " +
                $"(máximo 2-3 oraciones) para reforzar la habilidad '{skillName}', pensada para un nivel {difficulty}. " +
                "Redactala como propuesta, no como obligación. No agregues saludos ni explicaciones extra, " +
                "devolvé solamente el texto de la sugerencia.";
        }
    }
}
