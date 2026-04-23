using System.Text.Json;
using System.Text.Json.Serialization;

public class SteamService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;

    public SteamService(IHttpClientFactory factory, IConfiguration config)
    {
        _http = factory.CreateClient("Steam");
        _apiKey = config["SteamApi:Key"]
            ?? throw new InvalidOperationException("Steam API key not configured");
    }

    public async Task<List<SteamGame>> GetOwnedGamesAsync(string steamId)
    {
        var url = $"IPlayerService/GetOwnedGames/v1/?key={_apiKey}&steamid={steamId}&include_appinfo=true&format=json";
        var response = await _http.GetFromJsonAsync<SteamOwnedGamesResponse>(url);
        return response?.Response?.Games ?? new List<SteamGame>();
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