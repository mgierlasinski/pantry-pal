using PantryPal.Data;
using System.Net.Http.Json;

namespace PantryPal.Mobile.Services;

public class PreferredCuisinesService : IPreferredCuisinesService
{
    private readonly HttpClient _httpClient;

    public PreferredCuisinesService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PreferredCuisinesResponseDto> GetPreferredCuisinesAsync()
    {
        var response = await _httpClient.GetAsync("preferred-cuisines");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PreferredCuisinesResponseDto>();
        return result ?? throw new InvalidOperationException("Failed to deserialize response");
    }
}
