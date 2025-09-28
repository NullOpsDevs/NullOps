using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using NullOps.DAL.Enums;
using NullOps.DAL.Models;

namespace NullOps.DAL;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions options) : base(options) {}

    public DbSet<User> Users { get; set; }
    
    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(DatabaseContext).Assembly);
    }

    public static void MapEnums(NpgsqlDbContextOptionsBuilder builder)
    {
        builder.MapEnum<UserRole>();
    }
}
