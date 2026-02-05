using FindMyContacts.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FindMyContacts.Tests.Services;

public class SignatureParserTests
{
    private readonly SignatureParser _parser;

    public SignatureParserTests()
    {
        var logger = Mock.Of<ILogger<SignatureParser>>();
        _parser = new SignatureParser(logger);
    }

    [Fact]
    public async Task ParseSignatureAsync_WithPhoneNumber_ExtractsPhone()
    {
        var body = """
            Hi,

            Thanks for your email.

            Best regards,
            John Doe
            Phone: +1 (555) 123-4567
            """;

        var result = await _parser.ParseSignatureAsync(body);

        Assert.NotNull(result);
        Assert.Contains("555", result.Phone);
    }

    [Fact]
    public async Task ParseSignatureAsync_WithCompany_ExtractsCompany()
    {
        var body = """
            Hello,

            Let me know if you have questions.

            Regards,
            Jane Smith
            Senior Developer
            Acme Corporation Inc.
            """;

        var result = await _parser.ParseSignatureAsync(body);

        Assert.NotNull(result);
        Assert.Contains("Acme", result.Company);
    }

    [Fact]
    public async Task ParseSignatureAsync_WithJobTitle_ExtractsTitle()
    {
        var body = """
            Thanks,

            --
            Bob Johnson
            Chief Technology Officer
            Tech Solutions LLC
            bob@techsolutions.com
            """;

        var result = await _parser.ParseSignatureAsync(body);

        Assert.NotNull(result);
        Assert.NotNull(result.JobTitle);
        Assert.Contains("Chief", result.JobTitle);
    }

    [Fact]
    public async Task ParseSignatureAsync_WithLinkedIn_ExtractsUrl()
    {
        var htmlBody = """
            <html>
            <body>
            <p>Thanks!</p>
            <p>--</p>
            <p>John Doe</p>
            <p><a href="https://linkedin.com/in/johndoe">LinkedIn</a></p>
            </body>
            </html>
            """;

        var result = await _parser.ParseSignatureAsync(htmlBody);

        Assert.NotNull(result);
        Assert.Contains("linkedin.com/in/johndoe", result.LinkedInUrl);
    }

    [Fact]
    public async Task ParseSignatureAsync_WithMobilePhone_ExtractsSeparately()
    {
        var body = """
            Hi there,

            Thanks for reaching out about the project.

            Best regards,
            Sarah Connor
            Phone: 555-111-2222
            Mobile: 555-333-4444
            """;

        var result = await _parser.ParseSignatureAsync(body);

        Assert.NotNull(result);
        Assert.NotNull(result.Phone);
        Assert.NotNull(result.MobilePhone);
    }

    [Fact]
    public async Task ParseSignatureAsync_EmptyBody_ReturnsNull()
    {
        var result = await _parser.ParseSignatureAsync("");

        Assert.Null(result);
    }

    [Fact]
    public async Task ParseSignatureAsync_NoSignature_ReturnsNull()
    {
        var body = "Just a short message.";

        var result = await _parser.ParseSignatureAsync(body);

        Assert.Null(result);
    }

    [Fact]
    public async Task ParseSignatureAsync_WebsiteExtraction_ExcludesSocialMedia()
    {
        var htmlBody = """
            <html>
            <body>
            <p>Thanks for your message!</p>
            <p>I'll get back to you soon.</p>
            <p>Best regards,</p>
            <p>John Doe</p>
            <p>Senior Developer</p>
            <p><a href="https://twitter.com/johndoe">Twitter</a></p>
            <p><a href="https://acmecorp.com">Website</a></p>
            </body>
            </html>
            """;

        var result = await _parser.ParseSignatureAsync(htmlBody);

        Assert.NotNull(result);
        Assert.NotNull(result.Website);
        Assert.DoesNotContain("twitter", result.Website, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("acmecorp", result.Website);
    }

    [Fact]
    public async Task ParseSignatureAsync_CalculatesConfidence()
    {
        var body = """
            Best,

            John Doe
            Senior Manager
            Acme Inc.
            Phone: 555-123-4567
            https://acme.com
            """;

        var result = await _parser.ParseSignatureAsync(body);

        Assert.NotNull(result);
        Assert.True(result.Confidence > 0);
    }
}
