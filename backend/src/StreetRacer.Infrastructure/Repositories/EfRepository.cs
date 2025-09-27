using Microsoft.EntityFrameworkCore;
using StreetRacer.Application.Common;
using StreetRacer.Domain.Common;
using StreetRacer.Infrastructure.Data;

namespace StreetRacer.Infrastructure.Repositories;

public class EfRepository<T> : IRepository<T> where T : class, IDataModel
{
    protected readonly StreetRacerDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public EfRepository(StreetRacerDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<PagedResult<T>> ListAsync(Cursor cursor)
    {
        var query = _dbSet.AsQueryable();

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

        return new PagedResult<T>
        {
            Items = items,
            NextCursor = nextCursor,
            HasMore = hasMore
        };
    }

    public virtual async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        var entity = await GetAsync(id);
        if (entity != null)
        {
            entity.DeletedAt = DateTime.UtcNow;
            await UpdateAsync(entity);
        }
    }
}