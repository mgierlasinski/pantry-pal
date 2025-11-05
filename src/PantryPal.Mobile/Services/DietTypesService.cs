using PantryPal.Data;
using System.Net.Http.Json;

namespace PantryPal.Mobile.Services;

public class DietTypesService : IDietTypesService
{
    private readonly HttpClient _httpClient;

    public DietTypesService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<DietTypesResponseDto> GetDietTypesAsync()
    {
        var response = await _httpClient.GetAsync("diet-types");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<DietTypesResponseDto>();
        return result ?? throw new InvalidOperationException("Failed to deserialize response");
    }
}
