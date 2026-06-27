using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
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

        public async Task SendDecayAlarmAsync(string toEmail, string skillName, int daysInactive)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                _config["EmailSettings:SenderName"],
                _config["EmailSettings:SenderEmail"]));

            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = $"⚠️ Alerta Sharply: Estás olvidando {skillName}";

            message.Body = new TextPart("plain")
            {
                Text = $"Hola!\n\nHan pasado {daysInactive} días desde que practicaste '{skillName}'.\n" +
                       $"Tu retención de esta habilidad está cayendo. ¡Entra a Sharply y haz un repaso rápido!\n\nSaludos,\nEl equipo de Sharply"
            };

            using var client = new MailKit.Net.Smtp.SmtpClient();
            try
            {
                await client.ConnectAsync(_config["EmailSettings:SmtpServer"], int.Parse(_config["EmailSettings:SmtpPort"]), SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_config["EmailSettings:SenderEmail"], _config["EmailSettings:Password"]);
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

            await SendDecayAlarmAsync(user.Email, skillAtRisk.Name, daysInactive);
        }
    }
}