namespace FindMyContacts.Configuration;

/// <summary>
/// Configuration for Microsoft Graph API authentication
/// </summary>
public class GraphConfiguration
{
    public const string SectionName = "Graph";

    /// <summary>
    /// Azure AD tenant ID
    /// </summary>
    public string TenantId { get; set; } = "common";

    /// <summary>
    /// Azure AD application (client) ID
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Client secret for confidential client apps (optional, use device code flow if not provided)
    /// </summary>
    public string? ClientSecret { get; set; }

    /// <summary>
    /// Scopes required for the application
    /// </summary>
    public string[] Scopes { get; set; } =
    [
        "User.Read",
        "Mail.Read",
        "Calendars.Read",
        "Contacts.Read",
        "People.Read",
        "User.ReadBasic.All"
    ];

    /// <summary>
    /// Use device code flow for authentication (interactive)
    /// </summary>
    public bool UseDeviceCodeFlow { get; set; } = true;

    /// <summary>
    /// Use interactive browser authentication
    /// </summary>
    public bool UseInteractiveBrowser { get; set; } = false;
}
