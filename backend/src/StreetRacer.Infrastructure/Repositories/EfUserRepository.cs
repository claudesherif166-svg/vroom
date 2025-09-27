using Microsoft.EntityFrameworkCore;
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
}