namespace HSPGUI.Views;
/// <summary>
/// The AntenaSetUp class is a .net Maui ContentPage that provides a user interface
/// for setting up an antenna. It includes methods for reading and writing antenna settings.
/// </summary>
public partial class AntenaSetUp : ContentPage
{
    /// <summary>
    /// A nullable HSPClient used to interact with the antenna.
    /// </summary>
    private HSPClient? client;
    /// <summary>
    /// The constructor for the AntenaSetUp class. It initializes the client field,
    /// calls the InitializeComponent method, and sets the initial selected index for various selectors.
    /// The initial selections are only changed if they are less than zero.
    /// </summary>
    public AntenaSetUp()
    {
        client = HSPClient.Instance;

        InitializeComponent();
        //baudRateSelector.SelectedIndex = Math.Max(baudRateSelector.SelectedIndex, 0);
        tariSelector.SelectedIndex = Math.Max(tariSelector.SelectedIndex, 0);
        bitPatternSelector.SelectedIndex = Math.Max(bitPatternSelector.SelectedIndex, 0);
        LFSelector.SelectedIndex = Math.Max(LFSelector.SelectedIndex, 0);
        recieverGainSelector.SelectedIndex = Math.Max(recieverGainSelector.SelectedIndex, 0);
        asyncRecieverGain.SelectedIndex = Math.Max(asyncRecieverGain.SelectedIndex, 0);
    }
    /// <summary>
    /// An event handler for the ReadSettings button click event. It reads the antenna status
    /// from the HSP and updates the selected index of various selectors based on the read settings.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    private async void ReadSettings_Clicked(object sender, EventArgs e)
    {
        if (client != null && client.isConnected())
        {
            var settings = await client.readAntenaSettings();

            if (settings != null && settings.Length > 5)
            {
                //baudRateSelector.SelectedIndex = int.Parse(settings[0]);
                tariSelector.SelectedIndex = int.Parse(settings[1]);
                bitPatternSelector.SelectedIndex = int.Parse(settings[2]);
                LFSelector.SelectedIndex = int.Parse(settings[3]);
                recieverGainSelector.SelectedIndex = int.Parse(settings[4]);
                asyncRecieverGain.SelectedIndex = int.Parse(settings[5]);
                
            }
        }
    }

    /// <summary>
    /// An event handler for the WriteSettings button click event. It writes the selected settings
    /// from the selectors to the antenna through the HSP.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    private async void WriteSettings_Clicked(object sender, EventArgs e)
    {
        var settings = new string[6];
        //settings[0] = (int)baudRateSelector.SelectedIndex;
        settings[1] = tariSelector.SelectedIndex.ToString();
        settings[2] = bitPatternSelector.SelectedIndex.ToString();
        settings[3] = LFSelector.SelectedIndex.ToString();
        settings[4] = recieverGainSelector.SelectedIndex.ToString();
        settings[5] = asyncRecieverGain.SelectedIndex.ToString();

        if (client != null && client.isConnected())
        {
           await client.writeAntenaSettings(settings);
        }
    }
}