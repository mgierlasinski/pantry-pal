using PantryPal.Data;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace PantryPal.Mobile.Services;

public class RecipeService : IRecipeService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public RecipeService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        // TODO: Move to configuration
        _baseUrl = DeviceInfo.Platform == DevicePlatform.Android
            ? "https://10.0.2.2:7154" // Android emulator localhost
            : "https://localhost:7154";
    }

    public async Task<List<RecipeRejectReasonDto>> GetRejectReasonsAsync()
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/recipe-reject-reasons");
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<RecipeRejectReasonsResponseDto>();
        return result?.RejectReasons.ToList() ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    public async Task<RecipeGenerateResponseDto> GenerateRecipeAsync()
    {
        var response = await _httpClient.PostAsync($"{_baseUrl}/recipes/generate", null);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<RecipeGenerateResponseDto>();
        return result ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    public async Task<RecipeAcceptResponseDto> AcceptRecipeAsync(string generationId)
    {
        var response = await _httpClient.PostAsync($"{_baseUrl}/recipes/{generationId}/accept", null);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<RecipeAcceptResponseDto>();
        return result ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    public async Task RejectRecipeAsync(string generationId, RecipeRejectRequestDto payload)
    {
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_baseUrl}/recipes/{generationId}/reject", content);
        response.EnsureSuccessStatusCode();
    }

    public async Task<RecipesPaginatedResponseDto> GetRecipesAsync(int page, int pageSize)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/recipes?page={page}&pageSize={pageSize}");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<RecipesPaginatedResponseDto>();
        return result ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    public async Task DeleteRecipeAsync(string id)
    {
        var response = await _httpClient.DeleteAsync($"{_baseUrl}/recipes/{id}");
        response.EnsureSuccessStatusCode();
    }
}

