using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using RockCommitmentTracker.Application.Interfaces;
using RockCommitmentTracker.Application.Models;

namespace RockCommitmentTracker.Infrastructure.ExternalClients;

public class JsonPlaceholderUserProfileClient : IUserProfileClient
{ 
     private readonly HttpClient _httpClient;
    private readonly ILogger<JsonPlaceholderUserProfileClient> _logger;
 
    public JsonPlaceholderUserProfileClient(HttpClient httpClient, ILogger<JsonPlaceholderUserProfileClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
 
    public async Task<UserProfile?> GetUserAsync(string memberId)
    {
        _logger.LogInformation("Fetching profile for member {MemberId}", memberId);
 
        var response = await _httpClient.GetAsync($"/users/{memberId}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<UserProfile>();
    }
}
