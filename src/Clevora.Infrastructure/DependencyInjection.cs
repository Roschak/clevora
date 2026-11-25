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

        // Validate mount point (throws if not accessible)
        MountPointValidator.Validate(dbPath);

        services.AddDbContext<ClevoraDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        services.AddIdentityCore<ClevoraUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ClevoraDbContext>();

        services.AddScoped<JwtTokenService>();
        services.AddScoped<IComposerService, ComposerService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ITemplateService, TemplateService>();
        services.AddScoped<IAiMessageService, AzureAiMessageService>();
        services.AddScoped<IBlobStorageService, AzureBlobStorageService>();
        services.AddScoped<IAttachmentService, AttachmentService>();
        services.AddScoped<ISyncService, SyncService>();

        return services;
    }
}
