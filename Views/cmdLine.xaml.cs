namespace HSPGUI.Views;
using HSPGUI.Resources;
using System;

public partial class cmdLine : ContentPage
{
    /// <summary>
    /// A nullable HSPClient used to interact with the HSP object.
    /// </summary>
    private HSPClient? client;

    private bool visable = false;
    /// <summary>
    /// Initializes a new instance of the cmdLine class.
    /// </summary>
    public cmdLine()
    {        
        // Initialize the component
        InitializeComponent();        
    }
    protected override void OnAppearing()
    {        
        base.OnAppearing();
        // Retrieve the client instance from the application
        client = HSPClient.Instance;

        // Set the visibility of the connection button based on the client's connection status
        connectionBtn.IsVisible = client == null || !client.isConnected();


        if (client != null)
        {
            // Set the visibility of the stack layout based on the connection button's visibility
            StackLayout.IsVisible = !client.isConnected();
            // Set the visibility of the connection button based on the client's connection status
            connectionBtn.IsVisible = false;
            // Subscribe to the connection status changed event
            client.connectionStatusChanged += Client_connectionStatusChanged;
            dialogData.Text = client.dataBuffer;
            dialogDIAG.Text = client.dialogbuffer;
            client.dataUpdated += dialogDataUpdated;
            //client.cmdUpdated += dialogCmdUpdated;
            dialog.Text = client.cmdbuffer;
            var _ = scrollDIAG.ScrollToAsync(0, dialogDIAG.Height + scrollDIAG.Height, false);
            var i = scrollDATA.ScrollToAsync(0, dialogData.Height + scrollDATA.Height, false);
            var j = scrollCMD.ScrollToAsync(0, dialog.Height + scrollCMD.Height, false);
        }        
        visable = true;
    }
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (client != null)
        {
            client.connectionStatusChanged -= Client_connectionStatusChanged;
            client.dataUpdated -= dialogDataUpdated;
        }
        client = null;
        visable = false;
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
    public async void connectBtnClicked(object sender, EventArgs args) => await connectToServer();

    /// <summary>
    /// connnects to the server. If the connection fails it will display an alert.
    /// </summary>
    public async Task connectToServer()
    {
        System.Diagnostics.Debug.WriteLine("Attempting to connect to Server");

        if (client != null)
        {
            string IPAddress = Preferences.Get(Constants.KeyIpAddress, Constants.IpAddress);
            dialog.Text = await client.connectToHSP(IPAddress, Preferences.Get(Constants.KeyPort, Constants.Port),
                                                Preferences.Get(Constants.KeyPortDIAG,Constants.PortDIAG),
                                                Preferences.Get(Constants.KeyPortDATA,Constants.PortDATA));
            var _ = scrollCMD.ScrollToAsync(0,dialog.Height+10,true);
            // System.Diagnostics.Debug.WriteLine("Connection Attempt Finished");
            if (client.isConnected() == false)
            {
                var tryAgain = await DisplayAlert("Connection Error", "Connection to HSP failed. Try again?", "Try Again", "Cancel");
                if (tryAgain) await connectToServer();
            }
        }
    }

    /// <summary>
    /// sends user command entered into the cmd entry box
    /// to HSP and writes resonse back in dialog box back.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private async void cmd_Entered(object sender, EventArgs e)
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
            dialog.Text = $"{await client.writeUsrCMD(cmdText)}";
            await scrollCMD.ScrollToAsync(0, dialog.Height+scrollCMD.Height,false);
        }
        else
        {
            // Update the dialog to indicate that the client is not connected
            dialog.Text = "Not connected";
        }
    }
    /// <summary>
    /// Updates the dialog display if the dialog data has been updated.
    /// </summary>
    /// <param name="updated">Indicates whether the dialog data has been updated.</param>
    private void dialogDialogUpdtated(bool updated)
    {
        if (updated && client != null && visable)
        {
            // Update the dialog display with the new dialog buffer
            dialogDIAG.Text = client.dialogbuffer;

            // Scroll to the bottom of the dialog display
            var _ = scrollDIAG.ScrollToAsync(0, dialogDIAG.Height+ scrollDIAG.Height, false);
        }
    }

    /// <summary>
    /// Updates the data display if the data has been updated.
    /// </summary>
    /// <param name="updated">Indicates whether the data has been updated.</param>
    private void dialogDataUpdated(bool updated)
    {
        if (updated && client != null && visable)
        {
            // Update the data display with the new data buffer
            dialogData.Text = client.dataBuffer;

            // Scroll to the bottom of the data display
            var _ = scrollDATA.ScrollToAsync(0, dialogData.Height + scrollDATA.Height, false);
        }
    }
}