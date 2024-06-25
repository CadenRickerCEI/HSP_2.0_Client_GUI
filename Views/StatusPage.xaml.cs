namespace HSPGUI.Views;

public partial class StatusPage : ContentPage
{
    TelnetClient client = ((App)Application.Current)._client;
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
        await client.ConnectAsync("192.168.100.10", 10);
        System.Diagnostics.Debug.WriteLine("await finished");
        if (client.connected == false)
        {
            bool tryAgain = await DisplayAlert("Connection Error", "Connection to HSP failed try again", "Try Again", "Cancel");
            if (tryAgain)
            {
                connectToServer();
            }
        }
    }
}
