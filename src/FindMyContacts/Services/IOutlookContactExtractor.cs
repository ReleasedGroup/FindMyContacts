using FindMyContacts.Models;

namespace FindMyContacts.Services;

/// <summary>
/// Interface for extracting contacts from Outlook personal contacts
/// </summary>
public interface IOutlookContactExtractor
{
    /// <summary>
    /// Extracts all personal contacts from the user's Outlook contacts folder
    /// </summary>
    IAsyncEnumerable<Contact> ExtractContactsAsync(
        ExtractionOptions options,
        CancellationToken cancellationToken = default);
}
