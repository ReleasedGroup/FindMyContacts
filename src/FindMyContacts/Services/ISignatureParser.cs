namespace FindMyContacts.Services;

/// <summary>
/// Service for parsing email signatures to extract contact information
/// </summary>
public interface ISignatureParser
{
    /// <summary>
    /// Parses email body content to extract signature information
    /// </summary>
    Task<SignatureData?> ParseSignatureAsync(string bodyContent, CancellationToken cancellationToken = default);
}

/// <summary>
/// Data extracted from an email signature
/// </summary>
public class SignatureData
{
    public string? Company { get; set; }
    public string? JobTitle { get; set; }
    public string? Phone { get; set; }
    public string? MobilePhone { get; set; }
    public string? Website { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? Address { get; set; }
    public string? RawSignature { get; set; }
    public int Confidence { get; set; }
}
