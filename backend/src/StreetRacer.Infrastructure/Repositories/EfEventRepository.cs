using Microsoft.EntityFrameworkCore;
using StreetRacer.Domain.Entities;
using StreetRacer.Infrastructure.Data;

namespace StreetRacer.Infrastructure.Repositories;

public class EfEventRepository : EfRepository<Event>, IEventRepository
{
    public EfEventRepository(StreetRacerDbContext context) : base(context)
    {
    }

    public async Task<Event?> GetWithContributorsAsync(Guid eventId)
    {
        return await _context.Events
            .Include(e => e.Contributors)
            .FirstOrDefaultAsync(e => e.Id == eventId);
    }

    public async Task AddContributorAsync(EventContributor contributor)
    {
        await _context.EventContributors.AddAsync(contributor);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveContributorAsync(Guid contributorId)
    {
        var contributor = await _context.EventContributors.FindAsync(contributorId);
        if (contributor != null)
        {
            _context.EventContributors.Remove(contributor);
            await _context.SaveChangesAsync();
        }
    }
}