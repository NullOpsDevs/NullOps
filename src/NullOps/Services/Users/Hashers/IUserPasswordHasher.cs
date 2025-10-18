namespace NullOps.Services.Users.Hashers;

public interface IUserPasswordHasher
{
    string Hash(Guid userId, string password);
    bool Verify(Guid userId, string password, string hashedPassword);
}