using System.Text.Json.Serialization;

namespace YourApp.Models;

// Top-level response from /api/games
public class RawgSearchResponse
{
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("next")]
    public string? Next { get; set; }

    [JsonPropertyName("previous")]
    public string? Previous { get; set; }

    [JsonPropertyName("results")]
    public List<RawgGame> Results { get; set; } = new();
}

// A single game in the results array
public class RawgGame
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("slug")]
    public string Slug { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("released")]
    public string? Released { get; set; }  // "YYYY-MM-DD" or null

    [JsonPropertyName("tba")]
    public bool Tba { get; set; }

    [JsonPropertyName("background_image")]
    public string? BackgroundImage { get; set; }

    [JsonPropertyName("rating")]
    public double Rating { get; set; }  // RAWG's 0–5 community rating

    [JsonPropertyName("rating_top")]
    public int RatingTop { get; set; }

    [JsonPropertyName("ratings_count")]
    public int RatingsCount { get; set; }

    [JsonPropertyName("metacritic")]
    public int? Metacritic { get; set; }  // 0–100, nullable

    [JsonPropertyName("playtime")]
    public int Playtime { get; set; }  // average hours

    [JsonPropertyName("added")]
    public int Added { get; set; }  // how many RAWG users have it in their library

    [JsonPropertyName("updated")]
    public DateTime? Updated { get; set; }

    [JsonPropertyName("esrb_rating")]
    public RawgEsrbRating? EsrbRating { get; set; }

    [JsonPropertyName("genres")]
    public List<RawgGenre> Genres { get; set; } = new();

    [JsonPropertyName("tags")]
    public List<RawgTag> Tags { get; set; } = new();

    [JsonPropertyName("platforms")]
    public List<RawgPlatformWrapper> Platforms { get; set; } = new();

    [JsonPropertyName("parent_platforms")]
    public List<RawgPlatformWrapper> ParentPlatforms { get; set; } = new();

    [JsonPropertyName("stores")]
    public List<RawgStoreWrapper>? Stores { get; set; }

    [JsonPropertyName("short_screenshots")]
    public List<RawgScreenshot> ShortScreenshots { get; set; } = new();
}

// Nested types — RAWG wraps platforms and stores in an extra object
public class RawgPlatformWrapper
{
    [JsonPropertyName("platform")]
    public RawgPlatform Platform { get; set; } = new();
}

public class RawgPlatform
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("slug")]
    public string Slug { get; set; } = "";
}

public class RawgStoreWrapper
{
    [JsonPropertyName("store")]
    public RawgStore Store { get; set; } = new();
}

public class RawgStore
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("slug")]
    public string Slug { get; set; } = "";
}

public class RawgGenre
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("slug")]
    public string Slug { get; set; } = "";
}

public class RawgTag
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("slug")]
    public string Slug { get; set; } = "";

    [JsonPropertyName("language")]
    public string Language { get; set; } = "";

    [JsonPropertyName("games_count")]
    public int GamesCount { get; set; }
}

public class RawgScreenshot
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("image")]
    public string Image { get; set; } = "";
}

public class RawgEsrbRating
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("slug")]
    public string Slug { get; set; } = "";
}