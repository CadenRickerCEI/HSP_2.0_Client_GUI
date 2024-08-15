namespace HSPGUI;
/// <summary>
/// The App class is the main application class for a .NET MAUI application.
/// It initializes the main page and sets up the HSP client.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// The HSPClient used to interact with the HSP server.
    /// </summary>
    public HSPClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// Sets up the main page and initializes the HSP client.
    /// </summary>
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
        MainPage.Title = "HSP 4.0";
        _client = HSPClient.Instance;
    }

    /// <summary>
    /// Handles application startup logic. Sets default preferences for IP address and port if they are not already set.
    /// </summary>
    protected override void OnStart()
    {
        // Handle when your app starts
        if (!Preferences.ContainsKey(Constants.KeyIpAddress))
        {
            Preferences.Set(Constants.KeyIpAddress, Constants.IpAddress); // Set to the HSP Default Address
        }

        if (!Preferences.ContainsKey(Constants.KeyPort)) // Set to the HSP Default port
        {
            Preferences.Set(Constants.KeyPort, Constants.Port);
        }
    }
}