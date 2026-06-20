using Sharply.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sharply.Application.Services
{
    public interface IMissionService
    {
        Task<SkillLog> CompleteMissionAsync(int skillId, string? notes);
    }
}
