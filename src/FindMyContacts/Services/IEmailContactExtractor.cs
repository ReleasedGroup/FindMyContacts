using FindMyContacts.Models;

namespace FindMyContacts.Services;

/// <summary>
/// Service for extracting contacts from emails
/// </summary>
public interface IEmailContactExtractor
{
    /// <summary>
    /// Extracts contacts from the user's inbox
    /// </summary>
    IAsyncEnumerable<Contact> ExtractFromInboxAsync(
        ExtractionOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts contacts from the user's sent items
    /// </summary>
    IAsyncEnumerable<Contact> ExtractFromSentItemsAsync(
        ExtractionOptions options,
        CancellationToken cancellationToken = default);
}
