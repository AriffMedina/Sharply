using Microsoft.EntityFrameworkCore;
using Sharply.Domain.Interfaces;
using Sharply.Domain.Models;
using Sharply.Infrastructure.Data;

namespace Sharply.Infrastructure.Repositories;

public class GroupSkillRepository : IGroupSkillRepository
{
    private readonly AppDbContext _context;

    public GroupSkillRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<GroupSkill?> GetByIdAsync(int id)
    {
        return await _context.GroupSkills.FindAsync(id);
    }

    public async Task<IEnumerable<GroupSkill>> GetByGroupIdAsync(int groupId)
    {
        return await _context.GroupSkills
            .Where(gs => gs.GroupId == groupId)
            .ToListAsync();
    }

    public async Task AddAsync(GroupSkill groupSkill)
    {
        await _context.GroupSkills.AddAsync(groupSkill);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(GroupSkill groupSkill)
    {
        _context.GroupSkills.Update(groupSkill);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var groupSkill = await _context.GroupSkills.FindAsync(id);
        if (groupSkill is not null)
        {
            _context.GroupSkills.Remove(groupSkill);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteByGroupIdAsync(int groupId)
    {
        var groupSkills = await _context.GroupSkills.Where(gs => gs.GroupId == groupId).ToListAsync();
        _context.GroupSkills.RemoveRange(groupSkills);
        await _context.SaveChangesAsync();
    }
}
