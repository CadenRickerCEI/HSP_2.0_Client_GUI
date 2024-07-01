namespace HSPGUI.Views;

public partial class AntenaSetUp : ContentPage
{
    public AntenaSetUp()
    {

        InitializeComponent();

        List<String> baudRateOptions = new List<String> {
        "9600","19200","38400","57600","115200","230400","460800","921600"};
        baudRateSelector.ItemsSource = baudRateOptions;
        List<String> tariOptions = new List<String> {
        " 6.25 usec","12.50 usec","25.00 usec"};
        tariSelector.ItemsSource = tariOptions;
        List<String> bitPatternOpt = new List<String> {
        "FM0","MILLER2","MILLER4", "MILLER8"};
        bitPatternSelector.ItemsSource = bitPatternOpt;
        List<String> LFOptions = new List<String> {
        "40 khz","160 khz","250 khz", "320 khz","640 khz"};
        LFSelector.ItemsSource = LFOptions;
        

        baudRateSelector.SelectedIndexChanged += (s, e) =>
        {
            //var updateBaudRate = true;
            var selectedBaudRate = baudRateSelector.SelectedItem;
        };

        

        tariSelector.SelectedIndexChanged += (s, e) =>
        {
            //var updateTariLength = true;
            var selectedTariLength = baudRateSelector.SelectedIndex;
        };

        

        bitPatternSelector.SelectedIndexChanged += (s, e) =>
        {
            //var updateBitPattern = true;
            var selectedBitPattern = baudRateSelector.SelectedIndex;
        };
        

        LFSelector.SelectedIndexChanged += async (s, e) =>
        {
            //var updateLF = true;
            var selectedLF = LFSelector.SelectedIndex;
            await DisplayAlert("Alert", "The selected item has changed to: " + LFSelector.SelectedItem, "OK");
        };
    }
}
