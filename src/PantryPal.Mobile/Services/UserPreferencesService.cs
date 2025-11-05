using PantryPal.Data;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace PantryPal.Mobile.Services;

public class UserPreferencesService : IUserPreferencesService
{
    private readonly HttpClient _httpClient;

    public UserPreferencesService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<UserPreferencesDto?> GetUserPreferencesAsync()
    {
        var response = await _httpClient.GetAsync("user-preferences");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null; // User hasn't set preferences yet
        }

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<UserPreferencesDto>();
        return result ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    public async Task<UserPreferencesDto> UpsertUserPreferencesAsync(UserPreferencesCreateDto preferences)
    {
        var json = JsonSerializer.Serialize(preferences);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("user-preferences", content);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<UserPreferencesDto>();
        return result ?? throw new InvalidOperationException("Failed to deserialize response");
    }
}
