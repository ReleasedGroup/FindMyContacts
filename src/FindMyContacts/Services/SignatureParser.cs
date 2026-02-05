using System.Text.RegularExpressions;
using System.Web;
using Microsoft.Extensions.Logging;

namespace FindMyContacts.Services;

/// <summary>
/// Parses email signatures to extract contact enrichment data
/// </summary>
public partial class SignatureParser : ISignatureParser
{
    private readonly ILogger<SignatureParser> _logger;

    public SignatureParser(ILogger<SignatureParser> logger)
    {
        _logger = logger;
    }

    public Task<SignatureData?> ParseSignatureAsync(string bodyContent, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(bodyContent))
            return Task.FromResult<SignatureData?>(null);

        try
        {
            // Convert HTML to plain text and find signature section
            var plainText = StripHtml(bodyContent);
            var signatureText = ExtractSignatureSection(plainText);

            if (string.IsNullOrWhiteSpace(signatureText))
                return Task.FromResult<SignatureData?>(null);

            var data = new SignatureData
            {
                RawSignature = signatureText,
                Confidence = 0
            };

            // Extract phone numbers
            ExtractPhoneNumbers(signatureText, data);

            // Extract company name
            ExtractCompany(signatureText, data);

            // Extract job title
            ExtractJobTitle(signatureText, data);

            // Extract website
            ExtractWebsite(bodyContent, signatureText, data);

            // Extract LinkedIn
            ExtractLinkedIn(bodyContent, data);

            // Calculate confidence based on what we found
            CalculateConfidence(data);

            return Task.FromResult<SignatureData?>(data.Confidence > 0 ? data : null);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error parsing signature");
            return Task.FromResult<SignatureData?>(null);
        }
    }

    private static string StripHtml(string html)
    {
        // Decode HTML entities
        var text = HttpUtility.HtmlDecode(html);

        // Replace common block elements with newlines
        text = Regex.Replace(text, @"<br\s*/?>", "\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"</?(p|div|tr|li)[^>]*>", "\n", RegexOptions.IgnoreCase);

        // Remove all HTML tags
        text = Regex.Replace(text, @"<[^>]+>", " ");

        // Normalize whitespace
        text = Regex.Replace(text, @"[ \t]+", " ");
        text = Regex.Replace(text, @"\n\s*\n+", "\n\n");

        return text.Trim();
    }

    private static string? ExtractSignatureSection(string text)
    {
        // Look for common signature delimiters
        string[] delimiters =
        [
            "\n--\n",
            "\n-- \n",
            "\nBest regards,",
            "\nBest,",
            "\nRegards,",
            "\nThanks,",
            "\nThank you,",
            "\nSincerely,",
            "\nCheers,",
            "\nKind regards,",
            "\nWarm regards,",
            "\nBest wishes,",
            "\n\nSent from my"
        ];

        int signatureStart = -1;
        foreach (var delimiter in delimiters)
        {
            var idx = text.IndexOf(delimiter, StringComparison.OrdinalIgnoreCase);
            if (idx >= 0 && (signatureStart < 0 || idx > signatureStart))
            {
                signatureStart = idx;
            }
        }

        if (signatureStart >= 0)
        {
            var signature = text[signatureStart..];
            // Limit signature to reasonable length
            if (signature.Length > 1500)
                signature = signature[..1500];
            return signature;
        }

        // If no delimiter found, take the last portion of the email
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length > 5)
        {
            return string.Join('\n', lines.TakeLast(10));
        }

        return null;
    }

    private void ExtractPhoneNumbers(string text, SignatureData data)
    {
        // Match various phone number formats
        var phoneMatches = PhoneRegex().Matches(text);

        foreach (Match match in phoneMatches)
        {
            var phone = NormalizePhoneNumber(match.Value);

            // Check if it's labeled as mobile/cell
            var precedingText = text[..match.Index].TakeLast(50).ToString();
            if (IsMobileLabel(precedingText))
            {
                data.MobilePhone ??= phone;
            }
            else
            {
                data.Phone ??= phone;
            }
        }
    }

    private static bool IsMobileLabel(string text)
    {
        var lower = text.ToLowerInvariant();
        return lower.Contains("mobile") ||
               lower.Contains("cell") ||
               lower.Contains("mob:") ||
               lower.Contains("m:");
    }

    private static string NormalizePhoneNumber(string phone)
    {
        // Remove common prefixes and normalize
        var cleaned = Regex.Replace(phone, @"^(tel:|phone:|ph:|p:|t:|fax:|f:)\s*", "", RegexOptions.IgnoreCase);
        return cleaned.Trim();
    }

    private void ExtractCompany(string text, SignatureData data)
    {
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        // Look for company indicators
        foreach (var line in lines)
        {
            // Skip lines that are likely names or titles
            if (line.Length < 3 || line.Length > 100)
                continue;

            // Check for common company suffixes
            if (CompanySuffixRegex().IsMatch(line))
            {
                data.Company = CleanCompanyName(line);
                return;
            }
        }

        // Try to find company from context clues
        foreach (var line in lines)
        {
            var lowerLine = line.ToLowerInvariant();

            // Look for "at [Company]" or "[Company] - title" patterns
            var atMatch = Regex.Match(line, @"\bat\s+([A-Z][A-Za-z0-9\s&]+(?:Inc|LLC|Ltd|Corp|Co)?\.?)", RegexOptions.None);
            if (atMatch.Success)
            {
                data.Company = CleanCompanyName(atMatch.Groups[1].Value);
                return;
            }
        }
    }

    private static string CleanCompanyName(string name)
    {
        // Remove common prefixes and clean up
        var cleaned = name.Trim();
        cleaned = Regex.Replace(cleaned, @"^(at|@)\s+", "", RegexOptions.IgnoreCase);
        cleaned = Regex.Replace(cleaned, @"\s*[|\-–]\s*$", "");
        return cleaned.Trim();
    }

    private void ExtractJobTitle(string text, SignatureData data)
    {
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        foreach (var line in lines)
        {
            if (line.Length < 3 || line.Length > 80)
                continue;

            // Check for common job title patterns
            if (JobTitleRegex().IsMatch(line))
            {
                // Clean up the title
                var title = line;
                title = Regex.Replace(title, @"\s*[|\-–@]\s*.*$", ""); // Remove company part
                title = Regex.Replace(title, @"^(title:|position:)\s*", "", RegexOptions.IgnoreCase);

                if (title.Length >= 3 && title.Length <= 60)
                {
                    data.JobTitle = title.Trim();
                    return;
                }
            }
        }
    }

    private void ExtractWebsite(string htmlContent, string plainText, SignatureData data)
    {
        // First try to find URLs in HTML hrefs
        var hrefMatches = HrefRegex().Matches(htmlContent);
        foreach (Match match in hrefMatches)
        {
            var url = match.Groups[1].Value;
            if (IsCompanyWebsite(url))
            {
                data.Website = NormalizeUrl(url);
                return;
            }
        }

        // Then try plain text URLs
        var urlMatches = UrlRegex().Matches(plainText);
        foreach (Match match in urlMatches)
        {
            var url = match.Value;
            if (IsCompanyWebsite(url))
            {
                data.Website = NormalizeUrl(url);
                return;
            }
        }
    }

    private static bool IsCompanyWebsite(string url)
    {
        var lower = url.ToLowerInvariant();

        // Exclude social media and common non-company URLs
        string[] excludedDomains =
        [
            "linkedin.com", "twitter.com", "facebook.com", "instagram.com",
            "youtube.com", "github.com", "mailto:", "tel:", "maps.google",
            "outlook.com", "office.com", "microsoft.com", "zoom.us", "teams.microsoft"
        ];

        return !excludedDomains.Any(d => lower.Contains(d));
    }

    private static string NormalizeUrl(string url)
    {
        if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            url = "https://" + url;
        }
        return url.TrimEnd('/');
    }

    private void ExtractLinkedIn(string htmlContent, SignatureData data)
    {
        var linkedInMatch = LinkedInRegex().Match(htmlContent);
        if (linkedInMatch.Success)
        {
            data.LinkedInUrl = NormalizeUrl(linkedInMatch.Value);
        }
    }

    private static void CalculateConfidence(SignatureData data)
    {
        int confidence = 0;

        if (!string.IsNullOrWhiteSpace(data.Company))
            confidence += 30;
        if (!string.IsNullOrWhiteSpace(data.JobTitle))
            confidence += 25;
        if (!string.IsNullOrWhiteSpace(data.Phone))
            confidence += 20;
        if (!string.IsNullOrWhiteSpace(data.MobilePhone))
            confidence += 10;
        if (!string.IsNullOrWhiteSpace(data.Website))
            confidence += 10;
        if (!string.IsNullOrWhiteSpace(data.LinkedInUrl))
            confidence += 5;

        data.Confidence = Math.Min(confidence, 100);
    }

    // Regex patterns using source generators for performance
    [GeneratedRegex(@"(?:tel:|phone:|ph:|p:|t:|mobile:|cell:|m:|fax:|f:)?\s*[\+]?[(]?[0-9]{1,3}[)]?[-\s\.]?[(]?[0-9]{1,4}[)]?[-\s\.]?[0-9]{1,4}[-\s\.]?[0-9]{1,9}", RegexOptions.IgnoreCase)]
    private static partial Regex PhoneRegex();

    [GeneratedRegex(@"\b(?:Inc\.?|LLC|Ltd\.?|Corp\.?|Corporation|Company|Co\.?|Group|Holdings|Partners|Consulting|Solutions|Services|Technologies|Tech|Labs?|Software|Systems)\b", RegexOptions.IgnoreCase)]
    private static partial Regex CompanySuffixRegex();

    [GeneratedRegex(@"\b(?:CEO|CTO|CFO|COO|CMO|CIO|President|Vice\s*President|VP|Director|Manager|Lead|Head|Chief|Senior|Sr\.?|Junior|Jr\.?|Principal|Staff|Engineer|Developer|Designer|Analyst|Consultant|Architect|Specialist|Coordinator|Administrator|Executive|Officer|Associate|Assistant|Intern)\b", RegexOptions.IgnoreCase)]
    private static partial Regex JobTitleRegex();

    [GeneratedRegex(@"href\s*=\s*[""']([^""']+)[""']", RegexOptions.IgnoreCase)]
    private static partial Regex HrefRegex();

    [GeneratedRegex(@"(?:https?://)?(?:www\.)?[a-zA-Z0-9][-a-zA-Z0-9]*\.[a-zA-Z]{2,}(?:/[^\s<>""]*)?", RegexOptions.IgnoreCase)]
    private static partial Regex UrlRegex();

    [GeneratedRegex(@"(?:https?://)?(?:www\.)?linkedin\.com/in/[a-zA-Z0-9_-]+/?", RegexOptions.IgnoreCase)]
    private static partial Regex LinkedInRegex();
}
