using System.Text.Json;
using YourApp.Models;

public class RawgService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;

    public RawgService(IHttpClientFactory factory, IConfiguration config)
    {
        _http = factory.CreateClient("Rawg");
        _apiKey = config["RawgApi:Key"]
            ?? throw new InvalidOperationException("RAWG API key not configured");
    }

    public async Task<RawgSearchResponse?> SearchGamesAsync(string query)
    {
        var url = $"games?key={_apiKey}&search={Uri.EscapeDataString(query)}";
        return await _http.GetFromJsonAsync<RawgSearchResponse>(url);
    }
}