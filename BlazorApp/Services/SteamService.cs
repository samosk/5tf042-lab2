using System.Text.Json;
using System.Text.Json.Serialization;

public class SteamService
{
    private readonly HttpClient _http;
    private readonly HttpClient _storeHttp;
    private readonly string _apiKey;

    public SteamService(IHttpClientFactory factory, IConfiguration config)
    {
        _http = factory.CreateClient("Steam");
        _storeHttp = factory.CreateClient("SteamStore");
        _apiKey = config["SteamApi:Key"]
            ?? throw new InvalidOperationException("Steam API key not configured");
    }

    public async Task<List<SteamGame>> GetOwnedGamesAsync(string steamId)
    {
        var url = $"IPlayerService/GetOwnedGames/v1/?key={_apiKey}&steamid={steamId}&include_appinfo=true&format=json";
        var response = await _http.GetFromJsonAsync<SteamOwnedGamesResponse>(url);
        return response?.Response?.Games ?? new List<SteamGame>();
    }
    public async Task<Dictionary<int, SteamPriceData>> GetPricesAsync(List<int> appIds)
{
    var prices = new Dictionary<int, SteamPriceData>();

    foreach (var appId in appIds)
    {
        try
        {
            var response = await _storeHttp.GetAsync(
                $"appdetails?appids={appId}&filters=price_overview,basic&cc=se");
            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty(appId.ToString(), out var appData)
                && appData.TryGetProperty("success", out var success)
                && success.GetBoolean()
                && appData.TryGetProperty("data", out var data))
            {
                // Skip DLC, demos, tools
                if (data.TryGetProperty("type", out var typeEl))
                {
                    var type = typeEl.GetString();
                    if (type is "dlc" or "demo" or "tool" or "mod" or "music")
                        continue;
                }

                if (data.TryGetProperty("is_free", out var isFreeEl)
                    && isFreeEl.GetBoolean())
                {
                    prices[appId] = new SteamPriceData
                    {
                        Currency = "SEK",
                        Initial = 0,
                        Final = 0,
                        DiscountPercent = 0,
                        IsFreeToPlay = true
                    };
                    continue;
                }

                if (data.TryGetProperty("price_overview", out var priceEl))
                {
                    var price = JsonSerializer.Deserialize<SteamPriceData>(priceEl.GetRawText());
                    if (price is not null)
                    {
                        prices[appId] = price;
                    }
                }
            }
        }
        catch
        {
            // Skip this game
        }

        // Steam rate limits to ~200 requests per 5 minutes
        await Task.Delay(300);
    }

    return prices;
}
}

// Models for the Steam response
public class SteamOwnedGamesResponse
{
    [JsonPropertyName("response")]
    public SteamOwnedGamesData? Response { get; set; }
}

public class SteamOwnedGamesData
{
    [JsonPropertyName("game_count")]
    public int GameCount { get; set; }

    [JsonPropertyName("games")]
    public List<SteamGame> Games { get; set; } = new();
}

public class SteamGame
{
    [JsonPropertyName("appid")]
    public int AppId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("playtime_forever")]
    public int PlaytimeForever { get; set; }  // in minutes

    [JsonPropertyName("img_icon_url")]
    public string ImgIconUrl { get; set; } = "";

    // Computed helpers
    public double PlaytimeHours => Math.Round(PlaytimeForever / 60.0, 1);

    public string IconFullUrl => string.IsNullOrEmpty(ImgIconUrl)
        ? ""
        : $"https://media.steampowered.com/steamcommunity/public/images/apps/{AppId}/{ImgIconUrl}.jpg";
}

public class SteamPriceData
{
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "";

    [JsonPropertyName("initial")]
    public int Initial { get; set; }

    [JsonPropertyName("final")]
    public int Final { get; set; }

    [JsonPropertyName("discount_percent")]
    public int DiscountPercent { get; set; }

    public bool IsFreeToPlay { get; set; }

    public double FinalPrice => Final / 100.0;
    public double InitialPrice => Initial / 100.0;
}