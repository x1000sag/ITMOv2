using Microsoft.EntityFrameworkCore;

namespace UserProfile.Infrastructure;

public class UserProfileDbContext : DbContext
{
    public UserProfileDbContext(DbContextOptions<UserProfileDbContext> options) : base(options)
    {
    }

    public DbSet<Domain.UserProfile> UserProfiles => Set<Domain.UserProfile>();
}
