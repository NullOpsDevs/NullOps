using NullOps.DAL.Enums;

namespace NullOps.DAL.Models;

public class User : IEntity
{
    /// <inheritdoc />
    public Guid Id { get; set; }
    
    public required string Username { get; set; }
    
    public required string Password { get; set; }
    
    public UserRole Role { get; set; }
    
    public bool IsBlocked { get; set; }
    
    /// <inheritdoc />
    public DateTime CreatedAt { get; set; }
    
    /// <inheritdoc />
    public DateTime? UpdatedAt { get; set; }
}
