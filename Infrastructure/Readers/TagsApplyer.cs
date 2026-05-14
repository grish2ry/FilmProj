using Domain;
using System.Globalization;
using System.Collections.Concurrent;
namespace Infrostruture.Readers;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

public class TagsApplyer
{
    public Dictionary<int, Tag> TagCode {get; private set;}

    public Dictionary<int, long> MovieCodeToImbd {get; private set;}

    public Dictionary<long, Film> FilmById {get; private set;}

    public TagsApplyer(Dictionary<int, Tag> tagCode, Dictionary<long, Film> filmById, Dictionary<int, long> movieCodeImbd)
    {
        TagCode = tagCode;
        FilmById = filmById;
        MovieCodeToImbd = movieCodeImbd;
    }

    public void ApplyTags(string path = "BD\\ml-latest\\TagScores_MovieLens.csv")
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
                    var c1 = l.IndexOf(',');
                    if (c1 < 0) continue;
                    var c2 = l.IndexOf(',', c1 + 1);
                    if (c2 < 0) continue;

                    if (!int.TryParse(l.AsSpan(0, c1), out var movieId)) continue;
                    if (!int.TryParse(l.AsSpan(c1 + 1, c2 - c1 - 1), out var tagId)) continue;
                    if (!double.TryParse(l.AsSpan(c2 + 1), NumberStyles.Float, CultureInfo.InvariantCulture, out double r)) continue;
                    if (r <= 0.5) continue;
                    if (!MovieCodeToImbd.TryGetValue(movieId, out var imdbId)) continue;
                    if (!FilmById.TryGetValue(imdbId, out var film)) continue;
                    if (!TagCode.TryGetValue(tagId, out var tag)) continue;
                    lock (film)
                    {
                        if (!film.Tags.ContainsKey(tag))
                        {
                            film.AddTag(tag, r);
                        }
                    }
                }
            })).ToArray();

        Task.WaitAll(workers.Concat(new[] { reader }).ToArray());
    }
}
