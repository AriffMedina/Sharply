using System;
using System.Collections.Generic;
using System.Text;

namespace Sharply.Domain.Interfaces
{
    public interface IEmailService
    {
        Task SendDecayAlarmAsync(string toEmail, string skillName, int daysInactive);
    }
}