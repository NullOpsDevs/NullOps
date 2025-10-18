namespace NullOps.DAL.Models;

public class Setting : IEntity
{
    /// <inheritdoc />
    public Guid Id { get; set; }

    public required string Key { get; set; }
    
    public required string Value { get; set; }
    
    /// <inheritdoc />
    public DateTime CreatedAt { get; set; }

    /// <inheritdoc />
    public DateTime? UpdatedAt { get; set; }
}
