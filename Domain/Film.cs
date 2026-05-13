using Presentation;
namespace Domain;

public class Film
{
    public HashSet<Person> Actors { get; private set; }
    public Person Director {get; set;}
    public Dictionary<Tag, double> Tags { get; private set; }
    public HashSet<string> Aliases { get; private set; }
    public string Title { get; private set; }
    public long ImdbId { get; private set; }
    public double Rating { get; set; }

    public Film(string title, long ImbId)
    {
        Title = title;
        ImdbId = ImbId;
        Actors = new HashSet<Person>();
        Tags = new Dictionary<Tag, double>();
        Aliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        AddAlias(title);
    }
    public void AddTag(Tag tag, double relevance)
    {
        Tags.Add(tag, relevance);
    }

    public void SetTitle(string title)
    {
        if (!string.IsNullOrWhiteSpace(title))
        {
            Title = title;
            AddAlias(title);
        }
    }

    public void AddAlias(string alias)
    {
        if (!string.IsNullOrWhiteSpace(alias) && alias != "\\N")
        {
            Aliases.Add(alias.Trim());
        }
    }

    public void Display(ConsoleUi ui)
    {
        Console.WriteLine(Title);
        Console.WriteLine(Rating);

        Console.WriteLine("Director:");
        if (Director != null)
            Console.WriteLine($"{Director.Name} {Director.Surname}".Trim());
        else
            Console.WriteLine("No data");

        Console.WriteLine("Actors:");
        if (Actors != null && Actors.Count > 0)
            ui.PrintActors(Actors);
        else
            Console.WriteLine("No data");
    }

};
