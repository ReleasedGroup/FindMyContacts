using FindMyContacts.Models;
using Xunit;

namespace FindMyContacts.Tests.Models;

public class ContactTests
{
    [Fact]
    public void Contact_Equality_IsCaseInsensitive()
    {
        var contact1 = new Contact { Email = "test@example.com" };
        var contact2 = new Contact { Email = "TEST@EXAMPLE.COM" };

        Assert.Equal(contact1, contact2);
        Assert.Equal(contact1.GetHashCode(), contact2.GetHashCode());
    }

    [Fact]
    public void Contact_Equality_DifferentEmails_NotEqual()
    {
        var contact1 = new Contact { Email = "user1@example.com" };
        var contact2 = new Contact { Email = "user2@example.com" };

        Assert.NotEqual(contact1, contact2);
    }

    [Fact]
    public void Contact_ToString_IncludesDisplayNameAndEmail()
    {
        var contact = new Contact
        {
            Email = "john@example.com",
            DisplayName = "John Doe"
        };

        var result = contact.ToString();

        Assert.Contains("John Doe", result);
        Assert.Contains("john@example.com", result);
    }

    [Fact]
    public void Contact_ToString_WithCompany_IncludesCompany()
    {
        var contact = new Contact
        {
            Email = "john@example.com",
            DisplayName = "John Doe",
            Company = "Acme Inc"
        };

        var result = contact.ToString();

        Assert.Contains("Acme Inc", result);
    }

    [Fact]
    public void Contact_Sources_CanBeModified()
    {
        var contact = new Contact { Email = "test@example.com" };

        contact.Sources.Add(ContactSource.EmailFrom);
        contact.Sources.Add(ContactSource.MeetingAttendee);

        Assert.Contains(ContactSource.EmailFrom, contact.Sources);
        Assert.Contains(ContactSource.MeetingAttendee, contact.Sources);
        Assert.Equal(2, contact.Sources.Count);
    }
}
