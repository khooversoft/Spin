﻿namespace NBlogWeb3.Services;

internal static class Startup
{
    public static IServiceCollection AddInternalServices(this IServiceCollection services)
    {
        services.AddScoped<LeftButtonStateService>();
        services.AddScoped<ArticleMenuService>();
        return services;
    }
}
