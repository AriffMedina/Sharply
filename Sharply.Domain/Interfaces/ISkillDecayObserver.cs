using Sharply.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sharply.Domain.Interfaces
{
    public interface ISkillDecayObserver
    {
        void Update(Skill skillAtRisk, User user);
    }
}
