using System.Runtime.CompilerServices;
using FindMyContacts.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace FindMyContacts.Services;

/// <summary>
/// Extracts contacts from email messages using Microsoft Graph
/// </summary>
public class EmailContactExtractor : IEmailContactExtractor
{
    private readonly IGraphAuthenticationService _authService;
    private readonly ISignatureParser _signatureParser;
    private readonly ILogger<EmailContactExtractor> _logger;

    public EmailContactExtractor(
        IGraphAuthenticationService authService,
        ISignatureParser signatureParser,
        ILogger<EmailContactExtractor> logger)
    {
        _authService = authService;
        _signatureParser = signatureParser;
        _logger = logger;
    }

    public async IAsyncEnumerable<Contact> ExtractFromInboxAsync(
        ExtractionOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var contact in ExtractFromFolderAsync("inbox", ContactSource.EmailFrom, options, cancellationToken))
        {
            yield return contact;
        }
    }

    public async IAsyncEnumerable<Contact> ExtractFromSentItemsAsync(
        ExtractionOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var contact in ExtractFromFolderAsync("sentitems", ContactSource.EmailTo, options, cancellationToken))
        {
            yield return contact;
        }
    }

    private async IAsyncEnumerable<Contact> ExtractFromFolderAsync(
        string folderId,
        ContactSource primarySource,
        ExtractionOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var client = await _authService.GetAuthenticatedClientAsync(cancellationToken);
        var currentUserEmail = await _authService.GetCurrentUserEmailAsync(cancellationToken);
        var cutoffDate = DateTime.UtcNow - options.EmailLookbackPeriod;

        _logger.LogInformation("Extracting contacts from {Folder} since {CutoffDate}", folderId, cutoffDate);

        int processedCount = 0;
        int maxEmails = options.MaxEmails > 0 ? options.MaxEmails : int.MaxValue;

        var messagesRequest = client.Me.MailFolders[folderId].Messages;
        var messages = await messagesRequest.GetAsync(requestConfig =>
        {
            requestConfig.QueryParameters.Top = 50;
            requestConfig.QueryParameters.Select = ["id", "subject", "from", "toRecipients", "ccRecipients", "receivedDateTime", "body"];
            requestConfig.QueryParameters.Filter = $"receivedDateTime ge {cutoffDate:yyyy-MM-ddTHH:mm:ssZ}";
            requestConfig.QueryParameters.Orderby = ["receivedDateTime desc"];
        }, cancellationToken);

        while (messages?.Value != null && processedCount < maxEmails)
        {
            foreach (var message in messages.Value)
            {
                if (processedCount >= maxEmails)
                    break;

                cancellationToken.ThrowIfCancellationRequested();

                // Extract from "From" field (inbox) or "To" field (sent items)
                if (folderId == "inbox" && message.From?.EmailAddress != null)
                {
                    var contact = CreateContactFromEmailAddress(message.From.EmailAddress, ContactSource.EmailFrom);
                    if (IsValidContact(contact, currentUserEmail, options))
                    {
                        // Try to enrich from signature
                        if (options.EnableSignatureExtraction && message.Body?.Content != null)
                        {
                            await EnrichFromBodyAsync(contact, message.Body.Content, cancellationToken);
                        }
                        yield return contact;
                    }
                }
                else if (folderId == "sentitems" && message.ToRecipients != null)
                {
                    foreach (var recipient in message.ToRecipients)
                    {
                        if (recipient.EmailAddress != null)
                        {
                            var contact = CreateContactFromEmailAddress(recipient.EmailAddress, ContactSource.EmailTo);
                            if (IsValidContact(contact, currentUserEmail, options))
                            {
                                yield return contact;
                            }
                        }
                    }
                }

                // Extract CC recipients if enabled
                if (options.IncludeCcRecipients && message.CcRecipients != null)
                {
                    foreach (var recipient in message.CcRecipients)
                    {
                        if (recipient.EmailAddress != null)
                        {
                            var contact = CreateContactFromEmailAddress(recipient.EmailAddress, ContactSource.EmailCc);
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
            if (messages.OdataNextLink != null && processedCount < maxEmails)
            {
                messages = await client.Me.MailFolders[folderId].Messages
                    .WithUrl(messages.OdataNextLink)
                    .GetAsync(cancellationToken: cancellationToken);
            }
            else
            {
                break;
            }
        }

        _logger.LogInformation("Processed {Count} emails from {Folder}", processedCount, folderId);
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

    private async Task EnrichFromBodyAsync(Contact contact, string bodyContent, CancellationToken cancellationToken)
    {
        try
        {
            var enrichmentData = await _signatureParser.ParseSignatureAsync(bodyContent, cancellationToken);

            if (enrichmentData != null)
            {
                contact.Company ??= enrichmentData.Company;
                contact.JobTitle ??= enrichmentData.JobTitle;
                contact.Phone ??= enrichmentData.Phone;
                contact.MobilePhone ??= enrichmentData.MobilePhone;
                contact.Website ??= enrichmentData.Website;
                contact.LinkedInUrl ??= enrichmentData.LinkedInUrl;
                contact.RawSignature = enrichmentData.RawSignature;
                contact.EnrichmentConfidence = enrichmentData.Confidence;
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to parse signature for {Email}", contact.Email);
        }
    }
}
