using System.Collections.ObjectModel;

namespace steamcito.Models;
public class Game
{
    public int Id { get; set; }
    public GameDetails Details { get; set; }
    
    public ObservableCollection<string> GamePaths { get; set; } = new ObservableCollection<string>();
    
    public Artwork Artworks { get; set; }

    public Game()
    {
        Details = new GameDetails();
        Artworks = new Artwork();
    }
}