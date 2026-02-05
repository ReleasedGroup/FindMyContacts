using Microsoft.Graph;

namespace FindMyContacts.Services;

/// <summary>
/// Service for authenticating with Microsoft Graph
/// </summary>
public interface IGraphAuthenticationService
{
    /// <summary>
    /// Gets an authenticated Graph service client
    /// </summary>
    Task<GraphServiceClient> GetAuthenticatedClientAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current user's email address
    /// </summary>
    Task<string> GetCurrentUserEmailAsync(CancellationToken cancellationToken = default);
}
