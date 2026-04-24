using EFStudio.Core.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace EFStudio.Core.Extensions;

public static class EFStudioExtensions
{
    public static IServiceCollection AddEFStudio(this IServiceCollection services)
    {
        services.AddScoped<Services.SchemaExplorer>();
        return services;
    }

    public static IApplicationBuilder UseEFStudio(this IApplicationBuilder app)
    {
        return app.UseMiddleware<EFStudioMiddleware>();
    }
}
