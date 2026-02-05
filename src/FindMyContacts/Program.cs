using FindMyContacts.Configuration;
using FindMyContacts.Models;
using FindMyContacts.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace FindMyContacts;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        try
        {
            var options = ParseArguments(args);

            if (options == null)
            {
                PrintUsage();
                return 1;
            }

            AnsiConsole.Write(new FigletText("FindMyContacts")
                .Centered()
                .Color(Color.Blue));

            AnsiConsole.MarkupLine("[grey]Office 365 Contact Extractor[/]");
            AnsiConsole.WriteLine();

            using var host = CreateHost(args);
            var extractionService = host.Services.GetRequiredService<IContactExtractionService>();
            var outputFormatter = host.Services.GetRequiredService<IOutputFormatter>();

            ExtractionResult? result = null;

            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync("Extracting contacts...", async ctx =>
                {
                    ctx.Status("Authenticating with Microsoft Graph...");
                    result = await extractionService.ExtractContactsAsync(options);

                    ctx.Status("Formatting output...");
                    await outputFormatter.WriteOutputAsync(result, options);
                });

            if (result == null)
            {
                AnsiConsole.MarkupLine("[red]Extraction failed to produce results.[/]");
                return 1;
            }

            PrintSummary(result);

            return result.Errors.Count == 0 ? 0 : 1;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            return 1;
        }
    }

    private static ExtractionOptions? ParseArguments(string[] args)
    {
        var options = new ExtractionOptions();

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i].ToLowerInvariant();

            switch (arg)
            {
                case "-h":
                case "--help":
                    return null;

                case "-o":
                case "--output":
                    if (i + 1 < args.Length)
                        options.OutputPath = args[++i];
                    break;

                case "-f":
                case "--format":
                    if (i + 1 < args.Length)
                    {
                        options.OutputFormat = args[++i].ToLowerInvariant() switch
                        {
                            "json" => OutputFormat.Json,
                            "csv" => OutputFormat.Csv,
                            "vcard" or "vcf" => OutputFormat.Vcard,
                            _ => OutputFormat.Json
                        };
                    }
                    break;

                case "--max-emails":
                    if (i + 1 < args.Length && int.TryParse(args[++i], out var maxEmails))
                        options.MaxEmails = maxEmails;
                    break;

                case "--max-events":
                    if (i + 1 < args.Length && int.TryParse(args[++i], out var maxEvents))
                        options.MaxEvents = maxEvents;
                    break;

                case "--days":
                    if (i + 1 < args.Length && int.TryParse(args[++i], out var days))
                    {
                        options.EmailLookbackPeriod = TimeSpan.FromDays(days);
                        options.EventLookbackPeriod = TimeSpan.FromDays(days);
                    }
                    break;

                case "--no-inbox":
                    options.IncludeInbox = false;
                    break;

                case "--no-sent":
                    options.IncludeSentItems = false;
                    break;

                case "--no-cc":
                    options.IncludeCcRecipients = false;
                    break;

                case "--no-signatures":
                    options.EnableSignatureExtraction = false;
                    break;

                case "--no-outlook-contacts":
                    options.IncludeOutlookContacts = false;
                    break;

                case "--no-gal":
                    options.IncludeGal = false;
                    break;

                case "--no-directory-users":
                    options.IncludeDirectoryUsers = false;
                    break;

                case "--max-outlook-contacts":
                    if (i + 1 < args.Length && int.TryParse(args[++i], out var maxOutlook))
                        options.MaxOutlookContacts = maxOutlook;
                    break;

                case "--max-gal-contacts":
                    if (i + 1 < args.Length && int.TryParse(args[++i], out var maxGal))
                        options.MaxGalContacts = maxGal;
                    break;

                case "--exclude-domain":
                    if (i + 1 < args.Length)
                        options.ExcludedDomains.Add(args[++i]);
                    break;
            }
        }

        return options;
    }

    private static void PrintUsage()
    {
        AnsiConsole.Write(new FigletText("FindMyContacts")
            .Centered()
            .Color(Color.Blue));

        var panel = new Panel(new Markup(
            """
            [bold]Office 365 Contact Extractor[/]

            Extracts contacts from your Office 365 mailbox, calendar,
            personal contacts, and Global Address List (GAL). Creates
            a deduplicated list enriched with email signature data.

            [bold yellow]Usage:[/]
              FindMyContacts [options]

            [bold yellow]Options:[/]
              -h, --help              Show this help message
              -o, --output <path>     Output file path (default: stdout)
              -f, --format <format>   Output format: json, csv, vcard (default: json)
              --max-emails <n>        Maximum emails to process (default: 1000)
              --max-events <n>        Maximum calendar events (default: 500)
              --days <n>              Lookback period in days (default: 365)
              --no-inbox              Skip inbox folder
              --no-sent               Skip sent items folder
              --no-cc                 Skip CC recipients
              --no-signatures         Skip signature parsing
              --no-outlook-contacts   Skip Outlook personal contacts
              --no-gal                Skip Global Address List
              --no-directory-users    Skip directory users (requires User.ReadBasic.All)
              --max-outlook-contacts  Maximum Outlook contacts (default: unlimited)
              --max-gal-contacts      Maximum GAL contacts (default: unlimited)
              --exclude-domain <d>    Exclude additional domain pattern

            [bold yellow]Authentication:[/]
              Set these environment variables or use appsettings.json:
              - GRAPH_TENANT_ID       Azure AD tenant ID (default: common)
              - GRAPH_CLIENT_ID       Azure AD application (client) ID

            [bold yellow]Required Permissions:[/]
              User.Read, Mail.Read, Calendars.Read, Contacts.Read,
              People.Read, User.ReadBasic.All (for directory users)

            [bold yellow]Examples:[/]
              FindMyContacts --format csv -o contacts.csv
              FindMyContacts --days 90 --max-emails 500
              FindMyContacts --no-gal --no-outlook-contacts
              FindMyContacts -f vcard -o contacts.vcf
            """))
        {
            Border = BoxBorder.Rounded,
            Padding = new Padding(2, 1)
        };

        AnsiConsole.Write(panel);
    }

    private static void PrintSummary(ExtractionResult result)
    {
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title("[bold blue]Extraction Summary[/]")
            .AddColumn("Metric")
            .AddColumn(new TableColumn("Value").RightAligned());

        table.AddRow("Total contacts found", result.Statistics.TotalContactsFound.ToString("N0"));
        table.AddRow("Unique contacts", result.Statistics.UniqueContactsAfterDeduplication.ToString("N0"));
        table.AddRow("From emails", result.Statistics.TotalEmailsProcessed.ToString("N0"));
        table.AddRow("From meetings", result.Statistics.TotalMeetingsProcessed.ToString("N0"));
        table.AddRow("From Outlook contacts", result.Statistics.TotalOutlookContactsProcessed.ToString("N0"));
        table.AddRow("From GAL", result.Statistics.TotalGalContactsProcessed.ToString("N0"));
        table.AddRow("Contacts with company", result.Statistics.ContactsWithCompany.ToString("N0"));
        table.AddRow("Contacts with job title", result.Statistics.ContactsWithJobTitle.ToString("N0"));
        table.AddRow("Contacts with phone", result.Statistics.ContactsWithPhone.ToString("N0"));
        table.AddRow("Contacts enriched", result.Statistics.ContactsEnriched.ToString("N0"));
        table.AddRow("Duration", result.Duration.ToString(@"mm\:ss"));

        AnsiConsole.Write(table);

        if (result.Statistics.ContactsBySource.Count > 0)
        {
            AnsiConsole.WriteLine();
            var sourceTable = new Table()
                .Border(TableBorder.Rounded)
                .Title("[bold blue]Contacts by Source[/]")
                .AddColumn("Source")
                .AddColumn(new TableColumn("Count").RightAligned());

            foreach (var kvp in result.Statistics.ContactsBySource.OrderByDescending(x => x.Value))
            {
                sourceTable.AddRow(kvp.Key.ToString(), kvp.Value.ToString("N0"));
            }

            AnsiConsole.Write(sourceTable);
        }

        if (result.Errors.Count > 0)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[red]Errors occurred during extraction:[/]");
            foreach (var error in result.Errors)
            {
                AnsiConsole.MarkupLine($"  [red]• {error}[/]");
            }
        }
    }

    private static IHost CreateHost(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(AppContext.BaseDirectory);
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
                config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true);
                config.AddEnvironmentVariables("GRAPH_");
                config.AddCommandLine(args);
            })
            .ConfigureServices((context, services) =>
            {
                // Configuration
                services.Configure<GraphConfiguration>(options =>
                {
                    var section = context.Configuration.GetSection(GraphConfiguration.SectionName);
                    section.Bind(options);

                    // Allow environment variable overrides
                    options.TenantId = context.Configuration["TENANT_ID"] ?? options.TenantId;
                    options.ClientId = context.Configuration["CLIENT_ID"] ?? options.ClientId;
                    options.ClientSecret = context.Configuration["CLIENT_SECRET"] ?? options.ClientSecret;
                });

                // Services
                services.AddSingleton<IGraphAuthenticationService, GraphAuthenticationService>();
                services.AddSingleton<ISignatureParser, SignatureParser>();
                services.AddSingleton<IContactAggregator, ContactAggregator>();
                services.AddTransient<IEmailContactExtractor, EmailContactExtractor>();
                services.AddTransient<IMeetingContactExtractor, MeetingContactExtractor>();
                services.AddTransient<IOutlookContactExtractor, OutlookContactExtractor>();
                services.AddTransient<IGalContactExtractor, GalContactExtractor>();
                services.AddTransient<IContactExtractionService, ContactExtractionService>();
                services.AddTransient<IOutputFormatter, OutputFormatter>();
            })
            .ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(LogLevel.Warning);

                if (context.Configuration.GetValue<bool>("Debug"))
                {
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Debug);
                }
            })
            .Build();
    }
}
