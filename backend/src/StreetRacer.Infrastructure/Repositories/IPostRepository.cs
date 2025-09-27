using StreetRacer.Application.Common;
using StreetRacer.Domain.Entities;

namespace StreetRacer.Infrastructure.Repositories;

public interface IPostRepository : IRepository<Post>
{
    Task<Post?> GetWithMediaAsync(Guid postId);
    Task<PagedResult<Post>> GetByAuthorAsync(Guid authorId, Cursor cursor);
    Task<PagedResult<Post>> GetPublicPostsAsync(Cursor cursor);
    Task AddMediaAsync(PostMedia media);
    Task<PostLike?> GetLikeAsync(Guid postId, Guid userId);
    Task AddLikeAsync(PostLike like);
    Task RemoveLikeAsync(Guid likeId);
}