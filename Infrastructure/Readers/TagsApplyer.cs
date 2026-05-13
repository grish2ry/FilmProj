using Domain;
using System.Globalization;
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
        foreach(var l in File.ReadLines(path).Skip(1))
        {
            var parts = l.Split(',');
            if (parts.Length < 3) continue;
            if (!int.TryParse(parts[0], out var movieId)) continue;
            if (!int.TryParse(parts[1], out var tagId)) continue;
            var relevance = parts[2];
            if (!double.TryParse(relevance, NumberStyles.Float, CultureInfo.InvariantCulture, out double r)) continue;
            if (r <= 0.5) continue;
            if (!MovieCodeToImbd.TryGetValue(movieId, out var imdbId)) continue;
            if (!FilmById.TryGetValue(imdbId, out var film)) continue;
            if (!TagCode.TryGetValue(tagId, out var tag)) continue;

            film.AddTag(tag, r);
        }
    }
}
