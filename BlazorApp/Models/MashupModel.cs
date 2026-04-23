namespace YourApp.Models;

public class MashupGame
{
    // Steam data
    public int SteamAppId { get; set; }
    public string Name { get; set; } = "";
    public double PlaytimeHours { get; set; }
    public string IconUrl { get; set; } = "";

    // RAWG data
    public double? RawgRating { get; set; }       // 0–5
    public int? Metacritic { get; set; }           // 0–100
    public int? AveragePlaytime { get; set; }      // hours (community average)
    public string? BackgroundImage { get; set; }
    public List<string> Genres { get; set; } = new();

    // Mashup logic — the "new thing" you're creating
    public double? ValueScore { get; set; }
    public string Recommendation { get; set; } = "";
}