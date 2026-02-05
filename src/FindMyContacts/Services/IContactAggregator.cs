using FindMyContacts.Models;

namespace FindMyContacts.Services;

/// <summary>
/// Service for aggregating and deduplicating contacts
/// </summary>
public interface IContactAggregator
{
    /// <summary>
    /// Adds a contact to the aggregator
    /// </summary>
    void AddContact(Contact contact);

    /// <summary>
    /// Gets the deduplicated list of contacts
    /// </summary>
    IReadOnlyList<Contact> GetDeduplicatedContacts();

    /// <summary>
    /// Gets the total number of contacts before deduplication
    /// </summary>
    int TotalContactsAdded { get; }

    /// <summary>
    /// Gets the number of unique contacts after deduplication
    /// </summary>
    int UniqueContactCount { get; }
}
