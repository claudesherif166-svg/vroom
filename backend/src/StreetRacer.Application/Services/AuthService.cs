using Microsoft.Extensions.Logging;
using StreetRacer.Application.DTOs;
using StreetRacer.Application.Services;
using StreetRacer.Domain.Entities;
using StreetRacer.Infrastructure.Repositories;

namespace StreetRacer.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository userRepository, ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<UserDto> SyncUserFromKeycloakAsync(string keycloakSub, string email, string username)
    {
        var existingUser = await _userRepository.GetByKeycloakSubjectAsync(keycloakSub);
        
        if (existingUser != null)
        {
            // Update existing user if needed
            var updated = false;
            if (existingUser.Email != email)
            {
                existingUser.Email = email;
                updated = true;
            }
            
            if (updated)
            {
                await _userRepository.UpdateAsync(existingUser);
            }
            
            return MapToUserDto(existingUser);
        }

        // Create new user
        var user = new User
        {
            Email = email,
            Username = await GenerateUniqueUsernameAsync(username),
            KeycloakSubject = keycloakSub,
            IsPrivate = false
        };

        await _userRepository.AddAsync(user);
        
        _logger.LogInformation("Created new user {UserId} from Keycloak subject {Subject}", user.Id, keycloakSub);

        return MapToUserDto(user);
    }

    public async Task<UserDto?> GetUserByKeycloakSubAsync(string keycloakSub)
    {
        var user = await _userRepository.GetByKeycloakSubjectAsync(keycloakSub);
        if (user == null) return null;

        return MapToUserDto(user);
    }

    private async Task<string> GenerateUniqueUsernameAsync(string baseUsername)
    {
        var username = baseUsername;
        var counter = 1;

        while (await _userRepository.GetByUsernameAsync(username) != null)
        {
            username = $"{baseUsername}{counter}";
            counter++;
        }

        return username;
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
            FollowersCount = 0,
            FollowingCount = 0,
            RacesCount = 0,
            IsFollowedByCurrentUser = false
        };
    }
}