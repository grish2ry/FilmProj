using Domain;
namespace Presentation;
public interface IUi
{
    void DisplayMenu();
    void FilmByPerson();
    void PrintFilms(List<Film> films);
    void FilmByTag();

    void RecomendationMenu();
    void FilmInfo();
    void PrintActors(HashSet<Person> Actors);
    void LogError(string msg);
    void PrintSingle(string msg);
}


