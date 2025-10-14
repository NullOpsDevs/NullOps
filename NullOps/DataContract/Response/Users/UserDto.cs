using System.Text.Json.Serialization;
using NullOps.DAL.Enums;
using NullOps.DAL.Models;

namespace NullOps.DataContract.Response.Users;

public class UserDto : IMappable<User, UserDto>
{
    public UserDto()
    {
        
    }
    
    public UserDto(User user)
    {
        Id = user.Id;
        Username = user.Username;
        Role = user.Role;
        IsBlocked = user.IsBlocked;
        CreatedAt = user.CreatedAt;
        UpdatedAt = user.UpdatedAt;
    }

    public Guid Id { get; set; }
    
    public string Username { get; set; }
    
    public UserRole Role { get; set; }
    
    public bool IsBlocked { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }

    /// <inheritdoc />
    public static UserDto MapTo(User from) => new(from);
}
