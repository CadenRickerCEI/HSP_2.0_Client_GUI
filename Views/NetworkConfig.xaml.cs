namespace HSPGUI.Views;
using HSPGUI.Resources;

/// <summary>
/// The NetworkConfig class is a .net MAUI ContentPage that provides a user interface
/// for configuring network settings, including IP address and port.
/// </summary>
public partial class NetworkConfig : ContentPage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NetworkConfig"/> class.
    /// Sets up the initial values for the IP address and port from preferences.
    /// </summary>
    public NetworkConfig()
    {
        InitializeComponent();
        var octetValidation = (OctetValidationBehavior)Resources["octetValidation"];
        var IpAddress = Preferences.Get("IpAddress", Constants.IpAddress);
        var octets = IpAddress.Split(".");
        octet1.Text = octets[0];
        octet2.Text = octets[1];
        octet3.Text = octets[2];
        octet4.Text = octets[3];
        portCMD.Text = Preferences.Get(Constants.KeyPort, Constants.Port).ToString();
        portDIAG.Text = Preferences.Get(Constants.KeyPortDIAG, Constants.PortDIAG).ToString();
        portDATA.Text = Preferences.Get(Constants.KeyPortDATA, Constants.PortDATA).ToString();
        
        demoModeBtn.Text = Preferences.Get(Constants.demoMode,false) ? "Demo Mode" : "Normal Mode";
    }

    /// <summary>
    /// Event handler for the Completed event of the first octet entry field.
    /// Sets the focus to the second octet entry field.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    private void octet1_Completed(object sender, EventArgs e) => octet2.Focus();

    /// <summary>
    /// Event handler for the Completed event of the second octet entry field.
    /// Sets the focus to the third octet entry field.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    private void octet2_Completed(object sender, EventArgs e) => octet3.Focus();

    /// <summary>
    /// Event handler for the Completed event of the third octet entry field.
    /// Sets the focus to the fourth octet entry field.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    private void octet3_Completed(object sender, EventArgs e) => octet4.Focus();

    /// <summary>
    /// Event handler for the Completed event of the fourth octet entry field.
    /// Sets the focus to the port entry field.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    private void octet4_Completed(object sender, EventArgs e) => portCMD.Focus();

    /// <summary>
    /// Event handler for the Clicked event of the Save button.
    /// Validates the IP address and port, and saves them to preferences if valid.
    /// Displays an alert if the IP address or port is invalid.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    private async void Button_Clicked(object sender, EventArgs e)
    {
        var octet1_int = (octet1.Text == null) ? -1 : int.Parse(octet1.Text);
        var octet2_int = (octet2.Text == null) ? -1 : int.Parse(octet2.Text);
        var octet3_int = (octet3.Text == null) ? -1 : int.Parse(octet3.Text);
        var octet4_int = (octet4.Text == null) ? -1 : int.Parse(octet4.Text);
        var portCMD_int = (portCMD.Text == null) ? -1 : int.Parse(portCMD.Text);
        var portDATA_int = (portDATA.Text == null) ? -1 : int.Parse(portDATA.Text);
        var portDIAG_int = (portDIAG.Text == null) ? -1 : int.Parse(portDIAG.Text);
        System.Diagnostics.Debug.WriteLine($"{octet1_int}.{octet2_int}.{octet3_int}.{octet4_int} port {portCMD_int}");

        if (octet1_int >= 0 && octet1_int < 256 && octet2_int >= 0 && octet2_int < 256 &
            octet3_int >= 0 && octet3_int < 256 && octet4_int > 0 && octet4_int < 256 &
            portCMD_int >= 0 && portCMD_int < 65535 && portDATA_int >= 0 && portDATA_int < 65535 &&
            portDIAG_int >= 0 && portDIAG_int < 65535)
        {
            Preferences.Set(Constants.KeyIpAddress, octet1.Text + "." + octet2.Text + "." + octet3.Text + "." + octet4.Text);
            Preferences.Set(Constants.KeyPort, portCMD_int);
        }
        else
        {
            await DisplayAlert("Invalid", "Ip Address or Port is invalid", "Confirm");
        }
    }

    /// <summary>
    /// Event handler for the Focused event of the entry fields.
    /// Sets the cursor position to the start and selects all text in the entry field.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    private void focused(object sender, FocusEventArgs e)
    {
        var entry = (Entry)sender;
        entry.CursorPosition = 0;

        if (entry.Text != null)
        {
            entry.SelectionLength = entry.Text.Length;
        }
    }
    /// <summary>
    /// Handles the completion event for the CMD port and sets focus to the DATA port.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void portCMD_Completed(object sender, EventArgs e)
    {
        // Set focus to the DATA port
        portDATA.Focus();
    }

    /// <summary>
    /// Handles the completion event for the DATA port and sets focus to the DIAG port.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void portDATA_Completed(object sender, EventArgs e)
    {
        // Set focus to the DIAG port
        portDIAG.Focus();
    }

    private void demoMode_Clicked(object sender, EventArgs e)
    {
        bool demoMode = Preferences.Get(Constants.demoMode, false);
        Preferences.Set(Constants.demoMode,!demoMode);
        demoModeBtn.Text = !demoMode?"Demo Mode":"Normal Mode";

    }
}