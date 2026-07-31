using Microsoft.EntityFrameworkCore;
using Sharply.Domain.Interfaces;
using Sharply.Domain.Models;
using Sharply.Infrastructure.Data;

namespace Sharply.Infrastructure.Repositories;

public class MissionCompletionRepository : IMissionCompletionRepository
{
    private readonly AppDbContext _context;

    public MissionCompletionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MissionCompletion>> GetByUserIdAsync(int userId)
    {
        return await _context.MissionCompletions
            .Where(c => c.UserId == userId)
            .ToListAsync();
    }

    public async Task AddAsync(MissionCompletion completion)
    {
        await _context.MissionCompletions.AddAsync(completion);
        await _context.SaveChangesAsync();
    }
}
