using System.Net;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NullOps.DataContract;
using NullOps.Serializers;
using NullOps.Services.Users;
using ILogger = Serilog.ILogger;

namespace NullOps.Setup;

public static class WebSetup
{
    public const string JwtIssuer = "nullops";
    public const string JwtAudience = "api";
    
    public static void SetupWeb(this WebApplicationBuilder builder, ILogger startupLogger)
    {
        var port = EnvSettings.Hosting.Port;
        
        startupLogger.Information("API is listening on port '{Port}'", (int) port);
        
        // Kestrel configuration
        builder.WebHost.ConfigureKestrel(o =>
        {
            o.AddServerHeader = false;
            o.ListenAnyIP(port);
        });

        // Controllers
        builder.Services
            .AddControllers(options =>
            {
                options.ModelValidatorProviders.Clear();
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            })
            .AddJsonOptions(options =>
            {
                // Also change your settings in GlobalJsonSerializerOptions.Options
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
        
        // JWT auth
        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
        
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                    ValidTypes = ["JWT"],
                    ValidIssuer = JwtIssuer,
                    ValidAudience = JwtAudience,
                    IssuerSigningKey = EnvSettings.Jwt.SigningKey,
        
                    ClockSkew = TimeSpan.FromMinutes(1)
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        
                        context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                        await context.Response.WriteAsJsonAsync(BaseResponse.Unauthorized, GlobalJsonSerializerOptions.Options);
                    },
                    
                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = (int) HttpStatusCode.Forbidden;
                        await context.Response.WriteAsJsonAsync(BaseResponse.Unauthorized, GlobalJsonSerializerOptions.Options);
                    }
                };
            });
        
        // Swagger
        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddEndpointsApiExplorer();
            
            builder.Services.AddSwaggerGen(c =>
            {
                c.CustomSchemaIds(type =>
                {
                    if (!type.IsGenericType)
                        return type.Name;
                    
                    var genericTypeName = type.Name[..type.Name.IndexOf('`')];
                    var genericArgs = string.Join("", type.GetGenericArguments().Select(t => t.Name));
                    return $"{genericTypeName}<{genericArgs}>";
                });
            });
        }
    }
}