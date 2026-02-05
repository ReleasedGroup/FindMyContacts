namespace FindMyContacts.Models;

/// <summary>
/// Represents a contact extracted from Office 365
/// </summary>
public class Contact : IEquatable<Contact>
{
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Company { get; set; }
    public string? JobTitle { get; set; }
    public string? Phone { get; set; }
    public string? MobilePhone { get; set; }
    public string? Department { get; set; }
    public string? Location { get; set; }
    public string? Website { get; set; }
    public string? LinkedInUrl { get; set; }

    public HashSet<ContactSource> Sources { get; set; } = [];
    public DateTime FirstSeenUtc { get; set; } = DateTime.UtcNow;
    public DateTime LastSeenUtc { get; set; } = DateTime.UtcNow;
    public int InteractionCount { get; set; } = 1;

    /// <summary>
    /// Confidence score for enriched data (0-100)
    /// </summary>
    public int EnrichmentConfidence { get; set; } = 0;

    /// <summary>
    /// Raw signature text used for enrichment
    /// </summary>
    public string? RawSignature { get; set; }

    public bool Equals(Contact? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return string.Equals(Email, other.Email, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj) => Equals(obj as Contact);

    public override int GetHashCode() =>
        StringComparer.OrdinalIgnoreCase.GetHashCode(Email);

    public override string ToString() =>
        $"{DisplayName} <{Email}>" + (Company != null ? $" - {Company}" : "");
}

/// <summary>
/// Indicates where a contact was discovered
/// </summary>
[Flags]
public enum ContactSource
{
    None = 0,
    EmailFrom = 1,
    EmailTo = 2,
    EmailCc = 4,
    MeetingOrganizer = 8,
    MeetingAttendee = 16,
    MeetingOptional = 32,
    OutlookContact = 64,
    GlobalAddressList = 128
}
