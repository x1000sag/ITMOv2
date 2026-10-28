using Moq;
using UserProfile.Api.Repositories;
using UserProfile.Api.Services;
using UserProfile.Domain;
using Xunit;

namespace Kanban.Tests;

public class UserProfileServiceTests
{
    [Fact]
    public async Task Create_ReturnsCreatedProfile()
    {
        var mockRepo = new Mock<IUserProfileRepository>();
        var profile = new UserProfile.Domain.UserProfile { Id = 1, Email = "test@example.com" };
        mockRepo.Setup(r => r.CreateAsync(It.IsAny<UserProfile.Domain.UserProfile>())).ReturnsAsync(profile);
        
        var service = new UserProfileService(mockRepo.Object);
        var result = await service.CreateAsync(profile);
        
        Assert.Equal(profile.Email, result.Email);
        mockRepo.Verify(r => r.CreateAsync(It.IsAny<UserProfile.Domain.UserProfile>()), Times.Once);
    }
    
    [Fact]
    public async Task GetById_ReturnsProfile_WhenExists()
    {
        var mockRepo = new Mock<IUserProfileRepository>();
        var profile = new UserProfile.Domain.UserProfile { Id = 1, Email = "test@example.com" };
        mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(profile);
        
        var service = new UserProfileService(mockRepo.Object);
        var result = await service.GetByIdAsync(1);
        
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }
    
    [Fact]
    public async Task Update_SetsIdAndReturnsUpdatedProfile()
    {
        var mockRepo = new Mock<IUserProfileRepository>();
        var profile = new UserProfile.Domain.UserProfile { Email = "updated@example.com" };
        var updated = new UserProfile.Domain.UserProfile { Id = 5, Email = "updated@example.com" };
        mockRepo.Setup(r => r.UpdateAsync(It.IsAny<UserProfile.Domain.UserProfile>())).ReturnsAsync(updated);
        
        var service = new UserProfileService(mockRepo.Object);
        var result = await service.UpdateAsync(5, profile);
        
        Assert.Equal(5, profile.Id);
        Assert.Equal(updated.Email, result.Email);
    }
}
