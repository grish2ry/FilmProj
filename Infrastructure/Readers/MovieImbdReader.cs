using Domain;
namespace Infrostruture.Readers;
public class MovieImbdReader
{
    public Dictionary<long, Film> MovieCodes { get; private set; }

    public MovieImbdReader(Dictionary<long, Film> movieCodes)
    {
        MovieCodes = movieCodes;
    }

    public void ReadImbd(string path = "BD\\ml-latest\\MovieCodes_IMDB.tsv")
    {
        foreach(var l in  File.ReadLines(path).Skip(1))
        {
            var parts = l.Split('\t');
            if(parts.Length < 5) continue;

            var region = parts[3];
            var lang = parts[4];
            var isRussian = region == "RU" || lang == "ru";
            var isEnglish = region == "US" || region == "GB" || lang == "en";
            if (!isRussian && !isEnglish) continue;
            var name = parts[2];
            if (string.IsNullOrWhiteSpace(name) || name == "\\N") continue;
            if (!TryParseImdbId(parts[0], out var code)) continue;
            
            if (!MovieCodes.TryGetValue(code, out var film))
            {
                film = new Film(name, code);
                MovieCodes.Add(code, film);
            }

            film.AddAlias(name);
            if (isRussian)
            {
                film.SetTitle(name);
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
