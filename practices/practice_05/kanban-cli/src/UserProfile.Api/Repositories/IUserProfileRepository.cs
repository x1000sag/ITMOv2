using UserProfile.Domain;

namespace UserProfile.Api.Repositories;

public interface IUserProfileRepository
{
    Task<Domain.UserProfile?> GetByIdAsync(int id);
    Task<Domain.UserProfile> CreateAsync(Domain.UserProfile profile);
    Task<Domain.UserProfile> UpdateAsync(Domain.UserProfile profile);
}
