namespace HSPGUI;
public partial class App : Application
{
    public HSPClient _client;
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
        MainPage.Title = "HSP 4.0";        
        _client = new HSPClient();
    }
    protected override void OnStart()
    {
        // Handle when your app starts
        if (Preferences.ContainsKey(Constants.KeyIpAddress) == false)
        {
            Preferences.Set(Constants.KeyIpAddress, Constants.IpAddress);// Set to the HSP Default Address
        }
        if (Preferences.ContainsKey(Constants.KeyPort) == false)//Set to the HSP Default port
        {
            Preferences.Set(Constants.KeyPort, Constants.Port);
        }
    }

}
