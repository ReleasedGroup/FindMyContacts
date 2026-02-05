namespace FindMyContacts.Models;

/// <summary>
/// Result of the contact extraction process
/// </summary>
public class ExtractionResult
{
    public List<Contact> Contacts { get; set; } = [];
    public ExtractionStatistics Statistics { get; set; } = new();
    public List<string> Errors { get; set; } = [];
    public DateTime ExtractionStartedUtc { get; set; }
    public DateTime ExtractionCompletedUtc { get; set; }

    public TimeSpan Duration => ExtractionCompletedUtc - ExtractionStartedUtc;
}

/// <summary>
/// Statistics about the extraction process
/// </summary>
public class ExtractionStatistics
{
    public int TotalEmailsProcessed { get; set; }
    public int TotalMeetingsProcessed { get; set; }
    public int TotalOutlookContactsProcessed { get; set; }
    public int TotalGalContactsProcessed { get; set; }
    public int TotalContactsFound { get; set; }
    public int UniqueContactsAfterDeduplication { get; set; }
    public int ContactsEnriched { get; set; }
    public int ContactsWithCompany { get; set; }
    public int ContactsWithPhone { get; set; }
    public int ContactsWithJobTitle { get; set; }

    public Dictionary<ContactSource, int> ContactsBySource { get; set; } = [];
}
