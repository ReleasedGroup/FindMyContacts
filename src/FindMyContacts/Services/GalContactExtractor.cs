using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models;
using Contact = FindMyContacts.Models.Contact;

namespace FindMyContacts.Services;

/// <summary>
/// Extracts contacts from the Global Address List (GAL) using Microsoft Graph
/// </summary>
public class GalContactExtractor : IGalContactExtractor
{
    private readonly IGraphAuthenticationService _authService;
    private readonly ILogger<GalContactExtractor> _logger;

    public GalContactExtractor(
        IGraphAuthenticationService authService,
        ILogger<GalContactExtractor> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public async IAsyncEnumerable<Contact> ExtractContactsAsync(
        Models.ExtractionOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var client = await _authService.GetAuthenticatedClientAsync(cancellationToken);
        var currentUserEmail = await _authService.GetCurrentUserEmailAsync(cancellationToken);

        _logger.LogInformation("Extracting contacts from Global Address List...");

        int processedCount = 0;
        int maxContacts = options.MaxGalContacts > 0 ? options.MaxGalContacts : int.MaxValue;

        // Use the People API which provides access to directory users
        // This gives us the GAL contacts relevant to the user
        var people = await client.Me.People.GetAsync(requestConfig =>
        {
            requestConfig.QueryParameters.Top = 100;
            requestConfig.QueryParameters.Select =
            [
                "id", "displayName", "givenName", "surname", "scoredEmailAddresses",
                "companyName", "jobTitle", "department", "officeLocation",
                "phones", "userPrincipalName"
            ];
        }, cancellationToken);

        while (people?.Value != null && processedCount < maxContacts)
        {
            foreach (var person in people.Value)
            {
                if (processedCount >= maxContacts)
                    break;

                cancellationToken.ThrowIfCancellationRequested();

                // Get email from scoredEmailAddresses or userPrincipalName
                var email = GetPrimaryEmail(person);
                if (string.IsNullOrWhiteSpace(email))
                    continue;

                // Skip current user
                if (email.Equals(currentUserEmail, StringComparison.OrdinalIgnoreCase))
                    continue;

                var contact = CreateContact(person, email);
                if (IsValidContact(contact, options))
                {
                    yield return contact;
                    processedCount++;
                }
            }

            // Get next page
            if (people.OdataNextLink != null && processedCount < maxContacts)
            {
                people = await client.Me.People
                    .WithUrl(people.OdataNextLink)
                    .GetAsync(cancellationToken: cancellationToken);
            }
            else
            {
                break;
            }
        }

        // Also try to get users from directory if we have permissions
        if (options.IncludeDirectoryUsers && processedCount < maxContacts)
        {
            var directoryContacts = await ExtractDirectoryUsersAsync(options, currentUserEmail, maxContacts - processedCount, cancellationToken);
            foreach (var contact in directoryContacts)
            {
                yield return contact;
                processedCount++;
            }
        }

        _logger.LogInformation("Processed {Count} GAL contacts", processedCount);
    }

    private async Task<List<Contact>> ExtractDirectoryUsersAsync(
        Models.ExtractionOptions options,
        string currentUserEmail,
        int maxContacts,
        CancellationToken cancellationToken = default)
    {
        var contacts = new List<Contact>();
        var client = await _authService.GetAuthenticatedClientAsync(cancellationToken);

        _logger.LogInformation("Extracting directory users...");

        try
        {
            var users = await client.Users.GetAsync(requestConfig =>
            {
                requestConfig.QueryParameters.Top = 100;
                requestConfig.QueryParameters.Select =
                [
                    "id", "displayName", "givenName", "surname", "mail",
                    "companyName", "jobTitle", "department", "officeLocation",
                    "businessPhones", "mobilePhone", "userPrincipalName"
                ];
                // Filter to only get users with email addresses
                requestConfig.QueryParameters.Filter = "mail ne null";
            }, cancellationToken);

            while (users?.Value != null && contacts.Count < maxContacts)
            {
                foreach (var user in users.Value)
                {
                    if (contacts.Count >= maxContacts)
                        break;

                    cancellationToken.ThrowIfCancellationRequested();

                    var email = user.Mail ?? user.UserPrincipalName;
                    if (string.IsNullOrWhiteSpace(email))
                        continue;

                    // Skip current user
                    if (email.Equals(currentUserEmail, StringComparison.OrdinalIgnoreCase))
                        continue;

                    var contact = CreateContactFromUser(user, email);
                    if (IsValidContact(contact, options))
                    {
                        contacts.Add(contact);
                    }
                }

                // Get next page
                if (users.OdataNextLink != null && contacts.Count < maxContacts)
                {
                    users = await client.Users
                        .WithUrl(users.OdataNextLink)
                        .GetAsync(cancellationToken: cancellationToken);
                }
                else
                {
                    break;
                }
            }
        }
        catch (Microsoft.Graph.Models.ODataErrors.ODataError ex) when (ex.ResponseStatusCode == 403)
        {
            _logger.LogWarning("Insufficient permissions to access directory users. " +
                "Add User.Read.All permission to include all directory users.");
        }

        return contacts;
    }

    private static string? GetPrimaryEmail(Person person)
    {
        // Try scored email addresses first
        if (person.ScoredEmailAddresses?.Count > 0)
        {
            return person.ScoredEmailAddresses[0].Address;
        }

        // Fall back to userPrincipalName
        return person.UserPrincipalName;
    }

    private static Contact CreateContact(Person person, string email)
    {
        var contact = new Contact
        {
            Email = email.ToLowerInvariant(),
            DisplayName = person.DisplayName ?? email,
            FirstName = person.GivenName,
            LastName = person.Surname,
            Company = person.CompanyName,
            JobTitle = person.JobTitle,
            Department = person.Department,
            Location = person.OfficeLocation,
            Sources = [Models.ContactSource.GlobalAddressList],
            // GAL contacts are authoritative, high confidence
            EnrichmentConfidence = 95
        };

        // Extract phone numbers from phones collection
        if (person.Phones?.Count > 0)
        {
            foreach (var phone in person.Phones)
            {
                if (phone.Type == PhoneType.Business && contact.Phone == null)
                {
                    contact.Phone = phone.Number;
                }
                else if (phone.Type == PhoneType.Mobile && contact.MobilePhone == null)
                {
                    contact.MobilePhone = phone.Number;
                }
            }
        }

        return contact;
    }

    private static Contact CreateContactFromUser(User user, string email)
    {
        var contact = new Contact
        {
            Email = email.ToLowerInvariant(),
            DisplayName = user.DisplayName ?? email,
            FirstName = user.GivenName,
            LastName = user.Surname,
            Company = user.CompanyName,
            JobTitle = user.JobTitle,
            Department = user.Department,
            Location = user.OfficeLocation,
            Sources = [Models.ContactSource.GlobalAddressList],
            // Directory users are authoritative, highest confidence
            EnrichmentConfidence = 100
        };

        // Extract phone numbers
        if (user.BusinessPhones?.Count > 0)
        {
            contact.Phone = user.BusinessPhones[0];
        }

        if (!string.IsNullOrWhiteSpace(user.MobilePhone))
        {
            contact.MobilePhone = user.MobilePhone;
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
