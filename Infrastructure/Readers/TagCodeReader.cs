using Domain;
using System.Collections.Concurrent;
namespace Infrostruture.Readers;
public class TagCodeReader{
    public Dictionary<int, Tag> TagCode {get; private set; }

    public TagCodeReader()
    {
        TagCode = new Dictionary<int, Tag>();
    }

    public void ReadTags(string path = "BD\\ml-latest\\TagCodes_MovieLens.csv")
    {
        var lines = new BlockingCollection<string>(10000);
        var dict = new ConcurrentDictionary<int, Tag>();

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
                    if (!int.TryParse(l.AsSpan(0, c1), out var tagId)) continue;
                    var tagCtx = l[(c1 + 1)..];
                    dict.TryAdd(tagId, new Tag(tagCtx));
                }
            })).ToArray();

        Task.WaitAll(workers.Concat(new[] { reader }).ToArray());
        TagCode = dict.ToDictionary(kv => kv.Key, kv => kv.Value);
    }
}
