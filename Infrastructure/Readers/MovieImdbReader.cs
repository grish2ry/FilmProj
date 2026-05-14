using Domain;
using System.Collections.Concurrent;
namespace Infrostruture.Readers;
public class MovieImdbReader
{
    public Dictionary<long, Film> MovieCodes { get; private set; }

    public MovieImdbReader(Dictionary<long, Film> movieCodes)
    {
        MovieCodes = movieCodes;
    }

    public void ReadImdb(string path = "BD\\ml-latest\\MovieCodes_IMDB.tsv")
    {
        var lines = new BlockingCollection<string>(10000);
        var dict = new ConcurrentDictionary<long, Film>();

        var reader = Task.Run(() =>
        {
            foreach (var line in File.ReadLines(path).Skip(1))
                lines.Add(line);
            lines.CompleteAdding();
        });

        var workers = Enumerable.Range(0, Environment.ProcessorCount)
        .Select(_ => Task.Run(() =>
        {
            foreach (var l in lines.GetConsumingEnumerable())
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
                
                var film = dict.GetOrAdd(code, _ => new Film(name, code));
                lock (film)
                {
                    film.AddAlias(name);
                    if (isRussian)
                    {
                        film.SetTitle(name);
                    }
                }

            }
        })).ToArray();

        Task.WaitAll(workers.Concat(new[] { reader }).ToArray());
        MovieCodes = dict.ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    private static bool TryParseImdbId(string raw, out long id)
    {
        id = 0;
        if (string.IsNullOrWhiteSpace(raw)) return false;
        var s = raw.StartsWith("tt", StringComparison.OrdinalIgnoreCase) ? raw[2..] : raw;
        return long.TryParse(s, out id);
    }
}
