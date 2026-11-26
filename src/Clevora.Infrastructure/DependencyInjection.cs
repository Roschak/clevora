using Clevora.Application.Interfaces;
using Clevora.Infrastructure.Data;
using Clevora.Infrastructure.Identity;
using Clevora.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Clevora.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var dbPath = configuration["Database:Path"] ?? "/home/data/app.db";

        // Validate mount point (throws if not accessible on Linux; skipped on Windows dev)
        MountPointValidator.Validate(dbPath);

        services.AddDbContext<ClevoraDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        services.AddIdentityCore<ClevoraUser>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ClevoraDbContext>();

        services.AddScoped<JwtTokenService>();
        services.AddScoped<IChatService, ChatService>();

        return services;
    }
}
