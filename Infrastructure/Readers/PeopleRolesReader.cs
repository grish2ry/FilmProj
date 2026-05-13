using Domain;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Infrostruture.Readers;
public class PeopleRolesReader
{
    public Dictionary<long, Film> FilmById {get; private set; }
    public Dictionary<long, Person> PersonById {get; private set; }

    public PeopleRolesReader(Dictionary<long, Film> filmById, Dictionary<long, Person> personById)
    {
        FilmById = filmById;
        PersonById = personById;
    }

    public void ReadRoles(string path = "BD\\ml-latest\\ActorsDirectorsCodes_IMDB.tsv")
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

                var filmIdRaw = line.Substring(0, t1);
                var pepIdRaw = line.Substring(t2 + 1, t3 - t2 - 1);
                var role = line.Substring(t3 + 1, t4 - t3 - 1);
                if (!TryParseImdbId(filmIdRaw, out var filmId)) continue;
                if (!TryParsePersonId(pepIdRaw, out var pepId)) continue;

                if (!FilmById.TryGetValue(filmId, out var film)) continue;
                if(!PersonById.TryGetValue(pepId, out var person)) continue;
                switch (role)
                {
                    case "actor":
                    case "actress":
                        person.SetRole(Role.Actor);

                        lock (person)
                        {
                            person.AddFilm(film);
                        }

                        lock (film)
                        {
                            film.Actors?.Add(person);
                        }
                        break;

                    case "director":
                        person.SetRole(Role.Director);

                        lock (person)
                        {
                            person.AddFilm(film);
                        }

                        lock (film)
                        {
                            film.Director = person;
                        }
                        break;
                }
                
            }
        })).ToArray();

        Task.WaitAll(workers.Concat(new[] { reader }).ToArray());
    }

    private static bool TryParseImdbId(string raw, out long id)
    {
        id = 0;
        if (string.IsNullOrWhiteSpace(raw)) return false;
        var s = raw.StartsWith("tt", StringComparison.OrdinalIgnoreCase) ? raw[2..] : raw;
        return long.TryParse(s, out id);
    }

    private static bool TryParsePersonId(string raw, out long id)
    {
        id = 0;
        if (string.IsNullOrWhiteSpace(raw)) return false;
        var s = raw.StartsWith("nm", StringComparison.OrdinalIgnoreCase) ? raw[2..] : raw;
        return long.TryParse(s, out id);
    }
}
