using System.Runtime.CompilerServices;
using FindMyContacts.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models;

namespace FindMyContacts.Services;

/// <summary>
/// Extracts contacts from calendar events using Microsoft Graph
/// </summary>
public class MeetingContactExtractor : IMeetingContactExtractor
{
    private readonly IGraphAuthenticationService _authService;
    private readonly ILogger<MeetingContactExtractor> _logger;

    public MeetingContactExtractor(
        IGraphAuthenticationService authService,
        ILogger<MeetingContactExtractor> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public async IAsyncEnumerable<Contact> ExtractFromEventsAsync(
        ExtractionOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var client = await _authService.GetAuthenticatedClientAsync(cancellationToken);
        var currentUserEmail = await _authService.GetCurrentUserEmailAsync(cancellationToken);
        var cutoffDate = DateTime.UtcNow - options.EventLookbackPeriod;
        var endDate = DateTime.UtcNow.AddMonths(3); // Also look ahead for upcoming meetings

        _logger.LogInformation("Extracting contacts from calendar events between {StartDate} and {EndDate}",
            cutoffDate, endDate);

        int processedCount = 0;
        int maxEvents = options.MaxEvents > 0 ? options.MaxEvents : int.MaxValue;

        var events = await client.Me.CalendarView.GetAsync(requestConfig =>
        {
            requestConfig.QueryParameters.StartDateTime = cutoffDate.ToString("o");
            requestConfig.QueryParameters.EndDateTime = endDate.ToString("o");
            requestConfig.QueryParameters.Top = 50;
            requestConfig.QueryParameters.Select = ["id", "subject", "organizer", "attendees", "start", "end"];
            requestConfig.QueryParameters.Orderby = ["start/dateTime desc"];
        }, cancellationToken);

        while (events?.Value != null && processedCount < maxEvents)
        {
            foreach (var evt in events.Value)
            {
                if (processedCount >= maxEvents)
                    break;

                cancellationToken.ThrowIfCancellationRequested();

                // Extract organizer
                if (evt.Organizer?.EmailAddress != null)
                {
                    var contact = CreateContactFromEmailAddress(evt.Organizer.EmailAddress, ContactSource.MeetingOrganizer);
                    if (IsValidContact(contact, currentUserEmail, options))
                    {
                        yield return contact;
                    }
                }

                // Extract attendees
                if (evt.Attendees != null)
                {
                    foreach (var attendee in evt.Attendees)
                    {
                        if (attendee.EmailAddress != null)
                        {
                            var source = attendee.Type == AttendeeType.Optional
                                ? ContactSource.MeetingOptional
                                : ContactSource.MeetingAttendee;

                            var contact = CreateContactFromEmailAddress(attendee.EmailAddress, source);
                            if (IsValidContact(contact, currentUserEmail, options))
                            {
                                yield return contact;
                            }
                        }
                    }
                }

                processedCount++;
            }

            // Get next page
            if (events.OdataNextLink != null && processedCount < maxEvents)
            {
                events = await client.Me.CalendarView
                    .WithUrl(events.OdataNextLink)
                    .GetAsync(cancellationToken: cancellationToken);
            }
            else
            {
                break;
            }
        }

        _logger.LogInformation("Processed {Count} calendar events", processedCount);
    }

    private Contact CreateContactFromEmailAddress(EmailAddress emailAddress, ContactSource source)
    {
        var contact = new Contact
        {
            Email = emailAddress.Address?.ToLowerInvariant() ?? string.Empty,
            DisplayName = emailAddress.Name ?? emailAddress.Address ?? string.Empty,
            Sources = [source]
        };

        // Try to parse first/last name from display name
        ParseDisplayName(contact);

        return contact;
    }

    private static void ParseDisplayName(Contact contact)
    {
        if (string.IsNullOrWhiteSpace(contact.DisplayName))
            return;

        var name = contact.DisplayName.Trim();

        // Handle "Last, First" format
        if (name.Contains(','))
        {
            var parts = name.Split(',', 2);
            contact.LastName = parts[0].Trim();
            contact.FirstName = parts.Length > 1 ? parts[1].Trim() : null;
        }
        // Handle "First Last" format
        else
        {
            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                contact.FirstName = parts[0];
                contact.LastName = string.Join(' ', parts.Skip(1));
            }
            else if (parts.Length == 1)
            {
                contact.FirstName = parts[0];
            }
        }
    }

    private bool IsValidContact(Contact contact, string currentUserEmail, ExtractionOptions options)
    {
        // Skip empty emails
        if (string.IsNullOrWhiteSpace(contact.Email))
            return false;

        // Skip current user
        if (contact.Email.Equals(currentUserEmail, StringComparison.OrdinalIgnoreCase))
            return false;

        // Skip excluded domains
        var emailDomain = contact.Email.Split('@').LastOrDefault() ?? "";
        var emailLocalPart = contact.Email.Split('@').FirstOrDefault() ?? "";

        foreach (var excluded in options.ExcludedDomains)
        {
            if (emailLocalPart.Contains(excluded, StringComparison.OrdinalIgnoreCase) ||
                emailDomain.Contains(excluded, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogDebug("Skipping excluded email: {Email}", contact.Email);
                return false;
            }
        }

        return true;
    }
}
