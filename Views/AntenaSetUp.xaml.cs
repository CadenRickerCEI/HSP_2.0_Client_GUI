namespace HSPGUI.Views;

public partial class AntenaSetUp : ContentPage
{
    private HSPClient client;
    public AntenaSetUp()
    {
        client = ((App)Application.Current)._client;
        InitializeComponent();
        baudRateSelector.SelectedIndex      = Math.Max(baudRateSelector.SelectedIndex, 0  );
        tariSelector.SelectedIndex          = Math.Max(tariSelector.SelectedIndex , 0 );
        bitPatternSelector.SelectedIndex    = Math.Max(bitPatternSelector.SelectedIndex , 0 );
        LFSelector.SelectedIndex            = Math.Max(LFSelector.SelectedIndex , 0 );
        recieverGainSelector.SelectedIndex  = Math.Max(recieverGainSelector.SelectedIndex , 0 );
        asyncRecieverGain.SelectedIndex     = Math.Max(asyncRecieverGain.SelectedIndex , 0 );        
    }
    private async void ReadSettings_Clicked(object sender, EventArgs e)
    {
        int[] settings = await client.readAntenaStatus();
        if (settings != null && settings.Length >5 )
        {
            baudRateSelector.SelectedIndex      = settings[0];
            tariSelector.SelectedIndex          = settings[1];
            bitPatternSelector.SelectedIndex    = settings[2];
            LFSelector.SelectedIndex            = settings[3];
            recieverGainSelector.SelectedIndex  = settings[4];
            asyncRecieverGain.SelectedIndex     = settings[5];
        }
    }
    private async void WriteSettings_Clicked(object sender, EventArgs e)
    {
        int [] settings = new int[6];
        settings[0] = (int) baudRateSelector.SelectedIndex;
        settings[1] = (int) tariSelector.SelectedIndex;
        settings[2] = (int) bitPatternSelector.SelectedIndex;
        settings[3] = (int) LFSelector.SelectedIndex;
        settings[4] = (int) recieverGainSelector.SelectedIndex;
        settings[5] = (int) asyncRecieverGain.SelectedIndex;
        await client.writeAntenaSettigns(settings);
    }
}
