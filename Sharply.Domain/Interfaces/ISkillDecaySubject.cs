using Sharply.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sharply.Domain.Interfaces
{
    public interface ISkillDecaySubject
    {
        void Attach(ISkillDecayObserver observer);
        void Detach(ISkillDecayObserver observer);
        Task NotifyAsync(Skill skillAtRisk, User user);
    }
}
