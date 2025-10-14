namespace NullOps.DAL.Enums;

public enum UserRole
{
    SuperAdministrator,
    Administrator,
    User
}

public static class UserRoleExtensions
{
    private const string SuperAdministrator = nameof(UserRole.SuperAdministrator);
    private const string Administrator = nameof(UserRole.Administrator);
    private const string User = nameof(UserRole.User);
    
    public static string ToRoleString(this UserRole role)
    {
        return role switch
        {
            UserRole.SuperAdministrator => SuperAdministrator,
            UserRole.Administrator => Administrator,
            UserRole.User => User,
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
        };
    }
}
