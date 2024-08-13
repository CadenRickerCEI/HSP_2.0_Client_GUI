namespace HSPGUI.Views;
using HSPGUI.Resources;
using System;

public partial class cmdLine : ContentPage
{
    private HSPClient? client;
    // private HSPClient? client;
    public cmdLine()
    {
        if (((App)Application.Current!) != null)
        {
            client = ((App)Application.Current)._client;
            client.connectionStatusChanged += Client_connectionStatusChanged;
        }

        InitializeComponent();
    }

    public void connectBtnClicked(object sender, EventArgs args) => connectToServer();

    /// <summary>
    ///  listener for when the connection status changes on the HSP
    /// </summary>
    /// <param name="connectionStatus"></param>
    private void Client_connectionStatusChanged(bool connectionStatus)
    {
        connectionBtn.IsVisible = !connectionStatus;
        StackLayout.IsVisible = connectionStatus;
    }

    public async void connectToServer()
    {
        System.Diagnostics.Debug.WriteLine("Attempting to connect to Server");

        if (client != null)
        {
            dialog.Text = client.connectToHSP(Preferences.Get(Constants.KeyIpAddress, Constants.IpAddress), Preferences.Get(Constants.KeyPort, Constants.Port));
            System.Diagnostics.Debug.WriteLine("Connection Attempt Finished");

            if (client.isConnected() == false)
            {
                var tryAgain = await DisplayAlert("Connection Error", "Connection to HSP failed. Try again?", "Try Again", "Cancel");
                if (tryAgain) connectToServer();
            }
        }
    }


    private void cmd_Completed(object sender, EventArgs e)
    {
        if (client != null && client.isConnected())
        {
            var cmdText = cmd.Text;
            cmd.Text = "";
            dialog.Text = $"USER_>{cmdText}\n{client.writeUsrCMD(cmdText)}";
        }
        else
        {
            dialog.Text = "Not connectd";
        }
    }
}