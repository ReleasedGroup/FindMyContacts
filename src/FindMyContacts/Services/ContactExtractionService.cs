using FindMyContacts.Models;
using Microsoft.Extensions.Logging;

namespace FindMyContacts.Services;

/// <summary>
/// Orchestrates the contact extraction process
/// </summary>
public class ContactExtractionService : IContactExtractionService
{
    private readonly IEmailContactExtractor _emailExtractor;
    private readonly IMeetingContactExtractor _meetingExtractor;
    private readonly IContactAggregator _aggregator;
    private readonly ILogger<ContactExtractionService> _logger;

    public ContactExtractionService(
        IEmailContactExtractor emailExtractor,
        IMeetingContactExtractor meetingExtractor,
        IContactAggregator aggregator,
        ILogger<ContactExtractionService> logger)
    {
        _emailExtractor = emailExtractor;
        _meetingExtractor = meetingExtractor;
        _aggregator = aggregator;
        _logger = logger;
    }

    public async Task<ExtractionResult> ExtractContactsAsync(ExtractionOptions options, CancellationToken cancellationToken = default)
    {
        var result = new ExtractionResult
        {
            ExtractionStartedUtc = DateTime.UtcNow
        };

        _logger.LogInformation("Starting contact extraction...");

        try
        {
            // Extract from inbox
            if (options.IncludeInbox)
            {
                _logger.LogInformation("Extracting contacts from inbox...");
                await foreach (var contact in _emailExtractor.ExtractFromInboxAsync(options, cancellationToken))
                {
                    _aggregator.AddContact(contact);
                    result.Statistics.TotalEmailsProcessed++;
                }
            }

            // Extract from sent items
            if (options.IncludeSentItems)
            {
                _logger.LogInformation("Extracting contacts from sent items...");
                await foreach (var contact in _emailExtractor.ExtractFromSentItemsAsync(options, cancellationToken))
                {
                    _aggregator.AddContact(contact);
                }
            }

            // Extract from calendar events
            _logger.LogInformation("Extracting contacts from calendar events...");
            await foreach (var contact in _meetingExtractor.ExtractFromEventsAsync(options, cancellationToken))
            {
                _aggregator.AddContact(contact);
                result.Statistics.TotalMeetingsProcessed++;
            }

            // Get deduplicated contacts
            result.Contacts = _aggregator.GetDeduplicatedContacts().ToList();
            result.Statistics.TotalContactsFound = _aggregator.TotalContactsAdded;
            result.Statistics.UniqueContactsAfterDeduplication = _aggregator.UniqueContactCount;

            // Calculate enrichment statistics
            CalculateStatistics(result);

            _logger.LogInformation(
                "Extraction complete. Found {Total} contacts, {Unique} unique after deduplication",
                result.Statistics.TotalContactsFound,
                result.Statistics.UniqueContactsAfterDeduplication);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during contact extraction");
            result.Errors.Add(ex.Message);
        }
        finally
        {
            result.ExtractionCompletedUtc = DateTime.UtcNow;
        }

        return result;
    }

    private static void CalculateStatistics(ExtractionResult result)
    {
        foreach (var contact in result.Contacts)
        {
            if (!string.IsNullOrWhiteSpace(contact.Company))
                result.Statistics.ContactsWithCompany++;

            if (!string.IsNullOrWhiteSpace(contact.Phone) || !string.IsNullOrWhiteSpace(contact.MobilePhone))
                result.Statistics.ContactsWithPhone++;

            if (!string.IsNullOrWhiteSpace(contact.JobTitle))
                result.Statistics.ContactsWithJobTitle++;

            if (contact.EnrichmentConfidence > 0)
                result.Statistics.ContactsEnriched++;

            // Count by source
            foreach (var source in contact.Sources)
            {
                if (!result.Statistics.ContactsBySource.TryAdd(source, 1))
                {
                    result.Statistics.ContactsBySource[source]++;
                }
            }
        }
    }
}
