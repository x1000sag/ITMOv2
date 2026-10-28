using UserProfile.Api.Repositories;
using UserProfile.Domain;

namespace UserProfile.Api.Services;

public class UserProfileService : IUserProfileService
{
    private readonly IUserProfileRepository _repository;

    public UserProfileService(IUserProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<Domain.UserProfile?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<Domain.UserProfile> CreateAsync(Domain.UserProfile profile)
    {
        return await _repository.CreateAsync(profile);
    }

    public async Task<Domain.UserProfile> UpdateAsync(int id, Domain.UserProfile profile)
    {
        profile.Id = id;
        return await _repository.UpdateAsync(profile);
    }
}
