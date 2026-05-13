namespace Presentation;
using Domain;
public class ConsoleUi
{
    public void DisplayMenu()
    {
        Console.WriteLine("--------MENU--------");
        Console.WriteLine("1 - Film by Person");
        Console.WriteLine("2 - Film by Tag");
        Console.WriteLine("3 - Film info");
        Console.WriteLine("4 - Recommend");
        Console.WriteLine("5 - Exit");
        Console.WriteLine("---------------------");
    }

    public void FilmByPerson()
    {
        Console.Clear();
        Console.WriteLine("Film by Person");
        Console.WriteLine("Enter person");

    }

    public void PrintFilms(List<Film> films)
    {
        foreach (var film in films)
        {
            Console.WriteLine(film.Title);
        }
    
    }

    public void FilmByTag()
    {
        Console.Clear();
        Console.WriteLine("Film by Tag");
        Console.WriteLine("Enter tag");
    }

    public void RecomendationMenu()
    {
        Console.WriteLine("------------RECOMMENDATIONS---------------");
    }

    public void FilmInfo()
    {
        Console.Clear();
        Console.WriteLine("Film info");
        Console.WriteLine("Enter film");
    
    }

    public void PrintActors(HashSet<Person> Actors)
    {
        foreach(var a in Actors)
        {
            Console.WriteLine(a.Name);
        }
    }


}