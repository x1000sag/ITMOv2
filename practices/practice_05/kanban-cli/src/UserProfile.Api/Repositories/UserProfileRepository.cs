using Microsoft.EntityFrameworkCore;
using UserProfile.Domain;
using UserProfile.Infrastructure;

namespace UserProfile.Api.Repositories;

public class UserProfileRepository : IUserProfileRepository
{
    private readonly UserProfileDbContext _context;

    public UserProfileRepository(UserProfileDbContext context)
    {
        _context = context;
    }

    public async Task<Domain.UserProfile?> GetByIdAsync(int id)
    {
        return await _context.UserProfiles.FindAsync(id);
    }

    public async Task<Domain.UserProfile> CreateAsync(Domain.UserProfile profile)
    {
        _context.UserProfiles.Add(profile);
        await _context.SaveChangesAsync();
        return profile;
    }

    public async Task<Domain.UserProfile> UpdateAsync(Domain.UserProfile profile)
    {
        _context.Entry(profile).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return profile;
    }
}
