using Microsoft.Net.Http.Headers;

namespace Domain;

public enum Role
{
    noname,
    Actor, 
    Director,
    both
};
public class Person
{
    public string Name { get; protected set; }
    public string Surname { get; protected set; }
    public long Id { get; private set;}

    public Role Role{ get; private set;}

    public HashSet<Film> Films {get; private set; }

    public Person(string name, string surname, long id)
    {
        Role = Role.noname;
        Name = name;
        Surname = surname;
        Id = id;
        Films = new HashSet<Film>();
    }

    public void AddFilm(Film film)
    {
        Films.Add(film);
    }

    public void SetRole(Role role)
    {
        if (role == Role.noname)
        {
            return;
        }

        if (Role == Role.noname)
        {
            Role = role;
            return;
        }

        if (Role == role || Role == Role.both)
        {
            return;
        }

        Role = Role.both;
    }
}
