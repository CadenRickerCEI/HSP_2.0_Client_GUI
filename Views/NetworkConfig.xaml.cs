namespace HSPGUI.Views;
using HSPGUI.Resources;

public partial class NetworkConfig : ContentPage
{
    public NetworkConfig()
    {
        InitializeComponent();
        var octetValidation = (OctetValidationBehavior)Resources["octetValidation"];
        string IpAddress = Preferences.Get("IpAddress", Constants.IpAddress);
        string[] octets = IpAddress.Split(".");
        octet1.Text = octets[0];
        octet2.Text = octets[1];
        octet3.Text = octets[2];
        octet4.Text = octets[3];
        port.Text = Preferences.Get(Constants.KeyPort, Constants.Port).ToString();
    }

    private void octet1_Completed(object sender, EventArgs e)
    {
        octet2.Focus();
    }

    private void octet2_Completed(object sender, EventArgs e)
    {
        octet3.Focus();
    }

    private void octet3_Completed(object sender, EventArgs e)
    {
        octet4.Focus();
    }
    private void octet4_Completed(object sender, EventArgs e)
    {
        port.Focus();
    }


    private async void Button_Clicked(object sender, EventArgs e)
    {
        int octet1_int = (octet1.Text == null) ? -1 : int.Parse(octet1.Text);
        int octet2_int = (octet2.Text == null) ? -1 : int.Parse(octet2.Text);
        int octet3_int = (octet3.Text == null) ? -1 : int.Parse(octet3.Text);
        int octet4_int = (octet4.Text == null) ? -1 : int.Parse(octet4.Text);
        int port_int = (port.Text == null) ? -1 : int.Parse(port.Text);
        System.Diagnostics.Debug.WriteLine($"{octet1_int}.{octet2_int}.{octet3_int}.{octet4_int} port {port_int}");
        if (octet1_int >= 0 && octet1_int < 256 && octet2_int >= 0 && octet2_int < 256 &
            octet3_int >= 0 && octet3_int < 256 && octet4_int > 0 && octet4_int < 256 &
            port_int >= 0 && port_int < 65535)
        {
            Preferences.Set(Constants.KeyIpAddress, octet1.Text + "." + octet2.Text + "." + octet3.Text + "." + octet4.Text);
            Preferences.Set(Constants.KeyPort, port_int);
        }
        else
        {
            await DisplayAlert("Invalid", "Ip Address or Port is invalid", "Confirm");
        }
    }

    private void focused(object sender, FocusEventArgs e)
    {
        Entry entry = (Entry)sender;
        entry.CursorPosition = 0;
        if (entry.Text != null)
        {
            entry.SelectionLength = entry.Text.Length;
        }
    }


}