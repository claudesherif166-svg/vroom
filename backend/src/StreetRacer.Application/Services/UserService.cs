using Microsoft.Extensions.Logging;
using StreetRacer.Application.Common;
using StreetRacer.Application.DTOs;
using StreetRacer.Application.Services;
using StreetRacer.Domain.Entities;
using StreetRacer.Infrastructure.Repositories;

namespace StreetRacer.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<UserDto?> GetUserAsync(Guid userId)
    {
        var user = await _userRepository.GetAsync(userId);
        if (user == null) return null;

        return MapToUserDto(user);
    }

    public async Task<UserProfileDto?> GetUserProfileAsync(Guid userId, Guid? currentUserId = null)
    {
        var user = await _userRepository.GetAsync(userId);
        if (user == null) return null;

        // TODO: Get follower counts, race counts, achievements, vehicles
        return new UserProfileDto
        {
            Id = user.Id,
            Username = user.Username,
            ProfilePictureUrl = user.ProfilePictureUrl,
            Bio = user.Bio,
            IsPrivate = user.IsPrivate,
            CreatedAt = user.CreatedAt,
            FollowersCount = 0, // TODO: Implement
            FollowingCount = 0, // TODO: Implement
            RacesCount = 0, // TODO: Implement
            IsFollowedByCurrentUser = false, // TODO: Implement
            Vehicles = new List<VehicleDto>(),
            Achievements = new List<UserAchievementDto>()
        };
    }

    public async Task<UserDto> UpdateUserAsync(Guid userId, UpdateUserDto updateUserDto)
    {
        var user = await _userRepository.GetAsync(userId);
        if (user == null)
            throw new ArgumentException("User not found");

        user.Username = updateUserDto.Username;
        user.Bio = updateUserDto.Bio;
        user.IsPrivate = updateUserDto.IsPrivate;

        await _userRepository.UpdateAsync(user);

        return MapToUserDto(user);
    }

    public async Task<PagedResult<UserDto>> GetFollowersAsync(Guid userId, Cursor cursor)
    {
        // TODO: Implement followers query
        return new PagedResult<UserDto>
        {
            Items = new List<UserDto>(),
            NextCursor = null,
            HasMore = false,
            TotalCount = 0
        };
    }

    public async Task<PagedResult<UserDto>> GetFollowingAsync(Guid userId, Cursor cursor)
    {
        // TODO: Implement following query
        return new PagedResult<UserDto>
        {
            Items = new List<UserDto>(),
            NextCursor = null,
            HasMore = false,
            TotalCount = 0
        };
    }

    public async Task<UserDto?> GetUserByUsernameAsync(string username)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        if (user == null) return null;

        return MapToUserDto(user);
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null) return null;

        return MapToUserDto(user);
    }

    private static UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            ProfilePictureUrl = user.ProfilePictureUrl,
            Bio = user.Bio,
            IsPrivate = user.IsPrivate,
            CreatedAt = user.CreatedAt,
            FollowersCount = 0, // TODO: Calculate
            FollowingCount = 0, // TODO: Calculate
            RacesCount = 0, // TODO: Calculate
            IsFollowedByCurrentUser = false // TODO: Calculate
        };
    }
}