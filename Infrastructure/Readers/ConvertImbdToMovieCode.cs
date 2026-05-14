using Domain;
using System.Collections.Concurrent;
namespace Infrostruture.Readers;
public class ImbdToMovieCode
{
    public Dictionary<long, Film> FilmById {get; private set; }

    public Dictionary<int, long> CodeConvertor {get; private set;}

    public ImbdToMovieCode(Dictionary<long, Film> filmById)
    {
        FilmById = filmById;
        CodeConvertor = new Dictionary<int, long>();
    }

    public void Convert(string path = "BD\\ml-latest\\links_IMDB_MovieLens.csv")
    {
        var lines = new BlockingCollection<string>(10000);
        var dict = new ConcurrentDictionary<int, long>();

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
                    var c1 = l.IndexOf(',');
                    if (c1 < 0) continue;
                    var c2 = l.IndexOf(',', c1 + 1);
                    if (c2 < 0) continue;

                    if (!int.TryParse(l.AsSpan(0, c1), out var movieID)) continue;
                    if (!long.TryParse(l.AsSpan(c1 + 1, c2 - c1 - 1), out var imdbId)) continue;
                    if (FilmById.ContainsKey(imdbId))
                    {
                        dict.TryAdd(movieID, imdbId);
                    }
                }
            })).ToArray();

        Task.WaitAll(workers.Concat(new[] { reader }).ToArray());
        CodeConvertor = dict.ToDictionary(kv => kv.Key, kv => kv.Value);
    }
}
