using PantryPal.Data;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace PantryPal.Mobile.Services;

public class UserPreferencesService : IUserPreferencesService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public UserPreferencesService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        // TODO: Move to configuration
        _baseUrl = DeviceInfo.Platform == DevicePlatform.Android
            ? "https://10.0.2.2:7154" // Android emulator localhost
            : "https://localhost:7154";
    }

    public async Task<UserPreferencesDto?> GetUserPreferencesAsync()
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/user-preferences");

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

        var response = await _httpClient.PostAsync($"{_baseUrl}/user-preferences", content);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<UserPreferencesDto>();
        return result ?? throw new InvalidOperationException("Failed to deserialize response");
    }
}
