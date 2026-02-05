namespace FindMyContacts.Models;

/// <summary>
/// Configuration options for contact extraction
/// </summary>
public class ExtractionOptions
{
    /// <summary>
    /// Maximum number of emails to process (0 = unlimited)
    /// </summary>
    public int MaxEmails { get; set; } = 1000;

    /// <summary>
    /// Maximum number of calendar events to process (0 = unlimited)
    /// </summary>
    public int MaxEvents { get; set; } = 500;

    /// <summary>
    /// How far back to look for emails
    /// </summary>
    public TimeSpan EmailLookbackPeriod { get; set; } = TimeSpan.FromDays(365);

    /// <summary>
    /// How far back to look for calendar events
    /// </summary>
    public TimeSpan EventLookbackPeriod { get; set; } = TimeSpan.FromDays(365);

    /// <summary>
    /// Include sent items folder
    /// </summary>
    public bool IncludeSentItems { get; set; } = true;

    /// <summary>
    /// Include inbox folder
    /// </summary>
    public bool IncludeInbox { get; set; } = true;

    /// <summary>
    /// Include CC recipients
    /// </summary>
    public bool IncludeCcRecipients { get; set; } = true;

    /// <summary>
    /// Try to extract contact details from email signatures
    /// </summary>
    public bool EnableSignatureExtraction { get; set; } = true;

    /// <summary>
    /// Exclude contacts from specific domains (e.g., noreply domains)
    /// </summary>
    public List<string> ExcludedDomains { get; set; } =
    [
        "noreply",
        "no-reply",
        "donotreply",
        "notifications",
        "mailer-daemon",
        "postmaster"
    ];

    /// <summary>
    /// Output format for the contact list
    /// </summary>
    public OutputFormat OutputFormat { get; set; } = OutputFormat.Json;

    /// <summary>
    /// Output file path (null = stdout)
    /// </summary>
    public string? OutputPath { get; set; }
}

public enum OutputFormat
{
    Json,
    Csv,
    Vcard
}
