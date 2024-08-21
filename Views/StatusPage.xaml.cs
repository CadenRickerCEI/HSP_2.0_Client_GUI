using Microsoft.Maui;
using Microsoft.Maui.Controls;
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
    /// Indicates whether the process is currently running.
    /// </summary>
    private bool _isRunning;
    /// <summary>
    /// The interval between each execution of the process.
    /// </summary>
    private TimeSpan _interval = TimeSpan.FromSeconds(3);
    /// <summary>
    /// Initializes a new instance of the <see cref="StatusPage"/> class.
    /// Sets up the client field and initializes the UI components.
    /// </summary>
    public StatusPage()
    {
        client = HSPClient.Instance;
        client.connectionStatusChanged += Client_connectionStatusChanged;
        client.dataUpdated += dialogDataUpdtated;
        client.dialogUpdated += dialogDialogUpdtated;
        InitializeComponent();
        if (client != null)
        {
            dialogData.Text = client.dataBuffer;            
            dialogDIAG.Text = client.dialogbuffer;
            var _ = scrollDIAG.ScrollToAsync(0, dialogDIAG.Height, true);
            var i = scrollDATA.ScrollToAsync(0, dialogData.Height, true);
            var j = scrollCMD.ScrollToAsync(0, dialog.Height, true);
        }
    }
    /// <summary>
    ///  listener for when the connection status changes on the HSP
    /// </summary>
    /// <param name="connectionStatus"></param>
    private void Client_connectionStatusChanged(bool connectionStatus)
    {
        connectionBtn.IsVisible = !connectionStatus;
        statusGrid.IsVisible = connectionStatus;
    }
    private void dialogDialogUpdtated(bool updated)
    {
        if (updated && client != null)
        {            
            dialogDIAG.Text = client.dialogbuffer;
            var _ = scrollDIAG.ScrollToAsync(0, dialogDIAG.Height, true);

        }
    }
    private void dialogDataUpdtated(bool updated)
    {
        if (updated && client != null)
        {
            dialogData.Text = client.dataBuffer;
            var _ = scrollDATA.ScrollToAsync(0, dialogData.Height+scrollDATA.Height, true); 
        }
    }
    /// <summary>
    /// Event handler for the button click event. Initiates the connection to the HSP server.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="args">The event arguments.</param>
    public async void OnButtonClicked(object sender, EventArgs args)
    {
        loadingIndicator.IsVisible = true;
        loadingIndicator.IsRunning = true;
        var orignalBtnText = connectionBtn.Text;
        connectionBtn.Text = "Connnecting to HSP";
        await Task.Delay(10);
        await connectToServer();
        connectionBtn.Text = orignalBtnText;

        if (client != null && client.isConnected() == false)
        {
            var tryAgain = await DisplayAlert("Connection Error", "Connection to HSP failed. Try again?", "Try Again", "Cancel");
            loadingIndicator.IsRunning = false;
            loadingIndicator.IsVisible = false;

            if (tryAgain) await connectToServer();
            else
            {
                connectionBtn.IsVisible = true;
                statusGrid.IsVisible = false;
                return;
            }
        }

        connectionBtn.IsVisible = false;
        statusGrid.IsVisible = true;
    }

    /// <summary>
    /// Asynchronously attempts to connect to the HSP server using the IP address and port
    /// stored in preferences. Displays an alert if the connection fails and offers to retry.
    /// </summary>
    public async Task connectToServer()
    {
        loadingIndicator.IsVisible = true;
        System.Diagnostics.Debug.WriteLine("Attempting to connect to Server");

        if (client != null)
        {
            // dialog.Text = client.connectToHSP(Preferences.Get(Constants.KeyIpAddress, Constants.IpAddress), Preferences.Get(Constants.KeyPort, Constants.Port));
            string IPAdrress = Preferences.Get(Constants.KeyIpAddress, Constants.IpAddress);
            int port = Preferences.Get(Constants.KeyPort, Constants.Port);
            int portDIAG = Preferences.Get(Constants.KeyPortDIAG, Constants.PortDIAG);
            int portData = Preferences.Get(Constants.KeyPortDATA, Constants.PortDATA);
            var message = "";
            await Task.Delay(10);
            try
            {
                message = await client.connectToHSP(IPAdrress, port, portDIAG, portData );
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            dialog.Text = message;
        }
        var _ = scrollCMD.ScrollToAsync(0, dialog.Height + 5, true);
        loadingIndicator.IsRunning = false;
        loadingIndicator.IsVisible = false;
    }

    /// <summary>
    /// clicked listener for get buffer count when pressed it queries 
    /// the HSP for the current buffer count.
    /// This command will display the current number of 
    /// records stored.The transfer mechanism between the 
    /// PI, (which stores the records), and the TR100, 
    /// (which physically modulates the RF carrier to 
    /// write to the TAG), is a 3 stage pipeline.The 
    /// three stages are first, the circular buffer 
    /// in the PI. Second, a holding register in the
    /// PI for the next record.Third, the record to 
    /// be written at the next trigger in the TR100
    /// holding buffer. The response to this command
    /// shows the count of all three of these pipeline stages.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void GetBufferCount(object sender, EventArgs e)
    {
        if (client != null && client.isConnected())
        {
            var result = await client.getBufferCount();
            bufferCount.Text = result[0];
            dialog.Text = result[1];
        }
        else
        {
            bufferCount.Text = "-1";
            dialog.Text = "Count invalid not connected";
        }
        var _ = scrollCMD.ScrollToAsync(0, dialog.Height + 5, true);
    }

    /// <summary>
    /// Engage HSP engages the HSP if its connected.
    /// When the ENGAGE command is issued the RF is turned 
    /// on, and the trigger line is processed according to 
    /// the edge control currently set(rising edge or falling edge)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    private async void EnagedHSP(object sender, EventArgs e)
    {
        if (client != null && client.isConnected())
        {
            dialog.Text = await client.EngageHSP();
            var _ = scrollCMD.ScrollToAsync(0, dialog.Height + 5, true);
        }
    }

    /// <summary>
    /// Disengages the HSP  if it's connected.
    /// If the system is in disengage state, normally the RF power 
    /// is off, and triggers are not recognized. 
    /// If auto clear is on buffer will be cleared when
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    private async void DisengageHSP(object sender, EventArgs e)
    {
        if (client != null && client.isConnected())
        {
            dialog.Text = await client.disengageHSP();
            var _ = scrollCMD.ScrollToAsync(0, dialog.Height , true);
        }
    }

    /// <summary>
    /// when the page opens start checking the buffer count.
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();
        StartPeriodicTask();
    }

    /// <summary>
    /// When the page is changd from this one stop checking buffer count.
    /// </summary>
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _isRunning = false;
    }

    /// <summary>
    /// Periodically check the buffer count and writes it to the count display.
    /// </summary>
    private void StartPeriodicTask()
    {
        _isRunning = true;

        Dispatcher.StartTimer(_interval, () =>
        {
            if (_isRunning)
            {
                // Your periodic function logic here
                Dispatcher.Dispatch(async () =>
                {
                    if (client != null && client.isConnected())
                    {
                        var result = await client.getBufferCount();
                        bufferCount.Text = result[0];
                        dialog.Text = result[1];
                        connectionBtn.IsVisible = false;
                        statusGrid.IsVisible = true;
                    }
                    else
                    {
                        connectionBtn.IsVisible = true;
                        statusGrid.IsVisible = false;
                        bufferCount.Text = "0";
                        dialog.Text = "Count invalid not connected";
                    }
                    var _ = scrollCMD.ScrollToAsync(0,dialog.Height,true);
                });
                return true; // Repeat again
            }
            return false; // Stop repeating
        });
    }
}