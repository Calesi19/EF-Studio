using EFStudio.Core.Middleware;
using EFStudio.Core.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

    public static IApplicationBuilder UseEFStudio(this IApplicationBuilder app, bool force = false)
    {
        if (!force)
        {
            var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();

            if (!env.IsDevelopment())
            {
                throw new InvalidOperationException(
                    "EFStudio is for development environments only. " +
                    "Wrap the call with: if (app.Environment.IsDevelopment()) { app.UseEFStudio(); }. " +
                    "To use in a non-Development environment intentionally, call UseEFStudio(force: true).");
            }
        }

        return app.UseMiddleware<EFStudioMiddleware>();
    }
}
