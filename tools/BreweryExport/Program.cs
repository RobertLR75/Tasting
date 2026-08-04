using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

var outputDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "data", "breweries"));
Directory.CreateDirectory(outputDirectory);

var outputPath = Path.Combine(outputDirectory, "active-breweries.json");
var httpClient = new HttpClient
{
    Timeout = TimeSpan.FromMinutes(5)
};

httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Tasting Brewery Export/1.0");

var openBreweryBreweries = await FetchOpenBreweryDbAsync(httpClient);
var wikidataBreweries = await FetchWikidataAsync(httpClient);

var mergedBreweries = MergeBreweries(openBreweryBreweries, wikidataBreweries)
    .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
    .ThenBy(x => x.Country, StringComparer.OrdinalIgnoreCase)
    .ToList();

var export = new BreweryExportDocument(
    GeneratedAtUtc: DateTimeOffset.UtcNow,
    RecordCount: mergedBreweries.Count,
    Sources:
    [
        new BreweryExportSource("openbrewerydb", "https://api.openbrewerydb.org/v1/breweries", openBreweryBreweries.Count),
        new BreweryExportSource("wikidata", "https://query.wikidata.org/", wikidataBreweries.Count)
    ],
    Breweries: mergedBreweries);

var jsonOptions = new JsonSerializerOptions
{
    WriteIndented = true,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};

await using var outputStream = File.Create(outputPath);
await JsonSerializer.SerializeAsync(outputStream, export, jsonOptions);

Console.WriteLine($"Wrote {mergedBreweries.Count} breweries to {outputPath}");

static async Task<List<ExportBrewery>> FetchOpenBreweryDbAsync(HttpClient httpClient)
{
    const int perPage = 200;
    var page = 1;
    var breweries = new List<ExportBrewery>();

    while (true)
    {
        var url = $"https://api.openbrewerydb.org/v1/breweries?per_page={perPage}&page={page}";
        using var stream = await httpClient.GetStreamAsync(url);
        var items = await JsonSerializer.DeserializeAsync<List<OpenBreweryDbItem>>(stream) ?? [];
        if (items.Count == 0)
        {
            break;
        }

        breweries.AddRange(items
            .Where(IsIncludedOpenBreweryType)
            .Select(item => new ExportBrewery(
                Name: item.Name.Trim(),
                Country: NormalizeOptional(item.Country),
                StateProvince: NormalizeOptional(item.StateProvince),
                City: NormalizeOptional(item.City),
                WebsiteUrl: NormalizeOptional(item.WebsiteUrl),
                IsActive: item.BreweryType is not "closed" and not "planning",
                SourceEntries:
                [
                    new BrewerySourceEntry(
                        Source: "openbrewerydb",
                        SourceId: item.Id,
                        BreweryType: NormalizeOptional(item.BreweryType),
                        SourceUrl: $"https://api.openbrewerydb.org/v1/breweries/{item.Id}")
                ])));

        page++;
    }

    return breweries;
}

static bool IsIncludedOpenBreweryType(OpenBreweryDbItem item)
{
    return item.BreweryType switch
    {
        "closed" => false,
        "planning" => false,
        "bar" => false,
        "cidery" => false,
        "beergarden" => false,
        "location" => false,
        _ => !string.IsNullOrWhiteSpace(item.Name)
    };
}

static async Task<List<ExportBrewery>> FetchWikidataAsync(HttpClient httpClient)
{
    const string sparql = """
        SELECT ?item ?itemLabel ?countryLabel ?website WHERE {
          ?item wdt:P31/wdt:P279* wd:Q131734.
          FILTER(NOT EXISTS { ?item wdt:P576 ?dissolved })
          OPTIONAL { ?item wdt:P17 ?country. }
          OPTIONAL { ?item wdt:P856 ?website. }
          SERVICE wikibase:label { bd:serviceParam wikibase:language "en". }
        }
        """;

    var url = $"https://query.wikidata.org/sparql?format=json&query={Uri.EscapeDataString(sparql)}";
    using var stream = await httpClient.GetStreamAsync(url);
    var response = await JsonSerializer.DeserializeAsync<WikidataResponse>(stream);

    return response?.Results.Bindings
        .Where(x => !string.IsNullOrWhiteSpace(x.ItemLabel?.Value))
        .Select(x => new ExportBrewery(
            Name: x.ItemLabel!.Value.Trim(),
            Country: NormalizeOptional(x.CountryLabel?.Value),
            StateProvince: null,
            City: null,
            WebsiteUrl: NormalizeOptional(x.Website?.Value),
            IsActive: true,
            SourceEntries:
            [
                new BrewerySourceEntry(
                    Source: "wikidata",
                    SourceId: x.Item?.Value?.Split('/').LastOrDefault(),
                    BreweryType: null,
                    SourceUrl: x.Item?.Value)
            ]))
        .ToList() ?? [];
}

static List<ExportBrewery> MergeBreweries(IEnumerable<ExportBrewery> primary, IEnumerable<ExportBrewery> secondary)
{
    var merged = new Dictionary<string, ExportBrewery>(StringComparer.Ordinal);

    foreach (var brewery in primary.Concat(secondary))
    {
        var key = BuildConservativeKey(brewery);
        if (!merged.TryGetValue(key, out var existing))
        {
            merged[key] = brewery with
            {
                SourceEntries = brewery.SourceEntries
                    .DistinctBy(x => $"{x.Source}:{x.SourceId}")
                    .ToList()
            };
            continue;
        }

        merged[key] = existing with
        {
            Country = existing.Country ?? brewery.Country,
            StateProvince = existing.StateProvince ?? brewery.StateProvince,
            City = existing.City ?? brewery.City,
            WebsiteUrl = existing.WebsiteUrl ?? brewery.WebsiteUrl,
            IsActive = existing.IsActive || brewery.IsActive,
            SourceEntries = existing.SourceEntries
                .Concat(brewery.SourceEntries)
                .DistinctBy(x => $"{x.Source}:{x.SourceId}")
                .ToList()
        };
    }

    return merged.Values.ToList();
}

static string BuildConservativeKey(ExportBrewery brewery)
{
    var normalizedName = NormalizeNameForKey(brewery.Name);
    var normalizedCountry = NormalizeOptional(brewery.Country)?.ToUpperInvariant() ?? "UNKNOWN";
    var normalizedCity = NormalizeOptional(brewery.City)?.ToUpperInvariant() ?? string.Empty;

    return $"{normalizedName}|{normalizedCountry}|{normalizedCity}";
}

static string NormalizeNameForKey(string value)
{
    var normalized = value.Normalize(NormalizationForm.FormKC).Trim().ToUpperInvariant();
    normalized = Regex.Replace(normalized, @"\s+", " ");
    normalized = normalized
        .Replace(" BREWING COMPANY", string.Empty, StringComparison.Ordinal)
        .Replace(" BREWING CO", string.Empty, StringComparison.Ordinal)
        .Replace(" BREWERY", string.Empty, StringComparison.Ordinal)
        .Replace(" BRYGGERI", string.Empty, StringComparison.Ordinal)
        .Trim();

    return normalized;
}

static string? NormalizeOptional(string? value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return null;
    }

    return value.Trim();
}

internal sealed record BreweryExportDocument(
    DateTimeOffset GeneratedAtUtc,
    int RecordCount,
    List<BreweryExportSource> Sources,
    List<ExportBrewery> Breweries);

internal sealed record BreweryExportSource(
    string Name,
    string Url,
    int RecordsFetched);

internal sealed record ExportBrewery(
    string Name,
    string? Country,
    string? StateProvince,
    string? City,
    string? WebsiteUrl,
    bool IsActive,
    List<BrewerySourceEntry> SourceEntries);

internal sealed record BrewerySourceEntry(
    string Source,
    string? SourceId,
    string? BreweryType,
    string? SourceUrl);

internal sealed record OpenBreweryDbItem(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("brewery_type")] string? BreweryType,
    [property: JsonPropertyName("city")] string? City,
    [property: JsonPropertyName("state_province")] string? StateProvince,
    [property: JsonPropertyName("country")] string? Country,
    [property: JsonPropertyName("website_url")] string? WebsiteUrl);

internal sealed record WikidataResponse(
    [property: JsonPropertyName("results")] WikidataResults Results);

internal sealed record WikidataResults(
    [property: JsonPropertyName("bindings")] List<WikidataBinding> Bindings);

internal sealed record WikidataBinding(
    [property: JsonPropertyName("item")] WikidataValue? Item,
    [property: JsonPropertyName("itemLabel")] WikidataValue? ItemLabel,
    [property: JsonPropertyName("countryLabel")] WikidataValue? CountryLabel,
    [property: JsonPropertyName("website")] WikidataValue? Website);

internal sealed record WikidataValue(
    [property: JsonPropertyName("value")] string Value);
