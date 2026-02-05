using FindMyContacts.Models;
using FindMyContacts.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FindMyContacts.Tests.Services;

public class ContactAggregatorTests
{
    private readonly ContactAggregator _aggregator;

    public ContactAggregatorTests()
    {
        var logger = Mock.Of<ILogger<ContactAggregator>>();
        _aggregator = new ContactAggregator(logger);
    }

    [Fact]
    public void AddContact_SingleContact_AddsSuccessfully()
    {
        var contact = new Contact { Email = "test@example.com", DisplayName = "Test User" };

        _aggregator.AddContact(contact);

        Assert.Equal(1, _aggregator.TotalContactsAdded);
        Assert.Equal(1, _aggregator.UniqueContactCount);
    }

    [Fact]
    public void AddContact_DuplicateEmails_Deduplicates()
    {
        var contact1 = new Contact { Email = "test@example.com", DisplayName = "Test User" };
        var contact2 = new Contact { Email = "TEST@EXAMPLE.COM", DisplayName = "Test User 2" };

        _aggregator.AddContact(contact1);
        _aggregator.AddContact(contact2);

        Assert.Equal(2, _aggregator.TotalContactsAdded);
        Assert.Equal(1, _aggregator.UniqueContactCount);
    }

    [Fact]
    public void AddContact_MergesSources()
    {
        var contact1 = new Contact
        {
            Email = "test@example.com",
            Sources = [ContactSource.EmailFrom]
        };
        var contact2 = new Contact
        {
            Email = "test@example.com",
            Sources = [ContactSource.MeetingAttendee]
        };

        _aggregator.AddContact(contact1);
        _aggregator.AddContact(contact2);

        var result = _aggregator.GetDeduplicatedContacts().First();

        Assert.Contains(ContactSource.EmailFrom, result.Sources);
        Assert.Contains(ContactSource.MeetingAttendee, result.Sources);
    }

    [Fact]
    public void AddContact_MergesEnrichmentData()
    {
        var contact1 = new Contact
        {
            Email = "test@example.com",
            Company = "Company A",
            EnrichmentConfidence = 50
        };
        var contact2 = new Contact
        {
            Email = "test@example.com",
            JobTitle = "Manager",
            EnrichmentConfidence = 30
        };

        _aggregator.AddContact(contact1);
        _aggregator.AddContact(contact2);

        var result = _aggregator.GetDeduplicatedContacts().First();

        Assert.Equal("Company A", result.Company);
        Assert.Equal("Manager", result.JobTitle);
    }

    [Fact]
    public void AddContact_PrefersHigherConfidenceData()
    {
        var contact1 = new Contact
        {
            Email = "test@example.com",
            Company = "Old Company",
            EnrichmentConfidence = 30
        };
        var contact2 = new Contact
        {
            Email = "test@example.com",
            Company = "New Company",
            EnrichmentConfidence = 80
        };

        _aggregator.AddContact(contact1);
        _aggregator.AddContact(contact2);

        var result = _aggregator.GetDeduplicatedContacts().First();

        Assert.Equal("New Company", result.Company);
    }

    [Fact]
    public void AddContact_IncrementsInteractionCount()
    {
        var contact1 = new Contact { Email = "test@example.com" };
        var contact2 = new Contact { Email = "test@example.com" };
        var contact3 = new Contact { Email = "test@example.com" };

        _aggregator.AddContact(contact1);
        _aggregator.AddContact(contact2);
        _aggregator.AddContact(contact3);

        var result = _aggregator.GetDeduplicatedContacts().First();

        Assert.Equal(3, result.InteractionCount);
    }

    [Fact]
    public void AddContact_PrefersLongerDisplayName()
    {
        var contact1 = new Contact
        {
            Email = "test@example.com",
            DisplayName = "John"
        };
        var contact2 = new Contact
        {
            Email = "test@example.com",
            DisplayName = "John Doe"
        };

        _aggregator.AddContact(contact1);
        _aggregator.AddContact(contact2);

        var result = _aggregator.GetDeduplicatedContacts().First();

        Assert.Equal("John Doe", result.DisplayName);
    }

    [Fact]
    public void GetDeduplicatedContacts_SortsByInteractionCount()
    {
        var contact1 = new Contact { Email = "low@example.com", InteractionCount = 1 };
        var contact2 = new Contact { Email = "high@example.com", InteractionCount = 10 };
        var contact3 = new Contact { Email = "medium@example.com", InteractionCount = 5 };

        _aggregator.AddContact(contact1);
        _aggregator.AddContact(contact2);
        _aggregator.AddContact(contact3);

        var results = _aggregator.GetDeduplicatedContacts();

        Assert.Equal("high@example.com", results[0].Email);
        Assert.Equal("medium@example.com", results[1].Email);
        Assert.Equal("low@example.com", results[2].Email);
    }

    [Fact]
    public void AddContact_EmptyEmail_Ignored()
    {
        var contact = new Contact { Email = "", DisplayName = "No Email" };

        _aggregator.AddContact(contact);

        Assert.Equal(0, _aggregator.UniqueContactCount);
    }

    [Fact]
    public void AddContact_TracksFirstAndLastSeen()
    {
        var earlier = DateTime.UtcNow.AddDays(-10);
        var later = DateTime.UtcNow;

        var contact1 = new Contact
        {
            Email = "test@example.com",
            FirstSeenUtc = earlier,
            LastSeenUtc = earlier
        };
        var contact2 = new Contact
        {
            Email = "test@example.com",
            FirstSeenUtc = later,
            LastSeenUtc = later
        };

        _aggregator.AddContact(contact1);
        _aggregator.AddContact(contact2);

        var result = _aggregator.GetDeduplicatedContacts().First();

        Assert.Equal(earlier, result.FirstSeenUtc);
        Assert.Equal(later, result.LastSeenUtc);
    }
}
