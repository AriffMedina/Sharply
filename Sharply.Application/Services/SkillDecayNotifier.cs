using Sharply.Domain.Interfaces;
using Sharply.Domain.Models;
using System.Collections.Generic;

namespace Sharply.Application.Services
{
    public class SkillDecayNotifier : ISkillDecaySubject
    {
        private readonly List<ISkillDecayObserver> _observers = new();

        public void Attach(ISkillDecayObserver observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }
        }

        public void Detach(ISkillDecayObserver observer)
        {
            if (_observers.Contains(observer))
            {
                _observers.Remove(observer);
            }
        }

        public void Notify(Skill skillAtRisk, User user)
        {
            foreach (var observer in _observers)
            {
                observer.Update(skillAtRisk, user);
            }
        }
    }
}