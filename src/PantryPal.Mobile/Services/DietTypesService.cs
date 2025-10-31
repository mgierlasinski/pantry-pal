using PantryPal.Data;
using System.Net.Http.Json;

namespace PantryPal.Mobile.Services;

public class DietTypesService : IDietTypesService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public DietTypesService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        // TODO: Move to configuration
        _baseUrl = DeviceInfo.Platform == DevicePlatform.Android
            ? "https://10.0.2.2:7154" // Android emulator localhost
            : "https://localhost:7154";
    }

    public async Task<DietTypesResponseDto> GetDietTypesAsync()
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/diet-types");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<DietTypesResponseDto>();
        return result ?? throw new InvalidOperationException("Failed to deserialize response");
    }
}
