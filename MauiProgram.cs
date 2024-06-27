namespace HSPGUI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<StatusPage>();

        builder.Services.AddSingleton<LoadFromFilePage>();

        builder.Services.AddSingleton<GenerateFromFilePage>();

        builder.Services.AddSingleton<AntenaSetUp>();

        builder.Services.AddSingleton<NetworkConfig>();

        return builder.Build();
    }
}
