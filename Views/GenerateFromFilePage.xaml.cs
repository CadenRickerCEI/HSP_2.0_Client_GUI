namespace HSPGUI.Views;
using HSPGUI.Resources;

public partial class GenerateFromFilePage : ContentPage
{

    public GenerateFromFilePage()
    {
        InitializeComponent();
        updateFromFileBtn.IsVisible = false;
        EPC_Entry.MinimumWidthRequest = EPC_Entry.FontSize * Constants.fontToWidthScale * (double)Constants.MaxLenEPC_hex;
        UserData_Entry.MinimumWidthRequest = UserData_Entry.FontSize * Constants.fontToWidthScale * (double)Constants.MaxLenUSR_Hex;
    }
    async void OnOpenFileClicked(object sender, EventArgs e)
    {

        var options = new PickOptions
        {
            PickerTitle = "Please select a CSV file",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>> { { DevicePlatform.WinUI, new[] { ".csv" } } })

        };

        var result = await FilePicker.PickAsync(options);

        if (result != null)
        {
            /*var fileName = result.FileName;
            var fileData = await result.OpenReadAsync();*/
            // Now you can use the file data

            // Display the file location in the Entry
            updateFromFileBtn.IsVisible = File.Exists(result.FullPath);
            fileLocationEntry.Text = result.FullPath;
        }
        else
        {
            updateFromFileBtn.IsVisible = false;
        }
    }

    private void updateFromFileBtn_Clicked(object sender, EventArgs e)
    {
        readGenerateFile();
    }
    private async void readGenerateFile()
    {
        CSVReader csvReader = new CSVReader(fileLocationEntry.Text);
        List<string[]> data = csvReader.ReadCSV();


        if (data != null)
        {
            foreach (var dataItem in data)
            {

                System.Diagnostics.Debug.WriteLine(string.Join(",", dataItem));

            }
            if (data != null)
            {
                if (data[1] != null)
                {
                    EPC_Entry.Text = data[1][0];
                    UserData_Entry.Text = data[1][1];
                    if (data[1][2] != "")
                    {
                        killPassCheckBox.IsChecked = true;
                        KillPass_Entry.Text = data[1][2];
                    }
                    else
                    {
                        killPassCheckBox.IsChecked = false;
                    }
                    AccessPass_Entry.Text = data[1][3];
                    PC_Entry.Text = data[1][4];
                    GeneratorNum_Entry.Text = data[1][5];
                }
            }

        }
        else
        {
            await DisplayAlert("File Error", "File is open or missing. Close file and try again.", "Cancel");
        }
    }
    void OnCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        KillPass_Entry.IsVisible = e.Value !;
    }

    private async void loadBuffer_Clicked(object sender, EventArgs e)
    {
        bool resetBuffer = await DisplayAlert("Load Buffer", "Clear buffer in HSP before loading or add to the buffer?", "Clear Buffer", "Add to Buffer");
        HSPClient client = ((App)Application.Current)._client ;
        string? EPCData = EPC_Entry.Text != null ? EPC_Entry.Text : "";
        string? UserData = UserData_Entry.Text != null ? UserData_Entry.Text : "";
        string? killData = ( KillPass_Entry.Text != null & killPassCheckBox.IsChecked )? KillPass_Entry.Text : "";
        string? AccData = UserData_Entry.Text != null ? UserData_Entry.Text : "";
        string? PCData = PC_Entry.Text != null ? PC_Entry.Text : "";
        if (EPCData.Length>=4 & EPC_SeqeuntialCheck.IsChecked )
        {
            System.Diagnostics.Debug.WriteLine("EPC Checked");
            EPCData = EPCData.Insert(EPCData.Length - 4, "!");
            System.Diagnostics.Debug.WriteLine(EPCData);
        }
        if (UserData.Length >= 4 & USR_SeqeuntialCheck.IsChecked)
        {
            UserData = UserData.Insert(UserData.Length - 4, "!");
        }

        string?[] bufferCmd = [EPCData, UserData, killData, AccData,PCData];    
        int numofItems = GeneratorNum_Entry.Text != null ? int.Parse(GeneratorNum_Entry.Text): -1;
        client.GenerateBuffer(bufferCmd, numofItems, resetBuffer);

    }
}
