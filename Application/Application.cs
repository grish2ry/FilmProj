using Domain;
using Presentation;
using Infrostruture.RecomendationServices;
using Infrostruture.Api;
using Infrostruture.Loggers;
using AppFilm.Helpers;
namespace AppFilm;
public class App
{
    public Dictionary<string, List<Film>> PersonInFilms {get; private set;}
    public Dictionary<string, List<Film>> FilmByTag {get; private set;}
    public Dictionary<string, List<Film>> FilmByName {get; private set;}
    public HashSet<Film> Films {get; private set; }
    public IDescriptionService DescriptionService {get; private set;}

    public IAppLogger Logger {get; private set;}

    public AppHelper Helper {get; private set;}

    private RecomendationService recomendationService;
    private IUi Ui;
    public App(
        IUi ui,
        IAppLogger logger,
        IDescriptionService descriptionService,
        AppHelper helper,
        HashSet<Film> films)
    {
        Ui = ui;
        Logger = logger;
        DescriptionService = descriptionService;
        Helper = helper;
        Films = films;

        recomendationService = new RecomendationService(Films.ToList());

        PersonInFilms = new Dictionary<string, List<Film>>(StringComparer.OrdinalIgnoreCase);
        FilmByTag = new Dictionary<string, List<Film>>(StringComparer.OrdinalIgnoreCase);
        FilmByName = new Dictionary<string, List<Film>>(StringComparer.OrdinalIgnoreCase);
        BuildIndexes();
    }

    public void BuildIndexes()
    {
        PersonInFilms.Clear();
        FilmByTag.Clear();
        FilmByName.Clear();

        foreach (var film in Films)
        {
            if (!FilmByName.TryGetValue(film.Title, out var titleFilms))
            {
                titleFilms = new List<Film>();
                FilmByName.Add(film.Title, titleFilms);
            }
            if (!titleFilms.Contains(film))
            {
                titleFilms.Add(film);
            }

            foreach (var alias in film.Aliases)
            {
                if (!FilmByName.TryGetValue(alias, out var aliasFilms))
                {
                    aliasFilms = new List<Film>();
                    FilmByName.Add(alias, aliasFilms);
                }
                if (!aliasFilms.Contains(film))
                {
                    aliasFilms.Add(film);
                }
            }

            if (film.Director != null)
            {
                var directorKey = $"{film.Director.Name} {film.Director.Surname}".Trim();
                if (!PersonInFilms.TryGetValue(directorKey, out var directorFilms))
                {
                    directorFilms = new List<Film>();
                    PersonInFilms.Add(directorKey, directorFilms);
                }

                if (!directorFilms.Contains(film))
                {
                    directorFilms.Add(film);
                }
            }

            if (film.Actors != null)
            {
                foreach (var actor in film.Actors)
                {
                    var actorKey = $"{actor.Name} {actor.Surname}".Trim();
                    if (!PersonInFilms.TryGetValue(actorKey, out var actorFilms))
                    {
                        actorFilms = new List<Film>();
                        PersonInFilms.Add(actorKey, actorFilms);
                    }

                    if (!actorFilms.Contains(film))
                    {
                        actorFilms.Add(film);
                    }
                }
            }

            if (film.Tags != null)
            {
                foreach (var tagPair in film.Tags)
                {
                    var tagKey = tagPair.Key.TagCtx;
                    if (!FilmByTag.TryGetValue(tagKey, out var tagFilms))
                    {
                        tagFilms = new List<Film>();
                        FilmByTag.Add(tagKey, tagFilms);
                    }
                    if (!tagFilms.Contains(film))
                    {
                        tagFilms.Add(film);
                    }
                }
            }
        }
    }
    public void Run()
    {
        Ui.DisplayMenu();        
        while (true)
        {
            var s = Console.ReadKey(true);
            switch (s.KeyChar)
            {
                case '1':
                    Ui.FilmByPerson();
                    var name = (Console.ReadLine() ?? string.Empty).Trim();
                    if (PersonInFilms.TryGetValue(name, out var f)){
                        recomendationService.AddSeenFilms(f);
                        Ui.PrintFilms(f);
                    }
                    else
                    {
                        Logger.LogErrorMessage("Person not found");
                    }
                    Thread.Sleep(3000);
                    Ui.DisplayMenu();


                    break;
                case '2':
                    Ui.FilmByTag();
                    var tag = (Console.ReadLine() ?? string.Empty).Trim();
                    if (FilmByTag.TryGetValue(tag, out var films))
                    {
                        recomendationService.AddSeenFilms(films);
                        Ui.PrintFilms(films);
                    }
                    else
                    {
                        Logger.LogErrorMessage("Tag not found");
                    }
                    Thread.Sleep(3000);
                    Ui.DisplayMenu();

                    break;
                case '3':
                    Ui.FilmInfo();
                    var film = (Console.ReadLine() ?? string.Empty).Trim();
                    Film? fi = null;
                    if (FilmByName.TryGetValue(film, out var filmMatches))
                    {
                        fi = filmMatches.FirstOrDefault();
                    }
                    if (fi == null)
                    {
                        var key = FilmByName.Keys.FirstOrDefault(k => k.Contains(film, StringComparison.OrdinalIgnoreCase));
                        if (key != null)
                        {
                            fi = FilmByName[key].FirstOrDefault();
                        }
                    }
                    if (fi == null)
                    {
                        Logger.LogErrorMessage("Film not found");
                        Thread.Sleep(3000);
                        Ui.DisplayMenu();
                        break;
                    }

                    fi.Display(Ui);
                    var description = DescriptionService.GetDescription($"tt{fi.ImdbId:D7}").GetAwaiter().GetResult();
                    if (string.IsNullOrWhiteSpace(description))
                    {
                        Logger.LogErrorMessage("Description not available");
                    }
                    else
                    {
                        Ui.PrintSingle(description);
                    }
                    recomendationService.AddSeenFilms(new List<Film> { fi });
                    
                    Thread.Sleep(3000);
                    Ui.DisplayMenu();
                    break;
                case '4':
                    Ui.RecomendationMenu();
                    
                    var recommend = recomendationService.Recommend();
                    Ui.PrintFilms(recommend);
                    Thread.Sleep(3000);
                    Ui.DisplayMenu();
                    break;
                case '5':
                    return;
                default:
                    break;
            }
            
        }
    }
}
