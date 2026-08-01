using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using Sharply.Domain.Enums;
using Sharply.Domain.Interfaces;
using Sharply.Domain.Models;
using System.Threading.Tasks;

namespace Sharply.Infrastructure.Messaging
{
    public class EmailService : IEmailService, ISkillDecayObserver
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendDecayAlarmAsync(string toEmail, string skillName, int daysInactive, Level level, string? suggestion = null)
        {
            string senderEmail = _config["EmailSettings:SenderEmail"]
                ?? throw new InvalidOperationException("Falta EmailSettings:SenderEmail en la configuración");
            string smtpServer = _config["EmailSettings:SmtpServer"]
                ?? throw new InvalidOperationException("Falta EmailSettings:SmtpServer en la configuración");
            string smtpPort = _config["EmailSettings:SmtpPort"]
                ?? throw new InvalidOperationException("Falta EmailSettings:SmtpPort en la configuración");
            string password = _config["EmailSettings:Password"]
                ?? throw new InvalidOperationException("Falta EmailSettings:Password en la configuración");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                _config["EmailSettings:SenderName"],
                senderEmail));

            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = $"⚠️ Alerta Sharply: Estás olvidando {skillName}";

            var levelLabel = level switch
            {
                Level.Beginner => "Principiante",
                Level.Advanced => "Avanzado",
                _ => "Intermedio"
            };

            var suggestionParagraph = string.IsNullOrWhiteSpace(suggestion)
                ? string.Empty
                : $"\n\nSugerencia de práctica: {suggestion}";

            message.Body = new TextPart("plain")
            {
                Text = $"Hola!\n\nHan pasado {daysInactive} días desde que practicaste '{skillName}' (nivel {levelLabel}).\n" +
                       $"Tu retención de esta habilidad está cayendo. ¡Entra a Sharply y haz un repaso rápido!" +
                       $"{suggestionParagraph}\n\nSaludos,\nEl equipo de Sharply"
            };

            using var client = new MailKit.Net.Smtp.SmtpClient();
            try
            {
                await client.ConnectAsync(smtpServer, int.Parse(smtpPort), SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(senderEmail, password);
                await client.SendAsync(message);
            }
            finally
            {
                await client.DisconnectAsync(true);
            }


        }

        // Patrón Observer
        public async Task UpdateAsync(Skill skillAtRisk, User user)
        {
            var daysInactive = (System.DateTime.UtcNow - skillAtRisk.LastPracticedAt).Days;

            await SendDecayAlarmAsync(user.Email, skillAtRisk.Name, daysInactive, skillAtRisk.Level, skillAtRisk.CurrentSuggestion);
        }
    }
}