namespace PantryPal.Api.Services.OpenRouter;

public interface IOpenRouterService
{
    Task<TResponse?> GetStructuredResponseAsync<TResponse>(
        string systemMessage,
        string userMessage,
        object jsonSchema,
        CancellationToken cancellationToken = default);
}
