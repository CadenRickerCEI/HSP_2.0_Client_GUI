// using Windows.Media.Protection.PlayReady;

using CommunityToolkit.Maui.Storage;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace HSPGUI.Views;
/// <summary>
/// The LoadFromFilePage class is a .net MAUI ContentPage that provides a user interface
/// for loading data from a CSV file and displaying the progress of the loading operation.
/// </summary>
public partial class LoadFromFilePage : ContentPage
{
    /// <summary>
    /// A nullable HSPClient used to interact with the antenna.
    /// </summary>
    private HSPClient? client;
    /// <summary>
    /// A Progress object to track the progress of the file loading operation.
    /// </summary>
    private Progress<double> _progress;
    private CancellationTokenSource cancellationTokenSource;


    /// <summary>
    /// The constructor for the LoadFromFilePage class. It initializes the client field,
    /// sets up the progress tracking, and configures the UI elements.
    /// </summary>
    public LoadFromFilePage()
    {
        client = HSPClient.Instance;
        _progress = new Progress<double>();
        cancellationTokenSource = new CancellationTokenSource();
        InitializeComponent();
        loadFileBtn.IsVisible = false;
        client.dialogUpdated += dialogDialogUpdtated;
        _progress.ProgressChanged += (s, e) =>
        {
            progressBar.Progress = e;
        };
        if (client != null)
        {
            dialogDIAG.Text = client.dialogbuffer;
            scrollDIAG.ScrollToAsync(0,dialogDIAG.Height,true);
        }
    }
    /// <summary>
    /// Event handler for the Open File button click event. It allows the user to select a CSV file
    /// and displays the file location in the Entry control.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    async void OnOpenFileClicked(object sender, EventArgs e)
    {
        var options = new PickOptions
        {
            PickerTitle = "Please select a CSV file",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>> { { DevicePlatform.WinUI, new[] { ".csv" } } })
        };

        var result = await FilePicker.PickAsync(options);

        if (result != null && File.Exists(result.FullPath))
        {
            // Display the file location in the Entry
            loadFileBtn.IsVisible = true;
            fileLocationEntry.Text = result.FullPath;
        }
        else
        {
            loadFileBtn.IsVisible = false;
            await DisplayAlert("File Does Not Exist", "Try Selecting the file again.", "Ok");
        }
    }

    /// <summary>
    /// Event handler for the Load File button click event. It loads data from the selected CSV file
    /// into the HSP, displaying the progress and handling any errors.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    private async void loadFileBtn_Clicked(object sender, EventArgs e)
    {
        progressBar.Progress = 0;
        progressFrame.IsVisible = true;
        progressBar.IsVisible = true;
        loadFileBtn.IsEnabled = false;
        var resetBuffer = await DisplayAlert("Clear HSP Buffer", "Press reset to clear the buffer or add to add to the buffer", "Reset", "Add");
        var loadResult = "Failed to Start Loading";

        if (_progress != null && client != null)
        {
            loadResult = await Task.Run(() =>
            {
                return client.LoadFromFile(fileLocationEntry.Text, _progress, resetBuffer, this);
            });
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("progress null");
        }

        if (loadResult == "")
        {
            await DisplayAlert("File Loading Complete", "File has been loaded into the HSP.", "Ok");
        }
        else if (loadResult == "File is missing, open in another program, or empty.")
        {
            await DisplayAlert("File Error", loadResult, "Ok");
        }
        else
        {
            await DisplayAlert("Tag Error", loadResult, "Ok");
        }

        progressBar.Progress = 0;
        progressBar.IsVisible = false;
        progressFrame.IsVisible = false;
        loadFileBtn.IsEnabled = true;
    }

    /// <summary>
    /// Event handler for the TextChanged event of the file location entry field. It updates the visibility
    /// of the Load File button based on whether the specified file exists.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    private void fileLocationEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (fileLocationEntry.Text != null)
        {
            // Display the file location in the Entry
            loadFileBtn.IsVisible = File.Exists(fileLocationEntry.Text) && client != null && client.isConnected();
        }
    }

    /// <summary>
    /// Event handler for the Create Sample button click event. It creates a sample file for the user to 
    /// reference when working with the csv.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    async void createSample(object sender, EventArgs e)
    {
        var csv = new StringBuilder();
        csv.AppendLine("EPC Data,User Data,Kill Password,Accesss Password,PC Word");
        csv.AppendLine("486F7720646F20796F75206F72670000,616E697A652061207370616365200000,70617274,793F3F3F,3F3F");
        csv.AppendLine("596F7520706C616E6574212048610001,48612048612048612048612048610000,CAFEFEED,DEADBEEF,ABBC");
        csv.AppendLine("496E20746865206865617274206F0002,66207468652073696D6D6572696E0002,6720706F,742C2057,6865");
        csv.AppendLine("7265207370696365732064616E630003,656420616E6420666C61766F72730003,20736F75,6768742C,2050");
        csv.AppendLine("6F72665207370696365732064616E636,56420616E6420666C61766F727320736,F7567687,42C20506,F726");
        csv.AppendLine("B206D657420436869636B656E2C20612,063756C696E61727920666174652C204,96E20612,06A616D6,2616");
        csv.AppendLine("C6179612C207468656972206C6F76652,0746F6F6B2073686170652E0A0A506F7,26B2C207,4656E646,5722");
        csv.AppendLine("0616E6420736561736F6E65642C20776,869737065726564206C6F772C20201C4,36869636,B656E2C2,06C6");
        csv.AppendLine("57420197320626C656E6420696E20746,8697320737069637920666C6F772E204,F7572207,56E696F6,E2C2");
        csv.AppendLine("0612073796D70686F6E79206F6620436,16A756E2064656C696768742C20496E2,07269636,520616E6,4207");
        csv.AppendLine("26F75782C20776520196C6C2064616E6,365207468726F75676820746865206E6,96768742,E201D0A0,A436");
        csv.AppendLine("869636B656E2C2066656174686572732,0727566666C65642C207265706C69656,42077697,46820676,C656");
        csv.AppendLine("52C20201C4465617220506F726B2C206,C65742019732073697A7A6C6520696E2,06861726,D6F6E792,E205");
        csv.AppendLine("96F757220736D6F6B79206E6F7465732,0616E64206D79207361766F727920677,26163652,C20546F6,7657");
        csv.AppendLine("46865722C20776520196C6C206372656,17465206120737069637920656D62726,163652E2,01D0A0A4,16E6");
        csv.AppendLine("420736F207468657920737769726C656,420696E2074686174206672616772616,E7420737,465772C2,0506");
        csv.AppendLine("F726B20197320726963686E657373206,56E7477696E656420776974682043686,9636B656,E2019732,0687");
        csv.AppendLine("5652E20546F6D61746F657320626C757,36865642C20616E64207065707065727,320626C6,17A65642,C204");
        csv.AppendLine("17320746865697220666C61766F72732,06D656C6465642C206C6F76652061626,C617A652,E0A0A496,E207");
        csv.AppendLine("46865206A616D62616C6179612019732,0737465616D79206166666169722C205,06F726B2,0616E642,0436");
        csv.AppendLine("869636B656E20666F756E6420736F6C6,163652074686572652E2054686569722,06461746,52C20612,06D6");
        csv.AppendLine("5646C6579206F66207A65737420616E6,420666972652C2041206C6F766520737,46F72792,073696D6,D657");
        csv.AppendLine("265642C206E6576657220746F2074697,2652E0A536F206E6578742074696D652,0796F752,07461737,4652");
        csv.AppendLine("07468617420737069637920626F776C2,C2052656D656D6265722074686520706,F726B206,16E64206,3686");
        csv.AppendLine("9636B656E20197320726F6C652E20496,E206A616D62616C61796120197320656,D6272616,3652C207,4686");
        csv.AppendLine("57920666F756E6420626C6973732C204,12063756C696E61727920726F6D616E6,36520146,12043616,A756");
        DateTime now = DateTime.Now;
        // Convert the CSV string to bytes
        var csvBytes = Encoding.UTF8.GetBytes(csv.ToString());
        using var stream = new MemoryStream(csvBytes);

        {
            var result = await FileSaver.Default.SaveAsync("LoadFileSample.csv",
                                                            stream, cancellationTokenSource.Token);
        }
    }

    /// <summary>
    /// Updates dialog box with the input
    /// </summary>
    /// <param name="input"> string to update dialog box</param>
    public void updatedialog(string input)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            dialog.Text = input;
        });
    }
    private void dialogDialogUpdtated(bool updated)
    {
        if (updated && client != null)
        {
            dialogDIAG.Text = client.dialogbuffer;
            var _ = scrollDIAG.ScrollToAsync(0, dialogDIAG.Height, true);
        }
    }
}