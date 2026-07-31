using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sharply.Application.Services;
using Sharply.Domain.Interfaces;
using Sharply.Infrastructure.Data;

namespace Sharply.Infrastructure.Jobs
{
    public class DecayWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DecayWorker> _logger;

        // Intervalo de revisión: cada 24 horas
        private readonly TimeSpan _interval = TimeSpan.FromHours(24);

        public DecayWorker(IServiceProvider serviceProvider, ILogger<DecayWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("DecayWorker iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RunDecayCheckAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fallo en ciclo de DecayWorker");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }

        private async Task RunDecayCheckAsync()
        {

            using var scope = _serviceProvider.CreateScope();

            var decayService = scope.ServiceProvider.GetRequiredService<ISkillDecayService>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var notifier = new SkillDecayNotifier();
            notifier.Attach((ISkillDecayObserver)emailService);

            var users = await context.Users
                .Include(u => u.Skills)
                .ToListAsync();

            foreach (var user in users)
            {
                if (!user.DecayEmailEnabled)
                    continue;

                var skillsAtRisk = await decayService.GetSkillsAtRiskAsync(user.Id, user.DecayRetentionThreshold);

                foreach (var skill in skillsAtRisk)
                {
                    _logger.LogInformation(
                        "Skill en riesgo: {SkillName} para usuario {UserId}. Notificando...",
                        skill.Name, user.Id);

                    await notifier.NotifyAsync(skill, user);
                }
            }
        }
    }
}
