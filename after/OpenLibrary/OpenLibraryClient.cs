using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace Library.After.OpenLibrary;

public sealed class OpenLibraryClient
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);
    private static readonly JsonSerializerOptions SnakeCase = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;
    private readonly ILogger<OpenLibraryClient> _logger;

    public OpenLibraryClient(HttpClient http, IMemoryCache cache, ILogger<OpenLibraryClient> logger)
    {
        _http = http;
        _cache = cache;
        _logger = logger;
    }

    public async Task<OpenLibraryEdition?> FindByIsbnAsync(string isbn, CancellationToken ct)
    {
        var key = $"isbn:{isbn}";
        if (_cache.TryGetValue(key, out OpenLibraryEdition? cached)) return cached;

        _logger.LogInformation("Looking up ISBN {Isbn} on Open Library", isbn);
        using var response = await _http.GetAsync($"isbn/{isbn}.json", ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;

        response.EnsureSuccessStatusCode();
        var edition = await response.Content.ReadFromJsonAsync<OpenLibraryEdition>(SnakeCase, ct);
        return _cache.Set(key, edition, CacheDuration);
    }
}
