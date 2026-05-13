using Domain;
using System.Globalization;
namespace Infrostruture.Readers;
public class RaitingReader
{
    public Dictionary<long, Film> MovieCodes { get; private set; }

    public RaitingReader(Dictionary<long, Film> movieCodes)
    {
        MovieCodes = movieCodes;
    }

    public void ReadRaiting(string path = "BD\\ml-latest\\Ratings_IMDB.tsv")
    {
        foreach(var l in  File.ReadLines(path).Skip(1))
        {
            var parts = l.Split('\t');
            if (parts.Length < 2) continue;

            if (!TryParseImdbId(parts[0], out var code)) continue;
            if (!double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var rating))
            {
                continue;
            }

            if (MovieCodes.TryGetValue(code, out var film))
            {
                film.Rating = rating;
            }
        }
    }

    private static bool TryParseImdbId(string raw, out long id)
    {
        id = 0;
        if (string.IsNullOrWhiteSpace(raw)) return false;
        var s = raw.StartsWith("tt", StringComparison.OrdinalIgnoreCase) ? raw[2..] : raw;
        return long.TryParse(s, out id);
    }
}

