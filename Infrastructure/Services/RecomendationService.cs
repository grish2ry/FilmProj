using Domain;

namespace Infrostruture.RecomendationServices;

public class RecomendationService
{
    public List<Film> Seen {get; private set;}
    public List<Film> Films {get; private set;}
    private readonly Dictionary<Person, HashSet<Film>> filmsByPerson;
    private readonly Dictionary<Tag, HashSet<Film>> filmsByTag;


    public RecomendationService(List<Film> films)
    {
        Seen = new List<Film>();
        Films = films;
        filmsByPerson = new Dictionary<Person, HashSet<Film>>();
        filmsByTag = new Dictionary<Tag, HashSet<Film>>();
        BuildIndexes();
    }

    public void AddSeenFilms(List<Film> films)
    {
        Seen.AddRange(films);
    }

    public List<Film> Recommend()
    {
        var seenSet = Seen.ToHashSet();
        if (seenSet.Count == 0) return new List<Film>();

        var candidates = BuildCandidates(seenSet);
        if (candidates.Count == 0) return new List<Film>();

        var top = candidates.Select(c => new { Film = c, Score = CalculateScore(c, seenSet) })
        .Where(x => x.Score > 0)
        .OrderByDescending(x => x.Score)
        .Take(10)
        .Select(x => x.Film)
        .ToList();

        return top;
    
    }

    private void BuildIndexes()
    {
        foreach (var film in Films)
        {
            if (film.Director != null)
            {
                AddPersonFilm(film.Director, film);
            }

            foreach (var actor in film.Actors)
            {
                AddPersonFilm(actor, film);
            }

            foreach (var tag in film.Tags.Keys)
            {
                if (!filmsByTag.TryGetValue(tag, out var set))
                {
                    set = new HashSet<Film>();
                    filmsByTag.Add(tag, set);
                }
                set.Add(film);
            }
        }
    }

    private void AddPersonFilm(Person person, Film film)
    {
        if (!filmsByPerson.TryGetValue(person, out var set))
        {
            set = new HashSet<Film>();
            filmsByPerson.Add(person, set);
        }
        set.Add(film);
    }

    private HashSet<Film> BuildCandidates(HashSet<Film> seenSet)
    {
        var candidates = new HashSet<Film>();
        foreach (var seen in seenSet)
        {
            if (seen.Director != null && filmsByPerson.TryGetValue(seen.Director, out var dirFilms))
            {
                foreach (var f in dirFilms) candidates.Add(f);
            }

            foreach (var actor in seen.Actors)
            {
                if (filmsByPerson.TryGetValue(actor, out var actorFilms))
                {
                    foreach (var f in actorFilms) candidates.Add(f);
                }
            }

            foreach (var tag in seen.Tags.Keys)
            {
                if (filmsByTag.TryGetValue(tag, out var tagFilms))
                {
                    foreach (var f in tagFilms) candidates.Add(f);
                }
            }
        }

        candidates.ExceptWith(seenSet);
        return candidates;
    }

    private double CalculateScore(Film candidate, HashSet<Film> seenSet)
    {
        const double actorWeight = 0.35;
        const double directorWeight = 0.25;
        const double tagWeight = 0.30;
        const double ratingWeight = 0.10;

        var bestBaseScore = 0.0;
        foreach (var seen in seenSet)
        {
            var actorScore = CalculateActorScore(seen, candidate);
            var directorScore = seen.Director != null && candidate.Director != null && seen.Director == candidate.Director ? 1.0 : 0.0;
            var tagScore = CalculateTagScore(seen, candidate);
            var baseScore = actorWeight * actorScore + directorWeight * directorScore + tagWeight * tagScore;
            if (baseScore > bestBaseScore) bestBaseScore = baseScore;
        }

        var ratingScore = Math.Clamp(candidate.Rating / 10.0, 0, 1);
        var total = bestBaseScore + ratingWeight * ratingScore;
        return Math.Clamp(total, 0, 1);
    }

    private static double CalculateActorScore(Film a, Film b)
    {
        if (a.Actors.Count == 0 || b.Actors.Count == 0) return 0;
        var common = a.Actors.Count(actor => b.Actors.Contains(actor));
        var maxCount = Math.Max(a.Actors.Count, b.Actors.Count);
        return maxCount == 0 ? 0 : (double)common / maxCount;
    }

    private static double CalculateTagScore(Film a, Film b)
    {
        if (a.Tags.Count == 0 || b.Tags.Count == 0) return 0;

        var commonTags = a.Tags.Keys.Where(t => b.Tags.ContainsKey(t)).ToList();
        if (commonTags.Count == 0) return 0;

        double intersectWeight = 0;
        foreach (var tag in commonTags)
        {
            intersectWeight += Math.Min(a.Tags[tag], b.Tags[tag]);
        }

        var unionTags = a.Tags.Keys.Union(b.Tags.Keys);
        double unionWeight = 0;
        foreach (var tag in unionTags)
        {
            var av = a.Tags.TryGetValue(tag, out var aVal) ? aVal : 0;
            var bv = b.Tags.TryGetValue(tag, out var bVal) ? bVal : 0;
            unionWeight += Math.Max(av, bv);
        }

        return unionWeight == 0 ? 0 : intersectWeight / unionWeight;
    }
}
