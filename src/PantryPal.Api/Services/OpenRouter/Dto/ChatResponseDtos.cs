using System.Text.Json.Serialization;

namespace PantryPal.Api.Services.OpenRouter.Dto;

public record ChatResponse([property: JsonPropertyName("choices")] List<Choice> Choices);

public record Choice([property: JsonPropertyName("message")] ChatMessage Message);
