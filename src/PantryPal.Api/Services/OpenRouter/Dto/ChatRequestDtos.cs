using System.Text.Json.Serialization;

namespace PantryPal.Api.Services.OpenRouter.Dto;

public record ChatRequest(
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("messages")] List<ChatMessage> Messages,
    [property: JsonPropertyName("response_format")] JsonResponseFormat ResponseFormat
);

public record ChatMessage(
    [property: JsonPropertyName("role")] string Role, 
    [property: JsonPropertyName("content")] string Content
);

public record JsonResponseFormat(
    [property: JsonPropertyName("type")] string Type, 
    [property: JsonPropertyName("json_schema")] JsonSchemaObject JsonSchema
);

public record JsonSchemaObject(
    [property: JsonPropertyName("name")] string Name, 
    [property: JsonPropertyName("strict")] bool Strict, 
    [property: JsonPropertyName("schema")] object Schema
);
