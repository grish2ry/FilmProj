using Domain;
using System.Globalization;
using System.Collections.Concurrent;
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
        var lines = new BlockingCollection<string>(10000);
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
                    var t1 = l.IndexOf('\t');
                    if (t1 < 0) continue;

                    if (!TryParseImdbId(l.AsSpan(0, t1), out var code)) continue;
                    if (!double.TryParse(l.AsSpan(t1 + 1), NumberStyles.Float, CultureInfo.InvariantCulture, out var rating))
                    {
                        continue;
                    }

                    if (MovieCodes.TryGetValue(code, out var film))
                    {
                        lock (film)
                        {
                            film.Rating = rating;
                        }
                    }
                }
            })).ToArray();

        Task.WaitAll(workers.Concat(new[] { reader }).ToArray());
    }

    private static bool TryParseImdbId(ReadOnlySpan<char> raw, out long id)
    {
        id = 0;
        if (raw.IsEmpty) return false;
        var s = raw.StartsWith("tt".AsSpan(), StringComparison.OrdinalIgnoreCase) ? raw[2..] : raw;
        return long.TryParse(s, out id);
    }
}
