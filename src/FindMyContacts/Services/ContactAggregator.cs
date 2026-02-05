using System.Collections.Concurrent;
using FindMyContacts.Models;
using Microsoft.Extensions.Logging;

namespace FindMyContacts.Services;

/// <summary>
/// Aggregates contacts and handles deduplication with data merging
/// </summary>
public class ContactAggregator : IContactAggregator
{
    private readonly ConcurrentDictionary<string, Contact> _contacts = new(StringComparer.OrdinalIgnoreCase);
    private readonly ILogger<ContactAggregator> _logger;
    private int _totalAdded;

    public ContactAggregator(ILogger<ContactAggregator> logger)
    {
        _logger = logger;
    }

    public int TotalContactsAdded => _totalAdded;
    public int UniqueContactCount => _contacts.Count;

    public void AddContact(Contact contact)
    {
        if (string.IsNullOrWhiteSpace(contact.Email))
            return;

        Interlocked.Increment(ref _totalAdded);

        _contacts.AddOrUpdate(
            contact.Email.ToLowerInvariant(),
            contact,
            (_, existing) => MergeContacts(existing, contact));
    }

    public IReadOnlyList<Contact> GetDeduplicatedContacts()
    {
        return _contacts.Values
            .OrderByDescending(c => c.InteractionCount)
            .ThenByDescending(c => c.EnrichmentConfidence)
            .ThenBy(c => c.DisplayName)
            .ToList();
    }

    private Contact MergeContacts(Contact existing, Contact incoming)
    {
        // Merge sources
        foreach (var source in incoming.Sources)
        {
            existing.Sources.Add(source);
        }

        // Update interaction tracking
        existing.InteractionCount++;
        if (incoming.FirstSeenUtc < existing.FirstSeenUtc)
            existing.FirstSeenUtc = incoming.FirstSeenUtc;
        if (incoming.LastSeenUtc > existing.LastSeenUtc)
            existing.LastSeenUtc = incoming.LastSeenUtc;

        // Merge display name (prefer longer/more detailed names)
        if (!string.IsNullOrWhiteSpace(incoming.DisplayName) &&
            (string.IsNullOrWhiteSpace(existing.DisplayName) ||
             incoming.DisplayName.Length > existing.DisplayName.Length))
        {
            existing.DisplayName = incoming.DisplayName;
        }

        // Merge name parts
        existing.FirstName ??= incoming.FirstName;
        existing.LastName ??= incoming.LastName;

        // Merge enrichment data (prefer data with higher confidence)
        if (incoming.EnrichmentConfidence > existing.EnrichmentConfidence)
        {
            MergeEnrichmentData(existing, incoming, preferIncoming: true);
        }
        else
        {
            MergeEnrichmentData(existing, incoming, preferIncoming: false);
        }

        return existing;
    }

    private static void MergeEnrichmentData(Contact existing, Contact incoming, bool preferIncoming)
    {
        if (preferIncoming)
        {
            existing.Company = incoming.Company ?? existing.Company;
            existing.JobTitle = incoming.JobTitle ?? existing.JobTitle;
            existing.Phone = incoming.Phone ?? existing.Phone;
            existing.MobilePhone = incoming.MobilePhone ?? existing.MobilePhone;
            existing.Department = incoming.Department ?? existing.Department;
            existing.Location = incoming.Location ?? existing.Location;
            existing.Website = incoming.Website ?? existing.Website;
            existing.LinkedInUrl = incoming.LinkedInUrl ?? existing.LinkedInUrl;
            existing.RawSignature = incoming.RawSignature ?? existing.RawSignature;
            existing.EnrichmentConfidence = Math.Max(existing.EnrichmentConfidence, incoming.EnrichmentConfidence);
        }
        else
        {
            existing.Company ??= incoming.Company;
            existing.JobTitle ??= incoming.JobTitle;
            existing.Phone ??= incoming.Phone;
            existing.MobilePhone ??= incoming.MobilePhone;
            existing.Department ??= incoming.Department;
            existing.Location ??= incoming.Location;
            existing.Website ??= incoming.Website;
            existing.LinkedInUrl ??= incoming.LinkedInUrl;
            existing.RawSignature ??= incoming.RawSignature;
            existing.EnrichmentConfidence = Math.Max(existing.EnrichmentConfidence, incoming.EnrichmentConfidence);
        }
    }
}
