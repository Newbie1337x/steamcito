namespace steamcito.Models;

public class Genre
{
    public Genre(string name)
    {
        Name = name;
    }

    public int Id { get; set; }
    public string Name { get; set; }
}