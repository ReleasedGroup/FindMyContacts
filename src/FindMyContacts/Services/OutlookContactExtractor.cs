using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models;
using Contact = FindMyContacts.Models.Contact;

namespace FindMyContacts.Services;

/// <summary>
/// Extracts contacts from Outlook personal contacts using Microsoft Graph
/// </summary>
public class OutlookContactExtractor : IOutlookContactExtractor
{
    private readonly IGraphAuthenticationService _authService;
    private readonly ILogger<OutlookContactExtractor> _logger;

    public OutlookContactExtractor(
        IGraphAuthenticationService authService,
        ILogger<OutlookContactExtractor> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public async IAsyncEnumerable<Contact> ExtractContactsAsync(
        Models.ExtractionOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var client = await _authService.GetAuthenticatedClientAsync(cancellationToken);

        _logger.LogInformation("Extracting Outlook personal contacts...");

        int processedCount = 0;
        int maxContacts = options.MaxOutlookContacts > 0 ? options.MaxOutlookContacts : int.MaxValue;

        var contacts = await client.Me.Contacts.GetAsync(requestConfig =>
        {
            requestConfig.QueryParameters.Top = 100;
            requestConfig.QueryParameters.Select =
            [
                "id", "displayName", "givenName", "surname", "emailAddresses",
                "companyName", "jobTitle", "businessPhones", "mobilePhone",
                "department", "officeLocation", "businessHomePage"
            ];
            requestConfig.QueryParameters.Orderby = ["displayName"];
        }, cancellationToken);

        while (contacts?.Value != null && processedCount < maxContacts)
        {
            foreach (var outlookContact in contacts.Value)
            {
                if (processedCount >= maxContacts)
                    break;

                cancellationToken.ThrowIfCancellationRequested();

                // Extract contacts for each email address
                if (outlookContact.EmailAddresses != null)
                {
                    foreach (var emailAddress in outlookContact.EmailAddresses)
                    {
                        if (!string.IsNullOrWhiteSpace(emailAddress.Address))
                        {
                            var contact = CreateContact(outlookContact, emailAddress.Address);
                            if (IsValidContact(contact, options))
                            {
                                yield return contact;
                            }
                        }
                    }
                }

                processedCount++;
            }

            // Get next page
            if (contacts.OdataNextLink != null && processedCount < maxContacts)
            {
                contacts = await client.Me.Contacts
                    .WithUrl(contacts.OdataNextLink)
                    .GetAsync(cancellationToken: cancellationToken);
            }
            else
            {
                break;
            }
        }

        _logger.LogInformation("Processed {Count} Outlook contacts", processedCount);
    }

    private static Contact CreateContact(Microsoft.Graph.Models.Contact outlookContact, string email)
    {
        var contact = new Contact
        {
            Email = email.ToLowerInvariant(),
            DisplayName = outlookContact.DisplayName ?? email,
            FirstName = outlookContact.GivenName,
            LastName = outlookContact.Surname,
            Company = outlookContact.CompanyName,
            JobTitle = outlookContact.JobTitle,
            Department = outlookContact.Department,
            Location = outlookContact.OfficeLocation,
            Website = outlookContact.BusinessHomePage,
            Sources = [Models.ContactSource.OutlookContact],
            // Outlook contacts are pre-enriched, high confidence
            EnrichmentConfidence = 90
        };

        // Extract phone numbers
        if (outlookContact.BusinessPhones?.Count > 0)
        {
            contact.Phone = outlookContact.BusinessPhones[0];
        }

        if (!string.IsNullOrWhiteSpace(outlookContact.MobilePhone))
        {
            contact.MobilePhone = outlookContact.MobilePhone;
        }

        return contact;
    }

    private bool IsValidContact(Contact contact, Models.ExtractionOptions options)
    {
        if (string.IsNullOrWhiteSpace(contact.Email))
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
