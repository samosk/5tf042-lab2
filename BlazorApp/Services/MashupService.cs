using YourApp.Models;

public class MashupService
{
    private readonly SteamService _steam;
    private readonly RawgService _rawg;

    public MashupService(SteamService steam, RawgService rawg)
    {
        _steam = steam;
        _rawg = rawg;
    }

    public async Task<List<MashupGame>> GetAnalyzedLibraryAsync(string steamId)
    {
        var steamGames = await _steam.GetOwnedGamesAsync(steamId);

        var gamesToAnalyze = steamGames
            .OrderByDescending(g => g.PlaytimeForever)
            .Take(25)
            .ToList();

        var results = new List<MashupGame>();
        var batchSize = 5;

        // Process in batches of 5 parallel requests
        for (int i = 0; i < gamesToAnalyze.Count; i += batchSize)
        {
            var batch = gamesToAnalyze.Skip(i).Take(batchSize);

            var tasks = batch.Select(async sg =>
            {
                var mashup = new MashupGame
                {
                    SteamAppId = sg.AppId,
                    Name = sg.Name,
                    PlaytimeHours = sg.PlaytimeHours,
                    IconUrl = sg.IconFullUrl
                };

                try
                {
                    var rawgResult = await _rawg.SearchGamesAsync(sg.Name);
                    var match = FindBestMatch(rawgResult, sg.Name);

                    if (match is not null)
                    {
                        mashup.RawgRating = match.Rating;
                        mashup.Metacritic = match.Metacritic;
                        mashup.AveragePlaytime = match.Playtime;
                        mashup.BackgroundImage = match.BackgroundImage;
                        mashup.Genres = match.Genres.Select(g => g.Name).ToList();
                    }
                }
                catch
                {
                    // RAWG lookup failed — continue with Steam data only
                }

                mashup.ValueScore = CalculateValueScore(mashup);
                mashup.Recommendation = GenerateRecommendation(mashup);
                return mashup;
            });

            var batchResults = await Task.WhenAll(tasks);
            results.AddRange(batchResults);
        }

        return results.OrderByDescending(g => g.ValueScore ?? 0).ToList();
    }

    private RawgGame? FindBestMatch(RawgSearchResponse? response, string steamName)
    {
        if (response?.Results is null || response.Results.Count == 0)
            return null;

        // Try exact match first (case-insensitive)
        var exact = response.Results.FirstOrDefault(
            g => g.Name.Equals(steamName, StringComparison.OrdinalIgnoreCase));

        if (exact is not null)
            return exact;

        // Fall back to first result with a decent rating count
        // This avoids matching against obscure itch.io clones
        return response.Results
            .Where(g => g.RatingsCount > 5)
            .FirstOrDefault();
    }

    private double? CalculateValueScore(MashupGame game)
    {
        // Need at least some RAWG data to calculate
        if (game.RawgRating is null || game.RawgRating == 0)
            return null;

        double score = 0;

        // Base: RAWG rating (0–5) normalized to 0–50
        score += game.RawgRating.Value * 10;

        // Metacritic bonus (0–100) normalized to 0–30
        if (game.Metacritic.HasValue)
            score += game.Metacritic.Value * 0.3;

        // Unplayed high-rated game = big bonus (the hidden gem factor)
        if (game.PlaytimeHours == 0 && game.RawgRating > 3.5)
            score += 15;

        // Underplayed relative to community average
        if (game.AveragePlaytime.HasValue && game.AveragePlaytime > 0)
        {
            double playedRatio = game.PlaytimeHours / game.AveragePlaytime.Value;
            if (playedRatio < 0.25)
                score += 10;  // You've barely scratched the surface
        }

        return Math.Round(score, 1);
    }

    private string GenerateRecommendation(MashupGame game)
    {
        if (game.ValueScore is null)
            return "No rating data available";

        if (game.PlaytimeHours == 0 && game.RawgRating > 4.0)
            return "Hidden gem — you own this but haven't tried it!";

        if (game.PlaytimeHours == 0 && game.RawgRating > 3.0)
            return "Worth trying — decent ratings, give it a shot";

        if (game.PlaytimeHours > 0 && game.AveragePlaytime.HasValue
            && game.PlaytimeHours < game.AveragePlaytime.Value * 0.25
            && game.RawgRating > 3.5)
            return "Underplayed — you might be missing out";

        if (game.PlaytimeHours > 100 && game.RawgRating > 4.0)
            return "All-time favorite";

        if (game.RawgRating > 4.0)
            return "Highly rated";

        if (game.RawgRating < 2.5)
            return "Low rated — maybe skip this one";

        return "Solid game";
    }
    public LibraryStats CalculateStats(List<MashupGame> games)
    {
        var stats = new LibraryStats
        {
            TotalGames = games.Count,
            TotalPlaytimeHours = Math.Round(games.Sum(g => g.PlaytimeHours), 1),
            NeverPlayedCount = games.Count(g => g.PlaytimeHours == 0),
            MostPlayed = games.OrderByDescending(g => g.PlaytimeHours).FirstOrDefault(),
        };

        stats.NeverPlayedPercent = stats.TotalGames > 0
            ? Math.Round(100.0 * stats.NeverPlayedCount / stats.TotalGames, 1)
            : 0;

        // Average rating from games that have RAWG data
        var ratedGames = games.Where(g => g.RawgRating.HasValue && g.RawgRating > 0).ToList();
        stats.AverageRating = ratedGames.Any()
            ? Math.Round(ratedGames.Average(g => g.RawgRating!.Value), 2)
            : 0;

        // Hidden gems: unplayed + well-rated
        stats.HiddenGemsCount = games.Count(g =>
            g.PlaytimeHours == 0 && g.RawgRating > 3.5);

        // Highest rated game you haven't played
        stats.HighestRatedUnplayed = games
            .Where(g => g.PlaytimeHours == 0 && g.RawgRating.HasValue)
            .OrderByDescending(g => g.RawgRating)
            .ThenByDescending(g => g.Metacritic ?? 0)
            .FirstOrDefault();

        // Biggest shame: highest metacritic/rating with zero playtime
        stats.BiggestShame = games
            .Where(g => g.PlaytimeHours == 0 && g.Metacritic.HasValue)
            .OrderByDescending(g => g.Metacritic)
            .FirstOrDefault();

        // Most played genre
        var genreGroups = games
            .SelectMany(g => g.Genres.Select(genre => new { Genre = genre, g.PlaytimeHours }))
            .GroupBy(x => x.Genre)
            .Select(g => new
            {
                Genre = g.Key,
                TotalHours = Math.Round(g.Sum(x => x.PlaytimeHours), 1)
            })
            .OrderByDescending(g => g.TotalHours)
            .FirstOrDefault();

        if (genreGroups is not null)
        {
            stats.MostPlayedGenre = genreGroups.Genre;
            stats.MostPlayedGenreHours = genreGroups.TotalHours;
        }

        return stats;
    }
}