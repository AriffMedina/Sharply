using Sharply.Domain.Interfaces;
using Sharply.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sharply.Application.Services
{
    public class SkillDecayService : ISkillDecayService
    {
        private readonly ISkillRepository _skillRepository;

        // Constante de la curva del olvido de Ebbinghaus (factor de decaimiento)
        private const double DecayConstant = 1.0;

        public SkillDecayService(ISkillRepository skillRepository)
        {
            _skillRepository = skillRepository;
        }

        public Task<int> GetDaysInactiveAsync(Skill skill)
        {
            var days = (DateTime.UtcNow - skill.LastPracticedAt).Days;
            return Task.FromResult(days);
        }

        public async Task<double> CalculateRetentionAsync(Skill skill)
        {
            var daysInactive = await GetDaysInactiveAsync(skill);

            // R = e^(-t/S)  -> formula simplificada de la curva del olvido
            var retention = Math.Exp(-daysInactive / DecayConstant);
            return Math.Round(retention, 4);
        }

        public async Task<IEnumerable<Skill>> GetSkillsAtRiskAsync(int userId, double retentionThreshold = 0.5)
        {
            var skills = await _skillRepository.GetByUserIdAsync(userId);
            var atRisk = new List<Skill>();

            foreach (var skill in skills)
            {
                var retention = await CalculateRetentionAsync(skill);
                if (retention < retentionThreshold)
                {
                    atRisk.Add(skill);
                }
            }

            return atRisk;
        }
    }
}
