using FindMyContacts.Models;
using FindMyContacts.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FindMyContacts.Tests.Services;

public class OutputFormatterTests
{
    private readonly OutputFormatter _formatter;
    private readonly ExtractionResult _sampleResult;

    public OutputFormatterTests()
    {
        var logger = Mock.Of<ILogger<OutputFormatter>>();
        _formatter = new OutputFormatter(logger);

        _sampleResult = new ExtractionResult
        {
            ExtractionStartedUtc = DateTime.UtcNow.AddMinutes(-5),
            ExtractionCompletedUtc = DateTime.UtcNow,
            Contacts =
            [
                new Contact
                {
                    Email = "john@example.com",
                    DisplayName = "John Doe",
                    FirstName = "John",
                    LastName = "Doe",
                    Company = "Acme Inc",
                    JobTitle = "Developer",
                    Phone = "555-1234",
                    Sources = [ContactSource.EmailFrom, ContactSource.MeetingAttendee],
                    InteractionCount = 5
                },
                new Contact
                {
                    Email = "jane@example.com",
                    DisplayName = "Jane Smith",
                    Company = "Tech Corp",
                    Sources = [ContactSource.EmailTo],
                    InteractionCount = 3
                }
            ],
            Statistics = new ExtractionStatistics
            {
                TotalContactsFound = 10,
                UniqueContactsAfterDeduplication = 2,
                ContactsWithCompany = 2,
                ContactsWithJobTitle = 1,
                ContactsWithPhone = 1
            }
        };
    }

    [Fact]
    public async Task FormatAsync_Json_ContainsContacts()
    {
        var result = await _formatter.FormatAsync(_sampleResult, OutputFormat.Json);

        Assert.Contains("john@example.com", result);
        Assert.Contains("jane@example.com", result);
        Assert.Contains("Acme Inc", result);
    }

    [Fact]
    public async Task FormatAsync_Json_ContainsStatistics()
    {
        var result = await _formatter.FormatAsync(_sampleResult, OutputFormat.Json);

        Assert.Contains("statistics", result);
        Assert.Contains("uniqueContactsAfterDeduplication", result);
    }

    [Fact]
    public async Task FormatAsync_Csv_ContainsHeader()
    {
        var result = await _formatter.FormatAsync(_sampleResult, OutputFormat.Csv);

        Assert.StartsWith("Email,DisplayName,FirstName,LastName,Company", result);
    }

    [Fact]
    public async Task FormatAsync_Csv_ContainsContacts()
    {
        var result = await _formatter.FormatAsync(_sampleResult, OutputFormat.Csv);

        Assert.Contains("john@example.com", result);
        Assert.Contains("Jane Smith", result);
    }

    [Fact]
    public async Task FormatAsync_Csv_EscapesSpecialCharacters()
    {
        _sampleResult.Contacts[0].Company = "Company, Inc.";

        var result = await _formatter.FormatAsync(_sampleResult, OutputFormat.Csv);

        Assert.Contains("\"Company, Inc.\"", result);
    }

    [Fact]
    public async Task FormatAsync_Vcard_ContainsVcardStructure()
    {
        var result = await _formatter.FormatAsync(_sampleResult, OutputFormat.Vcard);

        Assert.Contains("BEGIN:VCARD", result);
        Assert.Contains("END:VCARD", result);
        Assert.Contains("VERSION:3.0", result);
    }

    [Fact]
    public async Task FormatAsync_Vcard_ContainsContactInfo()
    {
        var result = await _formatter.FormatAsync(_sampleResult, OutputFormat.Vcard);

        Assert.Contains("FN:John Doe", result);
        Assert.Contains("EMAIL;TYPE=INTERNET:john@example.com", result);
        Assert.Contains("ORG:Acme Inc", result);
        Assert.Contains("TITLE:Developer", result);
    }

    [Fact]
    public async Task FormatAsync_Vcard_ContainsPhone()
    {
        var result = await _formatter.FormatAsync(_sampleResult, OutputFormat.Vcard);

        Assert.Contains("TEL;TYPE=WORK,VOICE:555-1234", result);
    }

    [Fact]
    public async Task FormatAsync_Vcard_ContainsStructuredName()
    {
        var result = await _formatter.FormatAsync(_sampleResult, OutputFormat.Vcard);

        Assert.Contains("N:Doe;John;;;", result);
    }

    [Fact]
    public async Task FormatAsync_Vcard_EscapesSpecialCharacters()
    {
        _sampleResult.Contacts[0].Company = "Company; Ltd, Inc.";

        var result = await _formatter.FormatAsync(_sampleResult, OutputFormat.Vcard);

        Assert.Contains("ORG:Company\\; Ltd\\, Inc.", result);
    }
}
