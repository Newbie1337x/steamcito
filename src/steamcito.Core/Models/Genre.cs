namespace steamcito.Models;

public class Genre(string name)
{
    public int Id { get; set; }
    public string Name { get; set; } = name;
}