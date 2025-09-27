using Microsoft.EntityFrameworkCore;
using StreetRacer.Application.Common;
using StreetRacer.Domain.Entities;
using StreetRacer.Infrastructure.Data;

namespace StreetRacer.Infrastructure.Repositories;

public class EfPostRepository : EfRepository<Post>, IPostRepository
{
    public EfPostRepository(StreetRacerDbContext context) : base(context)
    {
    }

    public async Task<Post?> GetWithMediaAsync(Guid postId)
    {
        return await _context.Posts
            .Include(p => p.Media)
            .FirstOrDefaultAsync(p => p.Id == postId);
    }

    public async Task<PagedResult<Post>> GetByAuthorAsync(Guid authorId, Cursor cursor)
    {
        var query = _context.Posts
            .Where(p => p.AuthorId == authorId);

        return await ApplyCursorPagination(query, cursor);
    }

    public async Task<PagedResult<Post>> GetPublicPostsAsync(Cursor cursor)
    {
        var query = _context.Posts
            .Where(p => p.Visibility == "public");

        return await ApplyCursorPagination(query, cursor);
    }

    public async Task AddMediaAsync(PostMedia media)
    {
        await _context.PostMedia.AddAsync(media);
        await _context.SaveChangesAsync();
    }

    public async Task<PostLike?> GetLikeAsync(Guid postId, Guid userId)
    {
        return await _context.PostLikes
            .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
    }

    public async Task AddLikeAsync(PostLike like)
    {
        await _context.PostLikes.AddAsync(like);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveLikeAsync(Guid likeId)
    {
        var like = await _context.PostLikes.FindAsync(likeId);
        if (like != null)
        {
            _context.PostLikes.Remove(like);
            await _context.SaveChangesAsync();
        }
    }

    private async Task<PagedResult<Post>> ApplyCursorPagination(IQueryable<Post> query, Cursor cursor)
    {
        // Apply cursor filtering
        if (cursor.CreatedAt.HasValue && cursor.Id.HasValue)
        {
            query = query.Where(x => x.CreatedAt < cursor.CreatedAt.Value ||
                                   (x.CreatedAt == cursor.CreatedAt.Value && x.Id.CompareTo(cursor.Id.Value) < 0));
        }

        var items = await query
            .Include(p => p.Media)
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

        return new PagedResult<Post>
        {
            Items = items,
            NextCursor = nextCursor,
            HasMore = hasMore
        };
    }
}