using Sharply.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sharply.Domain.Interfaces
{
    public interface ISkillLogRepository
    {
        Task<IEnumerable<SkillLog>> GetAllAsync();
        Task<SkillLog?> GetByIdAsync(int id);
        Task<IEnumerable<SkillLog>> GetBySkillIdAsync(int skillId);
        Task AddAsync(SkillLog log);
    }S
}
