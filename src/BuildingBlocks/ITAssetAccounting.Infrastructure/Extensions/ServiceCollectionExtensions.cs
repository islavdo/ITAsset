using System.Text;
using ITAssetAccounting.Infrastructure.Auth;
using ITAssetAccounting.Infrastructure.Caching;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace ITAssetAccounting.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJwtAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
            options.AddPolicy("ItOrAdmin", p => p.RequireRole("Admin", "ItSpecialist"));
            options.AddPolicy("ManagerOrAdmin", p => p.RequireRole("Admin", "Manager"));
        });

        return services;
    }

    public static IServiceCollection AddRedisCaching(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(connection))
        {
            services.AddStackExchangeRedisCache(options => options.Configuration = connection);
            services.AddScoped<ICacheService, RedisCacheService>();
        }
        else
        {
            services.AddDistributedMemoryCache();
            services.AddScoped<ICacheService, RedisCacheService>();
        }

        return services;
    }

    public static void ConfigureSerilog(IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }
}
