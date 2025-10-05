using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NullOps.DAL;
using NullOps.DAL.Enums;
using NullOps.DAL.Models;
using NullOps.DataContract;
using NullOps.Exceptions;
using NullOps.Extensions;
using NullOps.Services.Users.Hashers;
using NullOps.Setup;

namespace NullOps.Services.Users;

public class UsersService(IDbContextFactory<DatabaseContext> dbContextFactory, IUserPasswordHasher userPasswordHasher)
{
    public async Task<string> LoginAsync(string username, string password, CancellationToken ct)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(ct);
        
        var user = context.Users.FirstOrDefault(x => x.Username == username);
        
        if(user == null)
            throw new DomainException(ErrorCode.InvalidCredentials, "Invalid credentials", HttpStatusCode.Unauthorized);
        
        var passwordsMatch = userPasswordHasher.Verify(user.Id, password, user.Password);
        
        if(!passwordsMatch)
            throw new DomainException(ErrorCode.InvalidCredentials, "Invalid credentials", HttpStatusCode.Unauthorized);
        
        return IssueTokenAsync(user);
    }

    public async Task<string> UpdateTokenAsync(Guid userId, CancellationToken ct)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(ct);
        
        var user = await context.Users.FirstOrDefaultAsync(x => x.Id == userId, ct);
        
        if(user == null)
            throw new DomainException(ErrorCode.InvalidCredentials, "Invalid credentials", HttpStatusCode.Unauthorized);
        
        return IssueTokenAsync(user);
    }
    
    public async Task RegisterUserAsync(string username, string password, UserRole userRole, CancellationToken cancellationToken)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        if (!await context.GetConfigurationAsync(Settings.RegistrationEnabled, false))
            throw new DomainException(ErrorCode.RegistrationIsDisabled, "Registration is disabled", HttpStatusCode.Forbidden);
        
        var userExists = await context.Users.AnyAsync(x => x.Username == username, cancellationToken);

        if (userExists)
            throw new DomainException(ErrorCode.UserAlreadyExists, "User already exists", HttpStatusCode.Conflict);
        
        var superAdminUser = new User
        {
            Username = username,
            Password = string.Empty,
            Role = UserRole.SuperAdministrator
        };
        
        await context.Users.AddAsync(superAdminUser, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        await context.Users.Where(x => x.Id == superAdminUser.Id)
            .ExecuteUpdateAsync(x =>
                x.SetProperty(p => p.Password, userPasswordHasher.Hash(superAdminUser.Id, password)), cancellationToken);
        
        await transaction.CommitAsync(cancellationToken);
    }
    
    private static string IssueTokenAsync(User user)
    {
        Claim[] claims =
        [
            new(UserClaimTypes.UserId, user.Id.ToString("D")),
            new(UserClaimTypes.UserName, user.Username),
            new(UserClaimTypes.UserRole, user.Role.ToString("G"))
        ];
        
        var key = EnvSettings.Jwt.SigningKey;
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            issuer:              WebSetup.JwtIssuer,
            audience:            WebSetup.JwtAudience,
            claims:              claims,
            expires:             DateTime.UtcNow.AddMinutes(15),
            signingCredentials:  creds
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}