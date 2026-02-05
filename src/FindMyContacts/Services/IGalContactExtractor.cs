using FindMyContacts.Models;

namespace FindMyContacts.Services;

/// <summary>
/// Interface for extracting contacts from the Global Address List (GAL)
/// </summary>
public interface IGalContactExtractor
{
    /// <summary>
    /// Extracts contacts from the organization's Global Address List
    /// </summary>
    IAsyncEnumerable<Contact> ExtractContactsAsync(
        ExtractionOptions options,
        CancellationToken cancellationToken = default);
}
