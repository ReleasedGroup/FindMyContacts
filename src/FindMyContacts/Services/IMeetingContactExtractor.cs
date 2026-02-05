using FindMyContacts.Models;

namespace FindMyContacts.Services;

/// <summary>
/// Service for extracting contacts from calendar events/meetings
/// </summary>
public interface IMeetingContactExtractor
{
    /// <summary>
    /// Extracts contacts from calendar events
    /// </summary>
    IAsyncEnumerable<Contact> ExtractFromEventsAsync(
        ExtractionOptions options,
        CancellationToken cancellationToken = default);
}
