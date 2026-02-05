using FindMyContacts.Models;

namespace FindMyContacts.Services;

/// <summary>
/// Main service for orchestrating contact extraction
/// </summary>
public interface IContactExtractionService
{
    /// <summary>
    /// Extracts contacts from Office 365 mailbox and calendar
    /// </summary>
    Task<ExtractionResult> ExtractContactsAsync(ExtractionOptions options, CancellationToken cancellationToken = default);
}
