namespace HSPGUI.Views;

public partial class StatusPage : ContentPage
{
    HSPClient client = ((App)Application.Current)._client;
    public StatusPage()
    {
        InitializeComponent();
    }
    public void OnButtonClicked(object sender, EventArgs args)
    {
        connectToServer();
    }
    public async void connectToServer()
    {
        System.Diagnostics.Debug.WriteLine("Attempting to connect to Sevrver");
        await client.connectToHSP(Preferences.Get(Constants.KeyIpAddress, Constants.IpAddress), Preferences.Get(Constants.KeyPort, Constants.Port));
        System.Diagnostics.Debug.WriteLine("await finished");
        if (client._connected == false)
        {
            bool tryAgain = await DisplayAlert("Connection Error", "Connection to HSP failed try again", "Try Again", "Cancel");
            if (tryAgain)
            {
                connectToServer();
            }
        }
    }
}
