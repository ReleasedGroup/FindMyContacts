using Azure.Core;
using Azure.Identity;
using FindMyContacts.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;

namespace FindMyContacts.Services;

/// <summary>
/// Implementation of Microsoft Graph authentication service
/// </summary>
public class GraphAuthenticationService : IGraphAuthenticationService
{
    private readonly GraphConfiguration _config;
    private readonly ILogger<GraphAuthenticationService> _logger;
    private GraphServiceClient? _client;
    private string? _currentUserEmail;

    public GraphAuthenticationService(
        IOptions<GraphConfiguration> config,
        ILogger<GraphAuthenticationService> logger)
    {
        _config = config.Value;
        _logger = logger;
    }

    public async Task<GraphServiceClient> GetAuthenticatedClientAsync(CancellationToken cancellationToken = default)
    {
        if (_client != null)
        {
            return _client;
        }

        _logger.LogInformation("Authenticating with Microsoft Graph...");

        var credential = CreateCredential();
        _client = new GraphServiceClient(credential, _config.Scopes);

        // Verify authentication by getting current user
        await GetCurrentUserEmailAsync(cancellationToken);

        _logger.LogInformation("Successfully authenticated with Microsoft Graph");
        return _client;
    }

    public async Task<string> GetCurrentUserEmailAsync(CancellationToken cancellationToken = default)
    {
        if (_currentUserEmail != null)
        {
            return _currentUserEmail;
        }

        var client = _client ?? await GetAuthenticatedClientAsync(cancellationToken);
        var user = await client.Me.GetAsync(cancellationToken: cancellationToken);

        _currentUserEmail = user?.Mail ?? user?.UserPrincipalName
            ?? throw new InvalidOperationException("Could not determine current user email");

        _logger.LogInformation("Authenticated as: {Email}", _currentUserEmail);
        return _currentUserEmail;
    }

    private TokenCredential CreateCredential()
    {
        var options = new TokenCredentialOptions
        {
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
        };

        // If client secret is provided, use confidential client
        if (!string.IsNullOrWhiteSpace(_config.ClientSecret))
        {
            _logger.LogDebug("Using client credentials authentication");
            return new ClientSecretCredential(
                _config.TenantId,
                _config.ClientId,
                _config.ClientSecret,
                options);
        }

        // Use interactive browser if requested
        if (_config.UseInteractiveBrowser)
        {
            _logger.LogDebug("Using interactive browser authentication");
            return new InteractiveBrowserCredential(new InteractiveBrowserCredentialOptions
            {
                TenantId = _config.TenantId,
                ClientId = _config.ClientId,
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            });
        }

        // Default to device code flow
        _logger.LogDebug("Using device code authentication");
        return new DeviceCodeCredential(new DeviceCodeCredentialOptions
        {
            TenantId = _config.TenantId,
            ClientId = _config.ClientId,
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
            DeviceCodeCallback = context =>
            {
                Console.WriteLine();
                Console.WriteLine(context.Message);
                Console.WriteLine();
                return Task.CompletedTask;
            }
        });
    }
}
