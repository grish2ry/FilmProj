using Domain;
using Presentation;
using Infrostruture.Readers;
using Infrostruture.RecomendationServices;
using Infrostruture.Api;
public class App
{
    public Dictionary<string, List<Film>> PersonInFilms {get; private set;}
    public Dictionary<string, List<Film>> FilmByTag {get; private set;}
    public Dictionary<string, List<Film>> FilmByName {get; private set;}
    public HashSet<Film> Films {get; private set; }

    public ImdbDescriptionService DescriptionService {get; private set;}

    private RecomendationService recomendationService;
    private ConsoleUi ui;
    public App()
    {
        HttpClient c = new HttpClient();
        DescriptionService = new ImdbDescriptionService(c, "6e7cf0d1");
        
        ui = new ConsoleUi();
        
        Console.WriteLine("ajkbsd");
        var filmById = new Dictionary<long, Film>();

        Console.WriteLine("ajkbsd");
        var movieImbdReader = new MovieImbdReader(filmById);
        movieImbdReader.ReadImbd();


        Console.WriteLine("ajkbsd");
        var raitingReader = new RaitingReader(filmById);
        raitingReader.ReadRaiting();


        Console.WriteLine("ajkbsd");
        var peopleNamesReader = new PeopleNamesReader(filmById);
        peopleNamesReader.ReadNames();


        Console.WriteLine("ajkbsd");
        var peopleRolesReader = new PeopleRolesReader(filmById, peopleNamesReader.PersonById);
        peopleRolesReader.ReadRoles();

        Console.WriteLine("ajkbsd");

        var imbdToMovieCode = new ImbdToMovieCode(filmById);
        imbdToMovieCode.Convert();

        Console.WriteLine("ajkbsd");

        var tagCodeReader = new TagCodeReader();
        tagCodeReader.ReadTags();

        Console.WriteLine("ajkbsd");

        var tagsApplyer = new TagsApplyer(tagCodeReader.TagCode, filmById, imbdToMovieCode.CodeConvertor);
        tagsApplyer.ApplyTags();

        Films = new HashSet<Film>(filmById.Values);

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
        ui.DisplayMenu();
        
        while (true)
        {
            

            var s = Console.ReadKey(true);
            switch (s.KeyChar)
            {
                case '1':
                    ui.FilmByPerson();
                    var name = (Console.ReadLine() ?? string.Empty).Trim();
                    if (PersonInFilms.TryGetValue(name, out var f)){
                        recomendationService.AddSeenFilms(f);
                        ui.PrintFilms(f);
                    }
                    else
                    {
                        Console.WriteLine("Person not found");
                    }
                    Thread.Sleep(3000);
                    Console.Clear();
                    ui.DisplayMenu();


                    break;
                case '2':
                    ui.FilmByTag();
                    var tag = (Console.ReadLine() ?? string.Empty).Trim();
                    if (FilmByTag.TryGetValue(tag, out var films))
                    {
                        recomendationService.AddSeenFilms(films);
                        ui.PrintFilms(films);
                    }
                    else
                    {
                        Console.WriteLine("Tag not found");
                    }
                    Thread.Sleep(3000);
                    Console.Clear();
                    ui.DisplayMenu();

                    break;
                case '3':
                    ui.FilmInfo();
                    var film = (Console.ReadLine() ?? string.Empty).Trim();
                    Film? fi = null;
                    if (FilmByName.TryGetValue(film, out var filmMatches))
                    {
                        fi = filmMatches.FirstOrDefault();
                    }
                    if (fi == null)
                    {
                        var key = FilmByName.Keys.FirstOrDefault(k =>
                            k.Contains(film, StringComparison.OrdinalIgnoreCase));
                        if (key != null)
                        {
                            fi = FilmByName[key].FirstOrDefault();
                        }
                    }
                    if (fi == null)
                    {
                        Console.WriteLine("Film not found");
                        Thread.Sleep(3000);
                        Console.Clear();
                        ui.DisplayMenu();
                        break;
                    }

                    fi.Display(ui);
                    var description = DescriptionService.GetDescription($"tt{fi.ImdbId:D7}").GetAwaiter().GetResult();
                    if (string.IsNullOrWhiteSpace(description))
                    {
                        Console.WriteLine("Description not available");
                    }
                    else
                    {
                        Console.WriteLine(description);
                    }
                    recomendationService.AddSeenFilms(new List<Film> { fi });
                    
                    Thread.Sleep(3000);
                    Console.Clear();
                    ui.DisplayMenu();
                    break;
                case '4':
                    ui.RecomendationMenu();
                    
                    var recommend = recomendationService.Recommend();
                    ui.PrintFilms(recommend);
                    Thread.Sleep(3000);
                    Console.Clear();
                    ui.DisplayMenu();
                    break;
                case '5':
                    return;
                default:
                    break;
            }
            
        }
    }
}
