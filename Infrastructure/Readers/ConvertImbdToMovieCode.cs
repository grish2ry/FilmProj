using Domain;
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
        foreach(var l in File.ReadLines(path).Skip(1))
        {
            var parts = l.Split(',');
            if(parts.Length < 3) continue;
            if (!int.TryParse(parts[0], out var movieID)) continue;
            if (!long.TryParse(parts[1], out var imdbId)) continue;
            if (FilmById.ContainsKey(imdbId))
            {
                if (!CodeConvertor.ContainsKey(movieID))
                    CodeConvertor.Add(movieID, imdbId);
            }
        }
    }
}
