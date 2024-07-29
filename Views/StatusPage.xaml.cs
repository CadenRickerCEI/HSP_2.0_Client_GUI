using System.Collections.Specialized;
using System.ComponentModel;
using System.Xml.Linq;

namespace HSPGUI.Views;

/// <summary>
/// The StatusPage class is a .NET MAUI ContentPage that provides a user interface
/// for connecting to an HSP.
/// </summary>
public partial class StatusPage : ContentPage
{
    /// <summary>
    /// A nullable HSPClient used to interact with the HSP object.
    /// </summary>
    private HSPClient? client;
    

    /// <summary>
    /// Initializes a new instance of the <see cref="StatusPage"/> class.
    /// Sets up the client field and initializes the UI components.
    /// </summary>
    public StatusPage()
    {
        if (((App)Application.Current!) != null)
        {
            client = ((App)Application.Current)._client;
            client.connectionStatusChanged += Client_connectionStatusChanged;
        }

        InitializeComponent();
        
    }
    /// <summary>
    ///  listener for when the connection status changes on the HSP
    /// </summary>
    /// <param name="connectionStatus"></param>
    private void Client_connectionStatusChanged(bool connectionStatus)
    {
        if (connectionStatus)
        {
            connectedLabel.Text = "HSP Connected";
        }
        else
        {
            connectedLabel.Text = "HSP Definitely not Connected";
        }
    }

    /// <summary>
    /// Event handler for the button click event. Initiates the connection to the HSP server.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="args">The event arguments.</param>
    public void OnButtonClicked(object sender, EventArgs args) => connectToServer();

    /// <summary>
    /// Asynchronously attempts to connect to the HSP server using the IP address and port
    /// stored in preferences. Displays an alert if the connection fails and offers to retry.
    /// </summary>
    public async void connectToServer()
    {
        System.Diagnostics.Debug.WriteLine("Attempting to connect to Server");

        if (client != null)
        {
            await client.connectToHSP(Preferences.Get(Constants.KeyIpAddress, Constants.IpAddress), Preferences.Get(Constants.KeyPort, Constants.Port));
            System.Diagnostics.Debug.WriteLine("await finished");

            if (client.isConnected() == false)
            {
                var tryAgain = await DisplayAlert("Connection Error", "Connection to HSP failed. Try again?", "Try Again", "Cancel");
                if (tryAgain) connectToServer();
            }
        }
    }
    /// <summary>
    /// clicked listener for get buffer count when pressed it queries the HSP for the current buffer count
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void GetBufferCount(object sender, EventArgs e)
    {
        if (client != null && client.isConnected())
        {
            bufferCount.Text = await client.getBufferCount();
        }else
        {
            bufferCount.Text = "Count invalid not connected";
        }
    }
    /// <summary>
    /// Engage HSP engages the HSP if its connected.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    private async Task EngageHSP(object sender, EventArgs e)
    {
        if (client != null && client.isConnected())
        {
            await client.enableHSP();
        }
    }/// <summary>
    /// Disengages the HSP  if it's connected.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    private async Task DisegageHSP(object sender, EventArgs e)
    {
        if (client != null && client.isConnected())
        {
            await client.disengageHSP();
        }
    }
}