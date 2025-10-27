using PantryPal.Data;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace PantryPal.Mobile.Services;

public class PantryService : IPantryService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public PantryService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        // TODO: Move to configuration
        _baseUrl = DeviceInfo.Platform == DevicePlatform.Android
            ? "https://10.0.2.2:7154" // Android emulator localhost
            : "https://localhost:7154";
    }

    public async Task<PantryItemsPaginatedResponseDto> GetPantryItemsAsync(int page, int pageSize, string sortField = "name")
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/pantry-items?page={page}&pageSize={pageSize}&sortBy={sortField}");
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<PantryItemsPaginatedResponseDto>();
        return result ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    public async Task<PantryItemDto> CreatePantryItemAsync(PantryItemCreateDto item)
    {
        var json = JsonSerializer.Serialize(item);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync($"{_baseUrl}/pantry-items", content);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<PantryItemDto>();
        return result ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    public async Task<PantryItemDto> UpdatePantryItemAsync(string id, PantryItemUpdateDto item)
    {
        var json = JsonSerializer.Serialize(item);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PatchAsync($"{_baseUrl}/pantry-items/{id}", content);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<PantryItemDto>();
        return result ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    public async Task DeletePantryItemAsync(string id)
    {
        var response = await _httpClient.DeleteAsync($"{_baseUrl}/pantry-items/{id}");
        response.EnsureSuccessStatusCode();
    }
}

