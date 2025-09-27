using Microsoft.EntityFrameworkCore;
using StreetRacer.Application.Common;
using StreetRacer.Domain.Entities;
using StreetRacer.Infrastructure.Data;

namespace StreetRacer.Infrastructure.Repositories;

public class EfVehicleRepository : EfRepository<Vehicle>, IVehicleRepository
{
    public EfVehicleRepository(StreetRacerDbContext context) : base(context)
    {
    }

    public async Task<PagedResult<Vehicle>> GetByOwnerAsync(Guid? ownerId, Cursor cursor)
    {
        var query = _dbSet.AsQueryable();

        if (ownerId.HasValue)
        {
            query = query.Where(v => v.OwnerId == ownerId.Value);
        }

        // Apply cursor filtering
        if (cursor.CreatedAt.HasValue && cursor.Id.HasValue)
        {
            query = query.Where(x => x.CreatedAt < cursor.CreatedAt.Value ||
                                   (x.CreatedAt == cursor.CreatedAt.Value && x.Id.CompareTo(cursor.Id.Value) < 0));
        }

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Take(cursor.Limit + 1)
            .ToListAsync();

        var hasMore = items.Count > cursor.Limit;
        if (hasMore)
        {
            items.RemoveAt(items.Count - 1);
        }

        var nextCursor = hasMore && items.Any()
            ? new Cursor { CreatedAt = items.Last().CreatedAt, Id = items.Last().Id, Limit = cursor.Limit }.ToBase64()
            : null;

        return new PagedResult<Vehicle>
        {
            Items = items,
            NextCursor = nextCursor,
            HasMore = hasMore
        };
    }
}