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
        private readonly IDecayStrategyResolver _decayStrategyResolver;

        public SkillDecayService(ISkillRepository skillRepository, IDecayStrategyResolver decayStrategyResolver)
        {
            _skillRepository = skillRepository;
            _decayStrategyResolver = decayStrategyResolver;
        }

        public Task<int> GetDaysInactiveAsync(Skill skill)
        {
            var days = Math.Max(0, (DateTime.UtcNow - skill.LastPracticedAt).Days);
            return Task.FromResult(days);
        }

        public async Task<double> CalculateRetentionAsync(Skill skill)
        {
            var daysInactive = await GetDaysInactiveAsync(skill);
            var strategy = _decayStrategyResolver.Resolve(skill.User.DecayStrategy);
            return strategy.Calculate(skill.InitialRetention, daysInactive, skill.Level, skill.Priority);
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
