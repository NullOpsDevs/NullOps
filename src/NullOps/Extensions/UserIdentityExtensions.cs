using System.Net;
using System.Security.Claims;
using NullOps.DAL.Enums;
using NullOps.DataContract;
using NullOps.Exceptions;

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

    public static void AssertIsAdministrator(this ClaimsPrincipal user)
    {
        if (!user.IsInRole(UserRole.Administrator.ToRoleString()) &&
            !user.IsInRole(UserRole.SuperAdministrator.ToRoleString()))
            throw new DomainException(ErrorCode.InsufficientPermissions, "Insufficient permissions.", HttpStatusCode.Forbidden);
    }
}