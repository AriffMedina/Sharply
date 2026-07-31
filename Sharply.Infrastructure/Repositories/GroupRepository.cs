using Microsoft.EntityFrameworkCore;
using Sharply.Domain.Interfaces;
using Sharply.Domain.Models;
using Sharply.Infrastructure.Data;

namespace Sharply.Infrastructure.Repositories;

public class GroupRepository : IGroupRepository
{
    private readonly AppDbContext _context;

    public GroupRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Group?> GetByIdAsync(int id)
    {
        return await _context.Groups.FindAsync(id);
    }

    public async Task<Group?> GetByInviteCodeAsync(string inviteCode)
    {
        return await _context.Groups.FirstOrDefaultAsync(g => g.InviteCode == inviteCode);
    }

    public async Task AddAsync(Group group)
    {
        await _context.Groups.AddAsync(group);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var group = await _context.Groups.FindAsync(id);
        if (group is not null)
        {
            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();
        }
    }
}
