using EFStudio.Core.Middleware;
using EFStudio.Core.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EFStudio.Core.Extensions;

public static class EFStudioExtensions
{
    public static IServiceCollection AddEFStudio<TContext>(this IServiceCollection services)
        where TContext : DbContext
    {
        services.AddScoped<SchemaService>();
        services.AddScoped<DataService>();
        services.AddScoped<DbContext>(provider => provider.GetRequiredService<TContext>());
        return services;
    }

    public static IApplicationBuilder UseEFStudio(this IApplicationBuilder app)
    {
        return app.UseMiddleware<EFStudioMiddleware>();
    }
}
