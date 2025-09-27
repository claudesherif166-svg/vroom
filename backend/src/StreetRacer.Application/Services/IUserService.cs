using StreetRacer.Application.Common;
using StreetRacer.Application.DTOs;

namespace StreetRacer.Application.Services;

public interface IUserService
{
    Task<UserDto?> GetUserAsync(Guid userId);
    Task<UserProfileDto?> GetUserProfileAsync(Guid userId, Guid? currentUserId = null);
    Task<UserDto> UpdateUserAsync(Guid userId, UpdateUserDto updateUserDto);
    Task<PagedResult<UserDto>> GetFollowersAsync(Guid userId, Cursor cursor);
    Task<PagedResult<UserDto>> GetFollowingAsync(Guid userId, Cursor cursor);
    Task<UserDto?> GetUserByUsernameAsync(string username);
    Task<UserDto?> GetUserByEmailAsync(string email);
}

public interface IPostService
{
    Task<PostDto> CreatePostAsync(Guid authorId, CreatePostDto createPostDto);
    Task<PostDto?> GetPostAsync(Guid postId, Guid? currentUserId = null);
    Task DeletePostAsync(Guid postId, Guid userId);
    Task LikePostAsync(Guid postId, Guid userId);
    Task UnlikePostAsync(Guid postId, Guid userId);
    Task<PagedResult<PostDto>> GetUserPostsAsync(Guid userId, Guid? currentUserId, Cursor cursor);
}

public interface IFeedService
{
    Task<PagedResult<PostDto>> GetPersonalizedFeedAsync(Guid userId, Cursor cursor);
    Task<PagedResult<PostDto>> GetPublicFeedAsync(Cursor cursor);
}

public interface IAuthService
{
    Task<UserDto> SyncUserFromKeycloakAsync(string keycloakSub, string email, string username);
    Task<UserDto?> GetUserByKeycloakSubAsync(string keycloakSub);
}