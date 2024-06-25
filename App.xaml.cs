namespace HSPGUI;
using HSPGUI.Resources;
public partial class App : Application
{
	public TelnetClient _client;
	public App()
	{
		InitializeComponent();
        
        MainPage = new AppShell();
		_client = new TelnetClient();
		
	}
}
