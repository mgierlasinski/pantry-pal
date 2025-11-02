This document provides a comprehensive implementation plan for creating an `OpenRouterService` in the PantryPal project. This service will be responsible for all interactions with the OpenRouter.ai API to generate LLM-based content, such as recipes.

## 1. Service Description

The `OpenRouterService` will act as a centralized client for the OpenRouter API. It will encapsulate the logic for building requests, sending them via HTTP, parsing responses, and handling errors. The primary goal is to provide a simple, reliable, and strongly-typed interface for the rest of the application to leverage LLM capabilities, specifically for generating structured JSON data like recipes. It will be designed for dependency injection and configured through the standard .NET `IConfiguration` system.

## 2. Constructor Description

The service will use constructor injection to receive its dependencies, following best practices for modern .NET applications.

```csharp
public class OpenRouterService : IOpenRouterService
{
    private readonly HttpClient _httpClient;
    private readonly OpenRouterOptions _options;
    private readonly ILogger<OpenRouterService> _logger;
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public OpenRouterService(
        IHttpClientFactory httpClientFactory,
        IOptions<OpenRouterOptions> options,
        ILogger<OpenRouterService> logger)
    {
        // ... implementation
    }
}
```

-   `IHttpClientFactory`: Used to create and manage the lifecycle of `HttpClient` instances. The client will be configured with the OpenRouter API base address and default headers (Authorization, Referer, etc.).
-   `IOptions<OpenRouterOptions>`: Provides strongly-typed access to configuration settings from `appsettings.json` or other configuration sources. This will hold the API key, default model, and other necessary settings.
-   `ILogger<OpenRouterService>`: Used for structured logging of requests, responses, and errors.

## 3. Public Methods and Fields

The service will expose a single, generic public method to handle all chat completion requests that require a structured JSON response.

### `Task<TResponse?> GetStructuredResponseAsync<TResponse>(string systemMessage, string userMessage, object jsonSchema, CancellationToken cancellationToken = default)`

-   **Description:** Asynchronously sends a request to the OpenRouter Chat Completions API and deserializes the JSON response into a specified type `TResponse`.
-   **Generic Type Parameter `TResponse`:** The C# type into which the model's structured JSON response should be deserialized.
-   **Parameters:**
    -   `string systemMessage`: The system prompt that provides context and instructions to the model.
    -   `string userMessage`: The user's prompt or query.
    -   `object jsonSchema`: An anonymous or strongly-typed object representing the JSON schema that the model's output must conform to.
    -   `CancellationToken cancellationToken`: A token to support request cancellation.
-   **Returns:** A `Task` that resolves to an instance of `TResponse` on success, or `null` if the operation fails, cannot be completed, or the response is empty.

## 4. Private Methods and Fields

-   `_httpClient`: A static `HttpClient` instance provided by the `IHttpClientFactory`.
-   `_options`: Stores the injected `OpenRouterOptions`.
-   `_logger`: Stores the injected `ILogger` instance.
-   `_jsonSerializerOptions`: A static `JsonSerializerOptions` field configured for `camelCase` to be reused for all serialization and deserialization operations.
-   A private method for building the request payload (`BuildRequestPayload`).
-   A private method for parsing the API response (`ParseApiResponseAsync`).

## 5. Error Handling

Error handling will be implemented at multiple levels to ensure robustness and provide clear diagnostics.

1.  **Configuration Errors:** If essential settings (like the API Key) are missing, the service's constructor will throw an `ArgumentNullException` on startup, causing the application to fail fast.
2.  **HTTP and Network Errors:**
    -   Requests will be wrapped in a `try-catch` block to handle `HttpRequestException` for network failures or non-success status codes.
    -   A retry policy (e.g., using Polly) will be configured on the `HttpClient` to automatically handle transient network errors and rate limiting (HTTP 429) with exponential backoff.
3.  **API Errors:** If the OpenRouter API returns an error payload (e.g., for invalid requests), the service will deserialize the error object, log the details (`Code`, `Message`), and return `null` or throw a custom exception.
4.  **JSON Deserialization Errors:**
    -   If the API response body cannot be parsed into the C# response model, a `JsonException` will be caught.
    -   If the model's content (the nested JSON string) cannot be parsed into the target type `TResponse`, the exception will be caught.
    -   In both cases, the invalid JSON and the error message will be logged, and the method will return `null`.

## 6. Security Considerations

1.  **API Key Management:** The OpenRouter API key is a sensitive secret. It will be stored using the .NET Secret Manager during local development and as an environment variable or in a secure vault (like Azure Key Vault) in production. It will never be hardcoded or checked into source control.
2.  **Input Validation:** The service will not be directly responsible for sanitizing business logic input (e.g., user-provided ingredients). This should be handled by the calling service (e.g., an API endpoint using FluentValidation) before the data is passed to `OpenRouterService`. This prevents prompt injection and the submission of invalid data.
3.  **HTTPS:** All communication with the OpenRouter API will be enforced over HTTPS to protect data in transit. This is the default for `HttpClient`.

## 7. Step-by-Step Implementation Plan

### Step 1: Configuration and DI Setup

1.  **Define `OpenRouterOptions`:** Create a class in `PantryPal.Api` to hold configuration settings.

    ```csharp
    // src/PantryPal.Api/Services/OpenRouter/OpenRouterOptions.cs
    public class OpenRouterOptions
    {
        public const string SectionName = "OpenRouter";
        public required string ApiKey { get; set; }
        public required string BaseUrl { get; set; }
        public required string SiteName { get; set; }
        public required string DefaultModel { get; set; }
    }
    ```

2.  **Update `appsettings.json`:** Add the `OpenRouter` configuration section. Use Secret Manager for the `ApiKey`.

    ```json
    // src/PantryPal.Api/appsettings.Development.json
    "OpenRouter": {
      "ApiKey": "YOUR_API_KEY_FROM_SECRET_MANAGER",
      "BaseUrl": "https://openrouter.ai/api/v1",
      "SiteName": "http://localhost:5078", // Your app's URL
      "DefaultModel": "anthropic/claude-3.5-sonnet"
    }
    ```

3.  **Register Services in `Program.cs`:** Configure the options and register the `HttpClient` and the service itself.

    ```csharp
    // src/PantryPal.Api/Program.cs
    builder.Services.Configure<OpenRouterOptions>(
        builder.Configuration.GetSection(OpenRouterOptions.SectionName));

    builder.Services.AddHttpClient<IOpenRouterService, OpenRouterService>((serviceProvider, client) =>
    {
        var options = serviceProvider.GetRequiredService<IOptions<OpenRouterOptions>>().Value;
        client.BaseAddress = new Uri(options.BaseUrl);
        client.DefaultRequestHeaders.Authorization = new("Bearer", options.ApiKey);
        client.DefaultRequestHeaders.Add("HTTP-Referer", options.SiteName);
        client.DefaultRequestHeaders.Add("X-Title", "PantryPal");
    })
    .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

    builder.Services.AddScoped<IOpenRouterService, OpenRouterService>();
    ```

### Step 2: Define DTOs for API Communication

1.  Create C# records in `PantryPal.Api/Services/OpenRouter/Dto` to model the request and response payloads. Use `JsonPropertyName` attributes to map to the API's JSON structure.

    ```csharp
    // Request DTOs
    public record ChatRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("messages")] List<ChatMessage> Messages,
        [property: JsonPropertyName("response_format")] JsonResponseFormat ResponseFormat
    );
    public record ChatMessage([property: JsonPropertyName("role")] string Role, [property: JsonPropertyName("content")] string Content);
    public record JsonResponseFormat([property: JsonPropertyName("type")] string Type, [property: JsonPropertyName("json_schema")] JsonSchemaObject JsonSchema);
    public record JsonSchemaObject([property: JsonPropertyName("name")] string Name, [property: JsonPropertyName("strict")] bool Strict, [property: JsonPropertyName("schema")] object Schema);

    // Response DTOs
    public record ChatResponse([property: JsonPropertyName("choices")] List<Choice> Choices);
    public record Choice([property: JsonPropertyName("message")] ChatMessage Message);
    ```

### Step 3: Implement the `OpenRouterService`

1.  **Create the Interface:**

    ```csharp
    // src/PantryPal.Api/Services/OpenRouter/IOpenRouterService.cs
    public interface IOpenRouterService
    {
        Task<TResponse?> GetStructuredResponseAsync<TResponse>(
            string systemMessage,
            string userMessage,
            object jsonSchema,
            CancellationToken cancellationToken = default);
    }
    ```

2.  **Implement the Class:**
    -   Add the constructor as described in Section 2.
    -   Implement the `GetStructuredResponseAsync` method. This method will orchestrate the entire process: build the request payload, serialize it, send the HTTP POST request, read the response, and deserialize it twice (once for the API shell, once for the nested content).
    -   Use `_logger` to log key steps and any errors that occur.

### Step 4: Example for Defining JSON Schema and Calling the Service

This demonstrates how a calling service would use the `OpenRouterService`.

1.  **Define the target schema:**

    ```csharp
    var recipeSchema = new
    {
        type = "object",
        properties = new
        {
            recipeName = new { type = "string", description = "The name of the recipe." },
            description = new { type = "string", description = "A brief, enticing description of the dish." },
            prepTimeMinutes = new { type = "integer", description = "Estimated preparation time in minutes." },
            cookTimeMinutes = new { type = "integer", description = "Estimated cooking time in minutes." },
            servings = new { type = "integer", description = "Number of servings the recipe makes." },
            ingredients = new
            {
                type = "array",
                items = new { type = "string" },
                description = "A list of ingredients with quantities."
            },
            instructions = new
            {
                type = "array",
                items = new { type = "string" },
                description = "Step-by-step cooking instructions."
            }
        },
        required = new[] { "recipeName", "description", "prepTimeMinutes", "cookTimeMinutes", "servings", "ingredients", "instructions" }
    };
    ```

2.  **Define the system and user messages:**

    ```csharp
    string systemMessage = "You are a master chef. Your task is to create a delicious recipe based on the ingredients provided by the user. You must respond only with a JSON object that strictly adheres to the provided schema. Do not include any text outside of the JSON object.";

    string userMessage = "Please generate a recipe using the following ingredients: 500g chicken breast, 1 cup of white rice, 2 cups of broccoli florets, and 1/4 cup of soy sauce.";
    ```

3.  **Call the service:**

    ```csharp
    // Assume a `RecipeDto` class exists that matches the schema structure.
    var recipe = await _openRouterService.GetStructuredResponseAsync<RecipeDto>(
        systemMessage,
        userMessage,
        recipeSchema
    );

    if (recipe is not null)
    {
        // Use the generated recipe object
    }
    ```
