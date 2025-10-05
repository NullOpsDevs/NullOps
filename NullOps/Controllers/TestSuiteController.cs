using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NullOps.DAL;
using NullOps.DataContract;
using NullOps.Services.Seeding.Seeders;

namespace NullOps.Controllers;

[AllowAnonymous]
[Route("/api/v1/test-suite")]
public class TestSuiteController(IHostEnvironment hostEnvironment, IDbContextFactory<DatabaseContext> dbContextFactory, IServiceProvider serviceProvider)
{
    [HttpGet("clear-database")]
    public async Task<BaseResponse> ClearDatabase()
    {
        if (!hostEnvironment.IsDevelopment())
        {
            return new BaseResponse
            {
                Success = false,
                Error = new ResponseError
                {
                    Code = ErrorCode.TestModeIsNotEnabled,
                    Message = "Test mode is not enabled, go away!"
                }
            };
        }

        await using var context = await dbContextFactory.CreateDbContextAsync();
        
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        var seeder = ActivatorUtilities.CreateInstance<SuperAdminSeederService>(serviceProvider);
        await seeder.SeedAsync(context);

        return BaseResponse.Successful;
    }
}