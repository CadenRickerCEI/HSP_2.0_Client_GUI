﻿using CommunityToolkit.Maui.Core;

namespace HSPGUI;

/// <summary>
/// The MauiProgram class is responsible for configuring and creating the .NET MAUI application.
/// It sets up the application builder, configures fonts, and registers services.
/// </summary>
public static class MauiProgram
{
    /// <summary>
    /// Creates and configures the .NET MAUI application.
    /// </summary>
    /// <returns>A configured instance of <see cref="MauiApp"/>.</returns>
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>().UseMauiCommunityToolkitCore();

        builder.UseMauiApp<App>().UseMauiCommunityToolkitCore()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("Consolas.ttf");
            });

        // Registering pages as singleton services
        builder.Services.AddSingleton<StatusPage>();
        builder.Services.AddSingleton<GenerateFromFilePage>();
        builder.Services.AddSingleton<LoadFromFilePage>();
        builder.Services.AddSingleton<AntenaSetUp>();
        builder.Services.AddSingleton<cmdLine>();
        builder.Services.AddSingleton<NetworkConfig>();
        return builder.Build();
    }
}