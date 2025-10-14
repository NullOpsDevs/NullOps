namespace NullOps.DAL.Models;

public class Agent : IEntity
{
    /// <inheritdoc />
    public Guid Id { get; set; }

    /// <summary>Agent's name</summary>
    public required string Name { get; set; }

    /// <summary>Agent's address</summary>
    public required string Address { get; set; }

    /// <summary>Agent's port</summary>
    public required int Port { get; set; }

    /// <summary>Agent's token</summary>
    public required string Token { get; set; }
    
    /// <inheritdoc />
    public DateTime CreatedAt { get; set; }

    /// <inheritdoc />
    public DateTime? UpdatedAt { get; set; }
}