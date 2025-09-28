namespace NullOps.Services.Users.Hashers;

using BCrypt = BCrypt.Net.BCrypt;

public class BCryptPasswordHasher : IUserPasswordHasher
{
    /// <inheritdoc />
    public string Hash(Guid userId, string password)
    {
        return BCrypt.EnhancedHashPassword($"{userId:N}{password}", 11);
    }

    /// <inheritdoc />
    public bool Verify(Guid userId, string password, string hashedPassword)
    {
        return BCrypt.EnhancedVerify($"{userId:N}{password}", hashedPassword);
    }
}