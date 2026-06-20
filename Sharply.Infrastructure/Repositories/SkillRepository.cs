using Microsoft.EntityFrameworkCore;
using Sharply.Domain.Models;
using Sharply.Domain.Interfaces;
using Sharply.Infrastructure.Data;

namespace Sharply.Infrastructure.Repositories;

public class SkillRepository : ISkillRepository
{
    private readonly AppDbContext _context;

    public SkillRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Skill>> GetAllAsync()
    {
        return await _context.Skills.ToListAsync();
    }

    public async Task<Skill?> GetByIdAsync(int id)
    {
        return await _context.Skills.FindAsync(id);
    }

    public async Task<IEnumerable<Skill>> GetByUserIdAsync(int userId)
    {
        return await _context.Skills
            .Where(s => s.UserId == userId)
            .ToListAsync();
    }

    public async Task AddAsync(Skill skill)
    {
        await _context.Skills.AddAsync(skill);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Skill skill)
    {
        _context.Skills.Update(skill);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var skill = await _context.Skills.FindAsync(id);
        if (skill is not null)
        {
            _context.Skills.Remove(skill);
            await _context.SaveChangesAsync();
        }
    }
}