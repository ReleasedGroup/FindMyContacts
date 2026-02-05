using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FindMyContacts.Models;
using Microsoft.Extensions.Logging;

namespace FindMyContacts.Services;

/// <summary>
/// Formats contact extraction results to various output formats
/// </summary>
public class OutputFormatter : IOutputFormatter
{
    private readonly ILogger<OutputFormatter> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public OutputFormatter(ILogger<OutputFormatter> logger)
    {
        _logger = logger;
    }

    public Task<string> FormatAsync(ExtractionResult result, OutputFormat format, CancellationToken cancellationToken = default)
    {
        return format switch
        {
            OutputFormat.Json => Task.FromResult(FormatAsJson(result)),
            OutputFormat.Csv => Task.FromResult(FormatAsCsv(result)),
            OutputFormat.Vcard => Task.FromResult(FormatAsVcard(result)),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported output format")
        };
    }

    public async Task WriteOutputAsync(ExtractionResult result, ExtractionOptions options, CancellationToken cancellationToken = default)
    {
        var formatted = await FormatAsync(result, options.OutputFormat, cancellationToken);

        if (string.IsNullOrWhiteSpace(options.OutputPath))
        {
            Console.WriteLine(formatted);
        }
        else
        {
            var directory = Path.GetDirectoryName(options.OutputPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllTextAsync(options.OutputPath, formatted, cancellationToken);
            _logger.LogInformation("Output written to {Path}", options.OutputPath);
        }
    }

    private static string FormatAsJson(ExtractionResult result)
    {
        var output = new
        {
            extractedAt = result.ExtractionCompletedUtc,
            duration = result.Duration.ToString(),
            statistics = result.Statistics,
            contacts = result.Contacts.Select(c => new
            {
                c.Email,
                c.DisplayName,
                c.FirstName,
                c.LastName,
                c.Company,
                c.JobTitle,
                c.Phone,
                c.MobilePhone,
                c.Department,
                c.Website,
                c.LinkedInUrl,
                sources = c.Sources.Select(s => s.ToString()).ToList(),
                c.InteractionCount,
                c.FirstSeenUtc,
                c.LastSeenUtc,
                c.EnrichmentConfidence
            }),
            errors = result.Errors
        };

        return JsonSerializer.Serialize(output, JsonOptions);
    }

    private static string FormatAsCsv(ExtractionResult result)
    {
        var sb = new StringBuilder();

        // Header
        sb.AppendLine("Email,DisplayName,FirstName,LastName,Company,JobTitle,Phone,MobilePhone,Website,LinkedIn,Sources,InteractionCount,EnrichmentConfidence");

        // Data rows
        foreach (var contact in result.Contacts)
        {
            sb.AppendLine(string.Join(",",
                EscapeCsv(contact.Email),
                EscapeCsv(contact.DisplayName),
                EscapeCsv(contact.FirstName),
                EscapeCsv(contact.LastName),
                EscapeCsv(contact.Company),
                EscapeCsv(contact.JobTitle),
                EscapeCsv(contact.Phone),
                EscapeCsv(contact.MobilePhone),
                EscapeCsv(contact.Website),
                EscapeCsv(contact.LinkedInUrl),
                EscapeCsv(string.Join(";", contact.Sources)),
                contact.InteractionCount.ToString(),
                contact.EnrichmentConfidence.ToString()
            ));
        }

        return sb.ToString();
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        // Escape quotes and wrap in quotes if contains special characters
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }

    private static string FormatAsVcard(ExtractionResult result)
    {
        var sb = new StringBuilder();

        foreach (var contact in result.Contacts)
        {
            sb.AppendLine("BEGIN:VCARD");
            sb.AppendLine("VERSION:3.0");

            // Full name
            if (!string.IsNullOrWhiteSpace(contact.LastName) || !string.IsNullOrWhiteSpace(contact.FirstName))
            {
                sb.AppendLine($"N:{EscapeVcard(contact.LastName)};{EscapeVcard(contact.FirstName)};;;");
            }

            sb.AppendLine($"FN:{EscapeVcard(contact.DisplayName)}");
            sb.AppendLine($"EMAIL;TYPE=INTERNET:{EscapeVcard(contact.Email)}");

            if (!string.IsNullOrWhiteSpace(contact.Company))
            {
                sb.AppendLine($"ORG:{EscapeVcard(contact.Company)}");
            }

            if (!string.IsNullOrWhiteSpace(contact.JobTitle))
            {
                sb.AppendLine($"TITLE:{EscapeVcard(contact.JobTitle)}");
            }

            if (!string.IsNullOrWhiteSpace(contact.Phone))
            {
                sb.AppendLine($"TEL;TYPE=WORK,VOICE:{EscapeVcard(contact.Phone)}");
            }

            if (!string.IsNullOrWhiteSpace(contact.MobilePhone))
            {
                sb.AppendLine($"TEL;TYPE=CELL:{EscapeVcard(contact.MobilePhone)}");
            }

            if (!string.IsNullOrWhiteSpace(contact.Website))
            {
                sb.AppendLine($"URL:{EscapeVcard(contact.Website)}");
            }

            if (!string.IsNullOrWhiteSpace(contact.LinkedInUrl))
            {
                sb.AppendLine($"X-SOCIALPROFILE;TYPE=linkedin:{EscapeVcard(contact.LinkedInUrl)}");
            }

            // Add note about sources
            var sources = string.Join(", ", contact.Sources);
            sb.AppendLine($"NOTE:Extracted from Office 365. Sources: {sources}. Interactions: {contact.InteractionCount}");

            sb.AppendLine($"REV:{contact.LastSeenUtc:yyyyMMddTHHmmssZ}");
            sb.AppendLine("END:VCARD");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string EscapeVcard(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        // Escape special vCard characters
        return value
            .Replace("\\", "\\\\")
            .Replace(",", "\\,")
            .Replace(";", "\\;")
            .Replace("\n", "\\n")
            .Replace("\r", "");
    }
}
