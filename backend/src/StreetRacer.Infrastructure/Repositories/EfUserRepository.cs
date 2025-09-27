using Microsoft.EntityFrameworkCore;
using StreetRacer.Application.Common;
using StreetRacer.Domain.Entities;
using StreetRacer.Infrastructure.Data;

namespace StreetRacer.Infrastructure.Repositories;

public class EfUserRepository : EfRepository<User>, IUserRepository
{
    public EfUserRepository(StreetRacerDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetByKeycloakSubjectAsync(string subject)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.KeycloakSubject == subject);
    }

    public async Task<PagedResult<User>> SearchByUsernameAsync(string query, Cursor cursor)
    {
        var queryable = _context.Users
            .Where(u => u.Username.Contains(query));

        // Apply cursor filtering
        if (cursor.CreatedAt.HasValue && cursor.Id.HasValue)
        {
            queryable = queryable.Where(x => x.CreatedAt < cursor.CreatedAt.Value ||
                                           (x.CreatedAt == cursor.CreatedAt.Value && x.Id.CompareTo(cursor.Id.Value) < 0));
        }

        var items = await queryable
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

        return new PagedResult<User>
        {
            Items = items,
            NextCursor = nextCursor,
            HasMore = hasMore
        };
    }
}