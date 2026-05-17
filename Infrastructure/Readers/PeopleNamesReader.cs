using Domain;
using System.Collections.Concurrent;
using System.Threading.Tasks;
namespace Infrostruture.Readers;
public class PeopleNamesReader
{
    public Dictionary<long, Person> PersonById {get; private set; }

    public Dictionary<long, Film> FilmById {get; private set; }

    public PeopleNamesReader(Dictionary<long, Film> filmById)
    {
        PersonById = new Dictionary<long, Person>();
        FilmById = filmById;
    }

    public void ReadNames(string path = "BD\\ml-latest\\ActorsDirectorsNames_IMDB.txt")
    {
        var lines = new BlockingCollection<string>(10000);
        var dict = new ConcurrentDictionary<long, Person>();
  

        var reader = Task.Run(() =>
        {
            foreach (var line in File.ReadLines(path).Skip(1))
                lines.Add(line);
            lines.CompleteAdding();
        });

        var workers = Enumerable.Range(0, Environment.ProcessorCount)
        .Select(_ => Task.Run(() =>
        {
            foreach (var line in lines.GetConsumingEnumerable())
            {
                var span = line.AsSpan();
                var t1 = line.IndexOf('\t');
                if (t1 < 0) continue;

                var t2 = line.IndexOf('\t', t1 + 1);
                if (t2 < 0) continue;

                var t3 = line.IndexOf('\t', t2 + 1);
                if (t3 < 0) continue;

                var t4 = line.IndexOf('\t', t3 + 1);
                if (t4 < 0) continue;

                var t5 = line.IndexOf('\t', t4 + 1);
                if (t5 < 0) continue;

                if (!TryParsePersonId(span.Slice(0, t1), out var id)) continue;

                var fullName = span.Slice(t1 + 1, t2 - t1 - 1);
                if (fullName.IsEmpty) continue;

                var t6 = fullName.IndexOf(' ');
                var name = (t6 > 0 ? fullName.Slice(0, t6) : fullName).ToString();
                var surname = t6 > 0 ? fullName.Slice(t6 + 1).ToString() : string.Empty;
                dict.GetOrAdd(id, _ => new Person(name, surname, id));


            }
        })).ToArray();

        Task.WaitAll(workers.Concat(new[] { reader }).ToArray());
        PersonById = dict.ToDictionary(kv => kv.Key, kv => kv.Value);



    }

    private static bool TryParsePersonId(ReadOnlySpan<char> raw, out long id)
    {
        id = 0;
        if (raw.IsEmpty) return false;
        var s = raw.StartsWith("nm".AsSpan(), StringComparison.OrdinalIgnoreCase) ? raw[2..] : raw;
        return long.TryParse(s, out id);
    }


}
