using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NullOps.DAL;
using NullOps.DataContract;
using NullOps.Exceptions;
using NullOps.Extensions;
using NullOps.Services.Seeding.Seeders;

namespace NullOps.Controllers;

[AllowAnonymous]
[Route("/api/v1/test-suite")]
public class TestSuiteController(IHostEnvironment hostEnvironment, IDbContextFactory<DatabaseContext> dbContextFactory, IServiceProvider serviceProvider)
{
    [HttpGet("clear-database")]
    public async Task<BaseResponse> ClearDatabase()
    {
        AssertDevelopment();

        await using var context = await dbContextFactory.CreateDbContextAsync();
        
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        var seeder = ActivatorUtilities.CreateInstance<SuperAdminSeederService>(serviceProvider);
        await seeder.SeedAsync(context);

        return BaseResponse.Successful;
    }
    
    [HttpGet("settings/set")]
    public async Task<BaseResponse> SetSetting([FromQuery] string setting, [FromQuery] string value)
    {
        AssertDevelopment();
        
        await using var context = await dbContextFactory.CreateDbContextAsync();
        await context.SetConfigurationAsync(setting, value);
        
        return BaseResponse.Successful;
    }

    [NonAction]
    private void AssertDevelopment()
    {
        if (!hostEnvironment.IsDevelopment())
            throw new DomainException(ErrorCode.TestModeIsNotEnabled, "Go away!", (HttpStatusCode) 418);
    }
}