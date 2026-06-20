using Sharply.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sharply.Application.Services
{
    internal interface ISkillDecayService
    {
        Task<double> CalculateRetentionAsync(Skill skill);
        Task<IEnumerable<Skill>> GetSkillsAtRiskAsync(int userId, double retentionThreshold = 0.5);
        Task<int> GetDaysInactiveAsync(Skill skill);
    }
}
