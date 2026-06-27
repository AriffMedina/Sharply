using Sharply.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sharply.Domain.Interfaces
{
    public interface ISkillDecayObserver
    {
        Task UpdateAsync(Skill skillAtRisk, User user);
    }
}
