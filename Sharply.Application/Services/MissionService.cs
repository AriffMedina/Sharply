using Sharply.Domain.Interfaces;
using Sharply.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sharply.Application.Services
{
    internal class MissionService
    {
        private readonly ISkillRepository _skillRepository;
        private readonly ISkillLogRepository _skillLogRepository;

        public MissionService(ISkillRepository skillRepository, ISkillLogRepository skillLogRepository)
        {
            _skillRepository = skillRepository;
            _skillLogRepository = skillLogRepository;
        }

        public async Task<SkillLog> CompleteMissionAsync(int skillId, string? notes)
        {
            var skill = await _skillRepository.GetByIdAsync(skillId)
                ?? throw new KeyNotFoundException($"Skill con id {skillId} no encontrada.");

            var log = new SkillLog
            {
                SkillId = skillId,
                PracticedAt = DateTime.UtcNow,
                Notes = notes
            };

            await _skillLogRepository.AddAsync(log);

            // Reinicia la curva del olvido: la skill se considera practicada hoy
            skill.LastPracticedAt = DateTime.UtcNow;
            await _skillRepository.UpdateAsync(skill);

            return log;
        }
    }
}
