using PrimS.Telnet;

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
        if (!Preferences.ContainsKey(Constants.KeyPortDATA))
        {
            Preferences.Set(Constants.KeyPortDATA, Constants.PortDATA);
        }
        if (!Preferences.ContainsKey(Constants.KeyPortDIAG))
        {
            Preferences.Set(Constants.KeyPortDIAG, Constants.PortDIAG);
        }
        if (!Preferences.ContainsKey(Constants.demoMode))
        {
            Preferences.Set(Constants.demoMode, false);
        }
        if (!Preferences.ContainsKey(Constants.disableDiag))
        {
            Preferences.Set(Constants.disableDiag, true);
        }


        var _ = StartReadingPortsAsync();
    }
    /// <summary>
    /// Creates a new window and attaches an event handler for the window's destroying event.
    /// </summary>
    /// <param name="activationState">The activation state of the application.</param>
    /// <returns>The created window.</returns>
    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Create the window using the base class implementation
        var window = base.CreateWindow(activationState);

        // Attach the OnAppClosing event handler to the window's Destroying event
        window.Destroying += OnAppClosing;

        // Return the created window
        return window;
    }

    /// <summary>
    /// Handles the application closing event and performs cleanup operations.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void OnAppClosing(object? sender, EventArgs e)
    {
        // Log the app closing event
        Debug.WriteLine("App is closing. Running cleanup code...");

        // Disconnect the client
        _client.disconect();

        // Log the disconnection
        System.Diagnostics.Debug.WriteLine("closing Connections");
    }

    /// <summary>
    /// Continuously reads data from the ports as long as the client is connected.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    async Task StartReadingPortsAsync()
    {
        // Continuously read data from the ports
        while (true)
        {
            if (_client != null && _client.isConnected() && !_client.busy)
            {
                // Read data from the dialog ports
                await _client.readDialogPorts();
            }

            // Wait for 1 second before the next read operation
            await Task.Delay(Constants.tcpUpdateRate);
        }
    }
}