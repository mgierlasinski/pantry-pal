using System.Net.Http.Headers;

namespace PantryPal.Mobile.Services;

public class AuthDelegatingHandler : DelegatingHandler
{
    private readonly IAuthService _authService;

    public AuthDelegatingHandler(IAuthService authService)
    {
        _authService = authService;

        // Configure HttpClientHandler with SSL certificate validation for development
        var httpClientHandler = new HttpClientHandler();
#if DEBUG
        httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
        {
            if (cert is { Issuer: "CN=localhost" })
            {
                return true;
            }
            return errors == System.Net.Security.SslPolicyErrors.None;
        };
#endif
        InnerHandler = httpClientHandler;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var accessToken = await _authService.GetAccessTokenAsync();

        if (!string.IsNullOrEmpty(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
