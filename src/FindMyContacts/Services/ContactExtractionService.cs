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
    private readonly IOutlookContactExtractor _outlookExtractor;
    private readonly IGalContactExtractor _galExtractor;
    private readonly IContactAggregator _aggregator;
    private readonly ILogger<ContactExtractionService> _logger;

    public ContactExtractionService(
        IEmailContactExtractor emailExtractor,
        IMeetingContactExtractor meetingExtractor,
        IOutlookContactExtractor outlookExtractor,
        IGalContactExtractor galExtractor,
        IContactAggregator aggregator,
        ILogger<ContactExtractionService> logger)
    {
        _emailExtractor = emailExtractor;
        _meetingExtractor = meetingExtractor;
        _outlookExtractor = outlookExtractor;
        _galExtractor = galExtractor;
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

        // Extract from inbox
        if (options.IncludeInbox)
        {
            await ExtractWithErrorHandling("inbox", result, async () =>
            {
                await foreach (var contact in _emailExtractor.ExtractFromInboxAsync(options, cancellationToken))
                {
                    _aggregator.AddContact(contact);
                    result.Statistics.TotalEmailsProcessed++;
                }
            });
        }

        // Extract from sent items
        if (options.IncludeSentItems)
        {
            await ExtractWithErrorHandling("sent items", result, async () =>
            {
                await foreach (var contact in _emailExtractor.ExtractFromSentItemsAsync(options, cancellationToken))
                {
                    _aggregator.AddContact(contact);
                }
            });
        }

        // Extract from calendar events
        await ExtractWithErrorHandling("calendar events", result, async () =>
        {
            await foreach (var contact in _meetingExtractor.ExtractFromEventsAsync(options, cancellationToken))
            {
                _aggregator.AddContact(contact);
                result.Statistics.TotalMeetingsProcessed++;
            }
        });

        // Extract from Outlook personal contacts
        if (options.IncludeOutlookContacts)
        {
            await ExtractWithErrorHandling("Outlook contacts", result, async () =>
            {
                await foreach (var contact in _outlookExtractor.ExtractContactsAsync(options, cancellationToken))
                {
                    _aggregator.AddContact(contact);
                    result.Statistics.TotalOutlookContactsProcessed++;
                }
            });
        }

        // Extract from Global Address List
        if (options.IncludeGal)
        {
            await ExtractWithErrorHandling("Global Address List", result, async () =>
            {
                await foreach (var contact in _galExtractor.ExtractContactsAsync(options, cancellationToken))
                {
                    _aggregator.AddContact(contact);
                    result.Statistics.TotalGalContactsProcessed++;
                }
            });
        }

        // Always calculate final statistics
        result.Contacts = _aggregator.GetDeduplicatedContacts().ToList();
        result.Statistics.TotalContactsFound = _aggregator.TotalContactsAdded;
        result.Statistics.UniqueContactsAfterDeduplication = _aggregator.UniqueContactCount;
        CalculateStatistics(result);

        result.ExtractionCompletedUtc = DateTime.UtcNow;

        _logger.LogInformation(
            "Extraction complete. Found {Total} contacts, {Unique} unique after deduplication",
            result.Statistics.TotalContactsFound,
            result.Statistics.UniqueContactsAfterDeduplication);

        return result;
    }

    private async Task ExtractWithErrorHandling(string source, ExtractionResult result, Func<Task> extractAction)
    {
        try
        {
            _logger.LogInformation("Extracting contacts from {Source}...", source);
            await extractAction();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting from {Source}", source);
            result.Errors.Add($"Error extracting from {source}: {ex.Message}");
        }
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
