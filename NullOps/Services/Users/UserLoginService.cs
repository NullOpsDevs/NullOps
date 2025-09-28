using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NullOps.DAL;
using NullOps.DAL.Models;
using NullOps.DataContract;
using NullOps.Exceptions;
using NullOps.Services.Users.Hashers;
using NullOps.Setup;

namespace NullOps.Services.Users;

public class UserLoginService(IDbContextFactory<DatabaseContext> dbContextFactory, IUserPasswordHasher userPasswordHasher)
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