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
        InitializeComponent();       
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        client = HSPClient.Instance;
        client.connectionStatusChanged += Client_connectionStatusChanged;
        client.dataUpdated += dialogDataUpdated;
        StartPeriodicTask();
    }
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _isRunning = false;
        if (client != null)
        {
            client.connectionStatusChanged -= Client_connectionStatusChanged;
            client.dataUpdated -= dialogDataUpdated;
        }
        client = null;
    }
    /// <summary>
    ///  listener for when the connection status changes on the HSP
    /// </summary>
    /// <param name="connectionStatus"></param>
    private void Client_connectionStatusChanged(bool connectionStatus)
    {
        try
        {
            bool demoMode = Preferences.Get(Constants.demoMode, false);
            connectionBtn.IsVisible = !connectionStatus && !demoMode;
            statusGrid.IsVisible = connectionStatus || demoMode;
        }
        catch
        {
            connectionBtn.IsVisible = true;
            statusGrid.IsVisible = false;
        }
        
    }
    /// <summary>
    /// Updates the dialog display if the dialog data has been updated.
    /// </summary>
    /// <param name="updated">Indicates whether the dialog data has been updated.</param>
    private void dialogDataUpdated(bool updated)
    {
        if (client != null)
        {
            var tagString = client.tagLog.getCurrentTag();
            
            if (lastTag.Text != null || tagString != lastTag.Text)
            {
                lastTag.Text = tagString;
            }            
            
            try
            {
                var errorCount = client.tagLog.badTags.Count();
                if (tagErrLabel.Text == "" || tagErrLabel.Text == null) 
                {
                    tagErrLabel.Text = client.tagLog.dequeErrHist();
                }
                else
                {
                    errorCount++;
                }
                
                ErrorCount.Text = $"{errorCount.ToString(),5}";
            }
            catch
            {
                ErrorCount.Text = "Failed to get count";
            }
            clearErrBtn.IsVisible = tagErrLabel.Text != "";
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
            //dialog.Text = client.connectToHSP(Preferences.Get(Constants.KeyIpAddress, Constants.IpAddress), Preferences.Get(Constants.KeyPort, Constants.Port));
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
        //var _ = scrollCMD.ScrollToAsync(0, dialog.Height + 5, true);
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
            //dialog.Text = result[1];
        }
        else
        {
            bufferCount.Text = "-1";
            dialog.Text = "Count invalid not connected";
        }
        //var _ = scrollCMD.ScrollToAsync(0, dialog.Height + 5, true);        
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
            if (dialog.Text.Contains("ENGAGED")){
                await DisplayAlert("System Engaged", "HSP system is engaged", "Ok");
            }
            else
            {
                await DisplayAlert("System Engaged", dialog.Text, "Ok");
            }
            //var _ = scrollCMD.ScrollToAsync(0, dialog.Height + 5, true);
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
            //var _ = scrollCMD.ScrollToAsync(0, dialog.Height , true);
        }
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
                        bool demoMode = Preferences.Get(Constants.demoMode, false);
                        connectionBtn.IsVisible = !demoMode;
                        statusGrid.IsVisible = demoMode ;
                        bufferCount.Text = "0";
                        dialog.Text = "Count invalid not connected";
                    }
                    //var _ = scrollCMD.ScrollToAsync(0,dialog.Height,true);
                });
                return true; // Repeat again
            }
            return false; // Stop repeating
        });
    }

    private void removeErr_clicked(object sender, EventArgs e)
    {
       if (client != null)
       {
            tagErrLabel.Text = client.tagLog.dequeErrHist();
            clearErrBtn.IsVisible = tagErrLabel.Text != "";
            return;              
       }
       tagErrLabel.Text = "";
       scannerInput.Focus();
    }

    private void btnCLearPressed(object sender, EventArgs e)
    {
        scannerInput.Text = "";
    }

    private async void getSystemMode_Clicked(object sender, EventArgs e)
    {
        var systemTypes = new string[] { "STA", "VER",  "ENC", "TRE" };
        if (client!= null)
        {
            await client.readSystemMode();
            if (systemTypes.Contains(client.systemMode))
            {
                SysteModeSelction.SelectedIndex = Array.IndexOf(systemTypes, client.systemMode);
            }
        }        
    }

    private async void SysteModeSelction_SelectedIndexChanged(object sender, EventArgs e)
    {
        var systemTypes = new string[] { "STA", "VER", "ENC", "TRE" };
        if (client!= null && SysteModeSelction.SelectedIndex > -1)
        {
            await client.writeSystemMode(systemTypes[SysteModeSelction.SelectedIndex]);
        }
    }

    private async void bufferInfo_Clicked(object sender, EventArgs e)
    {
        await DisplayAlert("Buffer Help", "Buffer count is the number of tags left in the HSP system.\nError Count is the number of errors in the queue for the system.", "Done");
    }

    private async void SystemModeHelp_Clicked(object sender, EventArgs e)
    {
        await DisplayAlert("System Mode Help", "System mode controls the the mode the system runs in. Press Read System type to get the current setting.\n" +" Change system type by slecting a new type in the drop down.", "Done");
    }

    private async void lastTagHelp_Clicked(object sender, EventArgs e)
    {
        await DisplayAlert("Last Tag Help", "System mode controls the the mode the system runs in.", "Done");
    }

    private async void TagErrHelp_Clicked(object sender, EventArgs e)
    {
        await DisplayAlert("Tag Error Help", "The tag error displays the oldest tag in the error queque.\nThe codes meanings:\n"+
            "C<X> Error from tag <X> denotes the error num\n"+
            "Press \"Clear Error\" to remove the error from the queue.", "Done");
    }

    private async void scannerInputHelp_Clicked(object sender, EventArgs e)
    {
        await DisplayAlert("Scanner Input Help", "This box is here for input from the scanner. Press \"Clear Input\" to clear the box of text.", "Done");
    }
}