namespace HSPGUI.Views;
using HSPGUI.Resources;
using System;

public partial class cmdLine : ContentPage
{
    /// <summary>
    /// A nullable HSPClient used to interact with the HSP object.
    /// </summary>
    private HSPClient? client;

    /// <summary>
    /// Initializes a new instance of the cmdLine class.
    /// </summary>
    public cmdLine()
    {
        // Retrieve the client instance from the application
        client = HSPClient.Instance;
        // Subscribe to the connection status changed event
        client.connectionStatusChanged += Client_connectionStatusChanged;
        // Initialize the component
        InitializeComponent();

        // Set the visibility of the connection button based on the client's connection status
        connectionBtn.IsVisible = client == null || !client.isConnected();

        // Set the visibility of the stack layout based on the connection button's visibility
        StackLayout.IsVisible = !connectionBtn.IsVisible;
    }
    /// <summary>
    ///  listener for when the connection status changes on the HSP
    /// </summary>
    /// <param name="connectionStatus"></param>
    private void Client_connectionStatusChanged(bool connectionStatus)
    {
        connectionBtn.IsVisible = !connectionStatus;
        StackLayout.IsVisible = connectionStatus;
    }

    /// <summary>
    /// when the button is clicks it call the connect to serve.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public void connectBtnClicked(object sender, EventArgs args) => connectToServer();

    /// <summary>
    /// connnects to the server. If the connection fails it will display an alert.
    /// </summary>
    public async void connectToServer()
    {
        System.Diagnostics.Debug.WriteLine("Attempting to connect to Server");

        if (client != null)
        {
            string IPAddress = Preferences.Get(Constants.KeyIpAddress, Constants.IpAddress);
            dialog.Text = client.connectToHSP(IPAddress, Preferences.Get(Constants.KeyPort, Constants.Port),
                                                Preferences.Get(Constants.KeyPortDIAG,Constants.PortDIAG),
                                                Preferences.Get(Constants.KeyPortDATA,Constants.PortDATA));
            // System.Diagnostics.Debug.WriteLine("Connection Attempt Finished");
            if (client.isConnected() == false)
            {
                var tryAgain = await DisplayAlert("Connection Error", "Connection to HSP failed. Try again?", "Try Again", "Cancel");
                if (tryAgain) connectToServer();
            }
        }
    }

    /// <summary>
    /// sends user command entered into the cmd entry box
    /// to HSP and writes resonse back in dialog box back.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void cmd_Entered(object sender, EventArgs e)
    {
        // Check if the client is connected
        if (client != null && client.isConnected())
        {
            // Retrieve the command text from the cmd control
            var cmdText = cmd.Text;

            // Clear the command text box
            cmd.Text = "";
            // Update the dialog with the user command and the response from the client
            dialog.Text = $"USER_>{cmdText}";
            dialog.Text = $"{dialog.Text}\n{client.writeUsrCMD(cmdText)}";
        }
        else
        {
            // Update the dialog to indicate that the client is not connected
            dialog.Text = "Not connected";
        }
    }
}