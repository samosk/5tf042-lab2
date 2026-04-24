namespace YourApp.Models;

public class LibraryStats
{
    public int TotalGames { get; set; }
    public double TotalPlaytimeHours { get; set; }
    public int NeverPlayedCount { get; set; }
    public double NeverPlayedPercent { get; set; }
    public int HiddenGemsCount { get; set; }
    public string MostPlayedGenre { get; set; } = "";
    public double MostPlayedGenreHours { get; set; }
    public double AverageRating { get; set; }
    public double TotalCost {get; set; }
    public MashupGame? HighestRatedUnplayed { get; set; }
    public MashupGame? MostPlayed { get; set; }
    public MashupGame? BiggestShame { get; set; } // high rated, zero playtime
    public double TotalLibraryValue { get; set; }
    public double AverageGamePrice { get; set; }
    public string Currency { get; set; } = "";
    public int GamesWithPriceData { get; set; }
}