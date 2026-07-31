using Microsoft.EntityFrameworkCore;
using Sharply.Domain.Interfaces;
using Sharply.Domain.Models;
using Sharply.Infrastructure.Data;

namespace Sharply.Infrastructure.Repositories;

public class GroupMemberRepository : IGroupMemberRepository
{
    private readonly AppDbContext _context;

    public GroupMemberRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<GroupMember?> GetByUserIdAsync(int userId)
    {
        return await _context.GroupMembers.FirstOrDefaultAsync(m => m.UserId == userId);
    }

    public async Task<IEnumerable<GroupMember>> GetByGroupIdAsync(int groupId)
    {
        return await _context.GroupMembers
            .Where(m => m.GroupId == groupId)
            .ToListAsync();
    }

    public async Task AddAsync(GroupMember member)
    {
        await _context.GroupMembers.AddAsync(member);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var member = await _context.GroupMembers.FindAsync(id);
        if (member is not null)
        {
            _context.GroupMembers.Remove(member);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteByGroupIdAsync(int groupId)
    {
        var members = await _context.GroupMembers.Where(m => m.GroupId == groupId).ToListAsync();
        _context.GroupMembers.RemoveRange(members);
        await _context.SaveChangesAsync();
    }
}
