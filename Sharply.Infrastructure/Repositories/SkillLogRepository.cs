using Sharply.Domain.Interfaces;
using Sharply.Domain.Models;
using Sharply.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Sharply.Infrastructure.Repositories;
public class SkillLogRepository : ISkillLogRepository
{
    private readonly AppDbContext _context;

    public SkillLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SkillLog>> GetAllAsync()
    {
        return await _context.SkillLogs.ToListAsync();
    }

    public async Task<SkillLog?> GetByIdAsync(int id)
    {
        return await _context.SkillLogs.FindAsync(id);
    }

    public async Task<IEnumerable<SkillLog>> GetBySkillIdAsync(int skillId)
    {
        return await _context.SkillLogs
            .Where(l => l.SkillId == skillId)
            .ToListAsync();
    }

    public async Task AddAsync(SkillLog log)
    {
        await _context.SkillLogs.AddAsync(log);
        await _context.SaveChangesAsync();
    }
}
