using System.Security.Claims;
using NullOps.Services.Users;

namespace NullOps.Extensions;

public static class UserIdentityExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        
        if(userIdClaim == null)
            throw new Exception("UserID claim was not found");
        
        return Guid.Parse(userIdClaim.Value);
    }
}