namespace HSPGUI.Views;
using HSPGUI.Resources;
using Microsoft.Maui.Graphics.Text;
//using Windows.Media.Protection.PlayReady;


public partial class GenerateFromFilePage : ContentPage
{
    private HSPClient? client ;
    private bool EPC_Valid;
    private bool USR_Valid;
    private bool KIL_Valid;
    private bool ACC_Valid;
    private bool PCW_Valid;
    public GenerateFromFilePage()
    {
        
        if (((App)Application.Current!) != null)
        {
            client = ((App)Application.Current)._client ;
        }
        
        InitializeComponent();
        EPC_Valid = true;
        USR_Valid = true;
        KIL_Valid = true;
        ACC_Valid = true;
        PCW_Valid = true;
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
        List<string[]>? data = csvReader.ReadCSV();        
        if (data != null)
        {
            foreach (var dataItem in data)
            {
                System.Diagnostics.Debug.WriteLine(string.Join(",", dataItem));
            }
            if (data != null && data[1] != null && data[1].Length > 5)
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
            else
            {
                await DisplayAlert("File Invalid", "Make sure the file has 6 columms and data is on the second row.", "Cancel");
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
    void OnEPC_EntryTextChanged(object sender, TextChangedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("EPC Changed");
        if (EPC_Entry.Text != null && client != null)
        {
            EPC_Valid = client.validateInput(EPC_Entry.Text, EPC_Entry.Text.Length, false); 
            EPC_Entry.TextColor = EPC_Valid ? Color.FromArgb("#000000") : Color.FromArgb("#FF0000");
        }
    }
    void OnUSR_EntryTextChanged(object sender, TextChangedEventArgs e)
    {
        if (UserData_Entry.Text != null && client != null)
        {
            USR_Valid = client.validateInput(UserData_Entry.Text, UserData_Entry.Text.Length, false);
            UserData_Entry.TextColor = USR_Valid ? Color.FromArgb("#000000") : Color.FromArgb("#FF0000");
        }
    }
    void OnKIL_EntryTextChanged(object sender, TextChangedEventArgs e)
    {
        if (KillPass_Entry.Text != null && client != null)
        {
            KIL_Valid = client.validateInput(KillPass_Entry.Text, 8, false);
            KillPass_Entry.TextColor = KIL_Valid ? Color.FromArgb("#00000") : Color.FromArgb("#FF0000");
        }
    }
    void OnACC_EntryTextChanged(object sender, TextChangedEventArgs e)
    {
        if (AccessPass_Entry.Text != null && client != null)
        {
            ACC_Valid = client.validateInput(AccessPass_Entry.Text, 8, false);
            AccessPass_Entry.TextColor = ACC_Valid ? Color.FromArgb("#000000") : Color.FromArgb("#FF0000");
        }
    }
    void OnPCW_EntryTextChanged(object sender, TextChangedEventArgs e)
    {
        if (PC_Entry.Text != null && client != null)
        {
            PCW_Valid = client.validateInput(PC_Entry.Text, 4, false);
            PC_Entry.TextColor = PCW_Valid ? Color.FromArgb("#000000") : Color.FromArgb("#FF0000");
        }
    }

    private async void loadBuffer_Clicked(object sender, EventArgs e)
    {
        if (EPC_Valid && USR_Valid && (KIL_Valid || killPassCheckBox.IsChecked == false) && ACC_Valid && PCW_Valid)
        {
            bool resetBuffer = await DisplayAlert("Load Buffer", "Clear buffer in HSP before loading or add to the buffer?", "Clear Buffer", "Add to Buffer");
            string EPCData = EPC_Entry.Text != null ? EPC_Entry.Text : "";
            string UserData = UserData_Entry.Text != null ? UserData_Entry.Text : "";
            string killData = (KillPass_Entry.Text != null && killPassCheckBox.IsChecked) ? KillPass_Entry.Text : "";
            string AccData = UserData_Entry.Text != null ? UserData_Entry.Text : "";
            string PCData = PC_Entry.Text != null ? PC_Entry.Text : "";  
            
            EPCData = (EPCData.Length >= 4 && EPC_SeqeuntialCheck.IsChecked) ? EPCData.Insert(EPCData.Length - 4, "!"):EPCData; 
            UserData = (UserData.Length >= 4 && USR_SeqeuntialCheck.IsChecked)? UserData.Insert(UserData.Length - 4, "!"): UserData;
            
            string[] bufferCmd = [EPCData, UserData, killData, AccData, PCData];
            int numofItems = GeneratorNum_Entry.Text != null ? int.Parse(GeneratorNum_Entry.Text) : -1;
            if (client != null)
            {
                client.GenerateBuffer(bufferCmd, numofItems, resetBuffer);
            }
            
        }
        else
        {
            await DisplayAlert("Invalid Information", "Fix data highlighted in red", "cancel");
        }
    }



    private void EPC_Entry_Completed(object sender, EventArgs e)
    {
        UserData_Entry.Focus();
    }


    private void UserData_Entry_Completed(object sender, EventArgs e)
    {
        KillPass_Entry.Focus();
    }
    private void KillPass_Entry_Completed(object sender, EventArgs e)
    {
        AccessPass_Entry.Focus();
    }

    private void AccessPass_Entry_Completed(object sender, EventArgs e)
    {
        PC_Entry.Focus();
    }

    private void PC_Entry_Completed(object sender, EventArgs e)
    {
        GeneratorNum_Entry.Focus();
    }
}
