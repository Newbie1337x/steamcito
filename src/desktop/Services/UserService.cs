using steamcito.Data;
using steamcito.Models;

namespace steamcito.Services;

public class UserService
{
    private readonly AppDBContext _context;
    
    public UserService(AppDBContext context)
    {
        _context = context;
    }
    
    public List<User> GetAll() => _context.Users.ToList();
    
    public User? GetById(string id) => _context.Users.Find(id);
    
    public User? GetBySteamId(string steamId) 
        => _context.Users.FirstOrDefault(u => u.SteamId == steamId);
    
    public void Create(User user)
    {
        _context.Users.Add(user);
        _context.SaveChanges();
    }
    
    public void Update(User user)
    {
        _context.Users.Update(user);
        _context.SaveChanges();
    }
    
    public void Delete(string id)
    {
        
        var user = _context.Users.Find(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            _context.SaveChanges();
        }
    }
}
