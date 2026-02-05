# FindMyContacts

A .NET 10 console application that extracts contacts from your Office 365 mailbox, calendar, personal contacts, and Global Address List (GAL). Creates a deduplicated list enriched with information from email signatures.

## Features

- **Email Contact Extraction**: Extracts contacts from inbox (senders) and sent items (recipients)
- **Meeting Attendee Extraction**: Extracts organizers and attendees from calendar events
- **Outlook Personal Contacts**: Imports all contacts from your Outlook contacts folder
- **Global Address List (GAL)**: Extracts contacts from your organization's directory
- **Directory Users**: Optionally includes all users from Azure AD (requires admin consent)
- **Signature Parsing**: Enriches contacts with company, job title, phone numbers, and URLs from email signatures
- **Deduplication**: Merges duplicate contacts based on email address, combining data from multiple sources
- **Multiple Output Formats**: JSON, CSV, and vCard export options
- **Flexible Authentication**: Supports device code flow, interactive browser, and client credentials

## Requirements

- .NET 10 SDK
- Azure AD application registration with the following permissions:
  - `User.Read`
  - `Mail.Read`
  - `Calendars.Read`
  - `Contacts.Read` (for Outlook personal contacts)
  - `People.Read` (for GAL access)
  - `User.ReadBasic.All` (optional, for full directory access)

## Installation

### From Release

Download the latest release for your platform from the [Releases](releases) page.

### From Source

```bash
git clone https://github.com/your-repo/FindMyContacts.git
cd FindMyContacts
dotnet build
```

## Azure AD Setup

1. Go to [Azure Portal](https://portal.azure.com) > Azure Active Directory > App registrations
2. Click "New registration"
3. Name your app (e.g., "FindMyContacts")
4. Select "Accounts in any organizational directory and personal Microsoft accounts"
5. Set redirect URI to `http://localhost` (for device code flow)
6. After creation, note the **Application (client) ID**
7. Go to "API permissions" and add:
   - Microsoft Graph > Delegated permissions:
     - `User.Read`
     - `Mail.Read`
     - `Calendars.Read`
     - `Contacts.Read`
     - `People.Read`
     - `User.ReadBasic.All` (optional, requires admin consent)
8. Grant admin consent if required by your organization

## Configuration

### Environment Variables

```bash
export GRAPH_TENANT_ID="common"  # Or your specific tenant ID
export GRAPH_CLIENT_ID="your-client-id"
```

### appsettings.json

```json
{
  "Graph": {
    "TenantId": "common",
    "ClientId": "your-client-id",
    "UseDeviceCodeFlow": true
  }
}
```

## Usage

```bash
# Basic usage - outputs JSON to stdout
FindMyContacts

# Export to CSV file
FindMyContacts --format csv --output contacts.csv

# Export to vCard for import into other apps
FindMyContacts --format vcard --output contacts.vcf

# Limit lookback period and max items
FindMyContacts --days 90 --max-emails 500 --max-events 200

# Skip certain folders or features
FindMyContacts --no-inbox --no-signatures

# Exclude additional domains
FindMyContacts --exclude-domain example.com --exclude-domain test.org
```

### Command Line Options

| Option | Description | Default |
|--------|-------------|---------|
| `-h, --help` | Show help message | - |
| `-o, --output <path>` | Output file path | stdout |
| `-f, --format <format>` | Output format: json, csv, vcard | json |
| `--max-emails <n>` | Maximum emails to process | 1000 |
| `--max-events <n>` | Maximum calendar events | 500 |
| `--days <n>` | Lookback period in days | 365 |
| `--no-inbox` | Skip inbox folder | - |
| `--no-sent` | Skip sent items folder | - |
| `--no-cc` | Skip CC recipients | - |
| `--no-signatures` | Skip signature parsing | - |
| `--exclude-domain <d>` | Exclude additional domain pattern | - |

## Output Formats

### JSON

Structured JSON with full contact details and extraction statistics:

```json
{
  "extractedAt": "2024-01-15T10:30:00Z",
  "statistics": {
    "totalContactsFound": 1500,
    "uniqueContactsAfterDeduplication": 342,
    "contactsWithCompany": 187,
    "contactsWithJobTitle": 156
  },
  "contacts": [
    {
      "email": "john.doe@example.com",
      "displayName": "John Doe",
      "company": "Example Corp",
      "jobTitle": "Senior Engineer",
      "phone": "+1 555-123-4567",
      "sources": ["emailFrom", "meetingAttendee"],
      "interactionCount": 15
    }
  ]
}
```

### CSV

Standard CSV format compatible with spreadsheets and CRM imports:

```csv
Email,DisplayName,FirstName,LastName,Company,JobTitle,Phone,MobilePhone,Website,LinkedIn,Sources,InteractionCount,EnrichmentConfidence
john.doe@example.com,John Doe,John,Doe,Example Corp,Senior Engineer,+1 555-123-4567,,https://example.com,https://linkedin.com/in/johndoe,EmailFrom;MeetingAttendee,15,65
```

### vCard

Standard vCard 3.0 format for import into contact managers:

```vcard
BEGIN:VCARD
VERSION:3.0
N:Doe;John;;;
FN:John Doe
EMAIL;TYPE=INTERNET:john.doe@example.com
ORG:Example Corp
TITLE:Senior Engineer
TEL;TYPE=WORK,VOICE:+1 555-123-4567
URL:https://example.com
END:VCARD
```

## Contact Sources

Contacts are tagged with their source(s):

- `EmailFrom` - Sender of received emails
- `EmailTo` - Recipient of sent emails
- `EmailCc` - CC recipient
- `MeetingOrganizer` - Calendar event organizer
- `MeetingAttendee` - Required meeting attendee
- `MeetingOptional` - Optional meeting attendee

## Signature Enrichment

The application attempts to extract additional information from email signatures:

- **Company name** - Detected from common patterns (Inc., LLC, Corp., etc.)
- **Job title** - Detected from title keywords (CEO, Manager, Director, etc.)
- **Phone numbers** - Various formats including mobile/cell
- **Website URLs** - Company websites (excludes social media)
- **LinkedIn profiles** - Direct profile URLs

Enrichment confidence is calculated based on how much data was successfully extracted.

## Building

```bash
# Restore and build
dotnet restore
dotnet build

# Run tests
dotnet test

# Publish for Windows ARM64
dotnet publish src/FindMyContacts/FindMyContacts.csproj \
  --configuration Release \
  --runtime win-arm64 \
  --self-contained true \
  --output ./publish/win-arm64 \
  -p:PublishSingleFile=true
```

## CI/CD

The project includes GitHub Actions workflows for:

- Building and testing on every push/PR
- Creating Windows ARM64 artifacts
- Multi-platform builds (Windows x64, Linux x64/ARM64, macOS x64/ARM64)
- Automatic release asset uploads

## Architecture

```
FindMyContacts/
├── src/
│   └── FindMyContacts/
│       ├── Configuration/      # App configuration
│       ├── Models/             # Data models
│       ├── Services/           # Core services
│       │   ├── GraphAuthenticationService    # MS Graph auth
│       │   ├── EmailContactExtractor         # Email extraction
│       │   ├── MeetingContactExtractor       # Calendar extraction
│       │   ├── SignatureParser               # Signature enrichment
│       │   ├── ContactAggregator             # Deduplication
│       │   └── OutputFormatter               # Export formats
│       └── Program.cs          # Entry point
└── tests/
    └── FindMyContacts.Tests/   # Unit tests
```

## License

MIT License

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Run tests
5. Submit a pull request
