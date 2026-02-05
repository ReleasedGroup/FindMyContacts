using FindMyContacts.Models;

namespace FindMyContacts.Services;

/// <summary>
/// Service for formatting contact output
/// </summary>
public interface IOutputFormatter
{
    /// <summary>
    /// Formats the extraction result to the specified output format
    /// </summary>
    Task<string> FormatAsync(ExtractionResult result, OutputFormat format, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes the formatted output to a file or stdout
    /// </summary>
    Task WriteOutputAsync(ExtractionResult result, ExtractionOptions options, CancellationToken cancellationToken = default);
}
