using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using UserProfile.Domain;
using Xunit;

namespace Kanban.Tests;

public class ProfilesControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ProfilesControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetNonExistentProfile_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/profiles/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateProfile_ReturnsCreated()
    {
        var profile = new UserProfile.Domain.UserProfile
        {
            Email = "integration@example.com",
            FirstName = "Test",
            LastName = "User"
        };
        
        var response = await _client.PostAsJsonAsync("/profiles", profile);
        Assert.True(response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK);
    }
}
