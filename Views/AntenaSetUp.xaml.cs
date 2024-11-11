using System.ComponentModel;

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

            if (settings != null && settings.Length > 4)
            {                
                tariSelector.SelectedIndex = int.Parse(settings[0].Substring(0,1));
                bitPatternSelector.SelectedIndex = int.Parse(settings[0].Substring(1,1));
                LFSelector.SelectedIndex = int.Parse(settings[0].Substring(2, 1));
                recieverGainSelector.SelectedIndex = int.Parse(settings[1]);
                asyncRecieverGain.SelectedIndex = int.Parse(settings[2]);
                RadioFrequency.Text = settings[3];
                recieverAttenuation.SelectedIndex = int.Parse(settings[4]);              

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
        var settings = new string[5];
        settings[0] = $"{tariSelector.SelectedIndex.ToString()},{bitPatternSelector.SelectedIndex.ToString()},{LFSelector.SelectedIndex.ToString()}";
        settings[1] = recieverGainSelector.SelectedIndex.ToString();
        settings[2] = asyncRecieverGain.SelectedIndex.ToString();
        settings[3] = RadioFrequency.Text;
        settings[4] = recieverAttenuation.SelectedIndex.ToString();

        if (client != null && client.isConnected())
        {
           await client.writeAntenaSettings(settings);
        }
    }


    private void RadioFrequency_Completed(object sender, EventArgs e)
    {
        decimal number;
        if (decimal.TryParse(RadioFrequency.Text, out number))
        {
            number = number < 902 ? 902 : number;
            number = number > 928 ? 928 : number;
            RadioFrequency.Text = (Math.Round(number * 20) / 20).ToString("F2");
        }
    }
}