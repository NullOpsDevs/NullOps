using NullOps.DAL.Enums;

namespace NullOps.DAL.Models;

public class User : IEntity
{
    public Guid Id { get; set; }
    
    public string Username { get; set; }
    
    public string Password { get; set; }
    
    public UserRole Role { get; set; }
    
    public bool IsBlocked { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
}
