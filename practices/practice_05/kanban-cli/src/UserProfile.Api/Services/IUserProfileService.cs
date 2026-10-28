using UserProfile.Domain;

namespace UserProfile.Api.Services;

public interface IUserProfileService
{
    Task<Domain.UserProfile?> GetByIdAsync(int id);
    Task<Domain.UserProfile> CreateAsync(Domain.UserProfile profile);
    Task<Domain.UserProfile> UpdateAsync(int id, Domain.UserProfile profile);
}
