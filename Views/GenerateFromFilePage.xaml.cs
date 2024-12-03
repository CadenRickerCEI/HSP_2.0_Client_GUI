namespace HSPGUI.Views;
using HSPGUI.Resources;
using System.Text;
using System.IO;
using CommunityToolkit.Maui.Core;
using Microsoft.Maui.Storage;
using CommunityToolkit.Maui.Storage;


/// <summary>
/// The GenerateFromFilePage class is a xaml.Forms ContentPage that provides a user interface
/// for generating data from a CSV file and validating user input for various fields.
/// </summary>
public partial class GenerateFromFilePage : ContentPage
{
    /// <summary>
    /// A nullable HSPClient used to interact with the antenna.
    /// </summary>
    private HSPClient? client;
    /// <value>
    /// Boolean flags to indicate the validity of various input fields.
    /// </value>
    private bool EPC_Valid;
    private string EPC_invalidReason;
    /// <value>
    /// Status of the USR fields validity
    /// </value>
    private bool USR_Valid;
    private string USR_invalidReason;
    /// <value>
    /// Status of the KIL fields validity
    /// </value>
    private bool KIL_Valid;
    private string KIL_invalidReason;
    /// <value>
    /// Status of the ACC fields validity
    /// </value>
    private bool ACC_Valid;
    private string ACC_invalidReason;
    /// <value>
    /// Status of the PCW fields validity
    /// </value>
    private bool PCW_Valid;
    private string PCW_invalidReason;
    private CancellationTokenSource cancellationTokenSource;
    private bool visable = false;
    /// <value>
    /// The constructor for the GenerateFromFilePage class. It connects the HSP client variable to the one in the main app,
    /// sets the initial validity of input fields, and configures the UI elements.
    /// </value>
    public GenerateFromFilePage()
    {
        client = HSPClient.Instance;

        cancellationTokenSource = new CancellationTokenSource();
        client.cmdUpdated += cmdDataUpdated;
        InitializeComponent();
        EPC_Valid = true;
        USR_Valid = true;
        KIL_Valid = true;
        ACC_Valid = true;
        PCW_Valid = true;
        EPC_invalidReason = "";
        USR_invalidReason = "";
        KIL_invalidReason = "";
        ACC_invalidReason = "";
        PCW_invalidReason = "";
        updateFromFileBtn.IsVisible = false;
        EPC_Entry.MinimumWidthRequest = EPC_Entry.FontSize * Constants.fontToWidthScale * (double)Constants.MaxLenEPC_hex;
        UserData_Entry.MinimumWidthRequest = UserData_Entry.FontSize * Constants.fontToWidthScale * (double)Constants.MaxLenUSR_Hex;
        
        if (client != null)
        {
            cmdDIAG.Text = client.cmdbuffer;
            var _ = scrollDIAG.ScrollToAsync(0, cmdDIAG.Height, true);
        }
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        visable = true;
    }
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        visable = false;
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

    /// <summary>
    /// Event handler for the Update From File button click event. It reads data from the selected CSV file
    /// and updates the input fields accordingly.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    private void updateFromFileBtn_Clicked(object sender, EventArgs e) => readGenerateFile();

    /// <summary>
    /// Reads data from the selected CSV file and updates the input fields. Displays alerts if the file is invalid or missing.
    /// </summary>
    private async void readGenerateFile()
    {
        var csvReader = new CSVReader(fileLocationEntry.Text);
        var data = csvReader.ReadCSV();

        if (data != null)
        {
            foreach (var dataItem in data)
                System.Diagnostics.Debug.WriteLine(string.Join(",", dataItem));

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

    /// <summary>
    /// Event handler for the CheckedChanged event of the KillPass checkbox. It toggles the visibility of the KillPass entry field.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    void OnCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        KillPass_Entry.IsVisible = e.Value!;
    }

    /// <summary>
    /// Event handler for the TextChanged event of the EPC entry field. It validates the input and updates the text color based on validity.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    void OnEPC_EntryTextChanged(object sender, TextChangedEventArgs e)
    {
        EPC_Valid = true;
        EPC_invalidReason = "";

        if (EPC_Entry.Text != null && EPC_Entry.Text != "" && client != null)
        {
            EPC_invalidReason = client.validateInput(EPC_Entry.Text, EPC_Entry.Text.Length, true);
            EPC_Valid = EPC_invalidReason == "";
            EPC_Entry.TextColor = EPC_Valid ? Color.FromArgb("#000000") : Color.FromArgb("#FF0000");
        }
    }

    /// <summary>
    /// Event handler for the TextChanged event of the UserData entry field. It validates the input and updates the text color based on validity.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    void OnUSR_EntryTextChanged(object sender, TextChangedEventArgs e)
    {
        USR_Valid = true;
        USR_invalidReason = "";

        if (UserData_Entry.Text != null && UserData_Entry.Text != "" && client != null)
        {
            USR_invalidReason = client.validateInput(UserData_Entry.Text, UserData_Entry.Text.Length, true);
            USR_Valid = USR_invalidReason == "";
            UserData_Entry.TextColor = USR_Valid ? Color.FromArgb("#000000") : Color.FromArgb("#FF0000");
        }
    }

    /// <summary>
    /// Event handler for the TextChanged event of the KillPass entry field. It validates the input and updates the text color based on validity.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    void OnKIL_EntryTextChanged(object sender, TextChangedEventArgs e)
    {
        KIL_Valid = true;

        if (KillPass_Entry.Text != null && KillPass_Entry.Text != "" && client != null)
        {
            KIL_invalidReason = client.validateInput(KillPass_Entry.Text, 8, false);
            KIL_Valid = KIL_invalidReason == "";
            KillPass_Entry.TextColor = KIL_Valid ? Color.FromArgb("#00000") : Color.FromArgb("#FF0000");
        }
    }

    /// <summary>
    /// Event handler for the TextChanged event of the AccessPass entry field. It validates the input and updates the text color based on validity.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    void OnACC_EntryTextChanged(object sender, TextChangedEventArgs e)
    {
        ACC_Valid = true;

        if (AccessPass_Entry.Text != null && AccessPass_Entry.Text != "" && client != null)
        {
            ACC_invalidReason = client.validateInput(AccessPass_Entry.Text, 8, false);
            ACC_Valid = ACC_invalidReason == "";
            AccessPass_Entry.TextColor = ACC_Valid ? Color.FromArgb("#000000") : Color.FromArgb("#FF0000");
        }
    }

    /// <summary>
    /// Event handler for the TextChanged event of the PC entry field. It validates the input and updates the text color based on validity.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    void OnPCW_EntryTextChanged(object sender, TextChangedEventArgs e)
    {
        PCW_Valid = true;

        if (PC_Entry.Text != null && PC_Entry.Text != "" && client != null)
        {
            PCW_invalidReason = client.validateInput(PC_Entry.Text, 4, false);
            PCW_Valid = PCW_invalidReason == "";
            PC_Entry.TextColor = PCW_Valid ? Color.FromArgb("#000000") : Color.FromArgb("#FF0000");
        }
    }

    /// <summary>
    /// Event handler for the Load Buffer button click event. It validates the input fields and sends the data to the client to generate a buffer.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    private async void loadBuffer_Clicked(object sender, EventArgs e)
    {
        if (EPC_Valid && USR_Valid && (KIL_Valid || killPassCheckBox.IsChecked == false) && ACC_Valid && PCW_Valid)
        {
            var resetBuffer = await DisplayAlert("Load Buffer", "Clear buffer in HSP before loading or add to the buffer?", "Clear Buffer", "Add to Buffer");
            var EPCData = EPC_Entry.Text != null ? EPC_Entry.Text : "";
            var UserData = UserData_Entry.Text != null ? UserData_Entry.Text : "";
            var killData = (KillPass_Entry.Text != null && killPassCheckBox.IsChecked) ? KillPass_Entry.Text : "";
            var AccData = AccessPass_Entry.Text != null ? AccessPass_Entry.Text : "";
            var PCData = PC_Entry.Text != null ? PC_Entry.Text : "";

            string[] bufferCmd = [EPCData, UserData, killData, AccData, PCData];
            var numofItems = GeneratorNum_Entry.Text != null ? int.Parse(GeneratorNum_Entry.Text) : -1;

            if (client != null)
            {
                var _ = await client.GenerateBuffer(bufferCmd, numofItems, resetBuffer);
                var msg = await client.readServerMSg(true);
                cmdDIAG.Text = client.cmdbuffer;
            }
        }
        else
        {
            string errorMessage = "";

            if (!EPC_Valid)
            {
                errorMessage += $"EPC Invalid:\n{EPC_invalidReason}";
            }

            if (!USR_Valid)
            {
                errorMessage += $"USR Invalid:\n{USR_invalidReason}";
            }

            if (!KIL_Valid)
            {
                errorMessage += $"KIL Invalid:\n{KIL_invalidReason}";
            }

            if (!ACC_Valid)
            {
                errorMessage += $"ACC Invalid:\n{ACC_invalidReason}";
            }

            if (!PCW_Valid)
            {
                errorMessage += $"PCW Invalid:\n{PCW_invalidReason}";
            }

            await DisplayAlert("Invalid Information", errorMessage, "Cancel");
        }
    }

    /// <summary>
    /// Event handler for the Completed event of the EPC entry field. It sets the focus to the UserData entry field.
    /// This functions is added to improve the flow of filling in the form.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    private void EPC_Entry_Completed(object sender, EventArgs e) => UserData_Entry.Focus();

    /// <summary>
    /// Event handler for the Completed event of the UserData entry field. It sets the focus to the KillPass entry field.
    /// This functions is added to improve the flow of filling in the form.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    private void UserData_Entry_Completed(object sender, EventArgs e) => KillPass_Entry.Focus();

    /// <summary>
    /// Event handler for the Completed event of the KillPass entry field. It sets the focus to the AccessPass entry field.
    /// This functions is added to improve the flow of filling in the form.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    private void KillPass_Entry_Completed(object sender, EventArgs e) => AccessPass_Entry.Focus();

    /// <summary>
    /// Event handler for the Completed event of the AccessPass entry field. It sets the focus to the PC entry field.
    /// This functions is added to improve the flow of filling in the form.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    private void AccessPass_Entry_Completed(object sender, EventArgs e) => PC_Entry.Focus();

    /// <summary>
    /// Event handler for the Completed event of the PC entry field. It sets the focus to the GeneratorNum entry field.
    /// This functions is added to improve the flow of filling in the form.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    private void PC_Entry_Completed(object sender, EventArgs e) => GeneratorNum_Entry.Focus();

    /// <summary>
    /// Event handler for the Generate File button click event. It writes the data from the edit tag data section to a file for the
    /// user to use a template or create a new file.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments.</param>
    private async void GenerateFileClicked(object sender, EventArgs e)
    {
        var EPCData = EPC_Entry.Text != null ? EPC_Entry.Text : "";
        var UserData = UserData_Entry.Text != null ? UserData_Entry.Text : "";
        var killData = (KillPass_Entry.Text != null && killPassCheckBox.IsChecked) ? KillPass_Entry.Text : "";
        var AccData = UserData_Entry.Text != null ? UserData_Entry.Text : "";
        var PCData = PC_Entry.Text != null ? PC_Entry.Text : "";
        var numofItems = GeneratorNum_Entry.Text != null ? GeneratorNum_Entry.Text : "100";
        var csv = new StringBuilder();
        csv.AppendLine("EPC Data,User Data,Kill Password,Accesss Password,PC Word,Number Of Tags");
        csv.AppendLine($"{EPCData},{UserData},{killData},{AccData},{PCData},{numofItems}");
        DateTime now = DateTime.Now;
        // Convert the CSV string to bytes
        var csvBytes = Encoding.UTF8.GetBytes(csv.ToString());
        using var stream = new MemoryStream(csvBytes);

        {
            var result = await FileSaver.Default.SaveAsync($"GenerateTag{now.ToString("yy")}{now.ToString("MM")}{now.ToString("dd")}{now.ToString("HH")}{now.ToString("mm")}.csv",
                                                            stream, cancellationTokenSource.Token);
        }
    }
    private void cmdDataUpdated(bool updated)
    {
        if (updated && client != null)
        {
            cmdDIAG.Text = client.cmdbuffer;
            var _ = scrollDIAG.ScrollToAsync(0, cmdDIAG.Height ,true);
        }
    }

    private async void tagDataHelp_Clicked(object sender, EventArgs e)
    {
        await DisplayAlert("Help: Edit Tag Data", "Tag data may be eddited here before " +
            "loading the HSP or saving to file.\nDefinitions:\n" +
            "EPC Data: Data area for storing " +
            "identification information. May contain 0-9, A-F, and !. " +
            "The four characters following an ! will be incremented by one on every tag.\n" +
            "USR Data: Data for the user area of the tag. May contain 0-9, A-F, and !. " +
            "The four characters following an ! will be incremented by one on every tag.\n" +
            "Kill Password: Permanently disables the tag with the correct password, " +
            "after which it stops responding to any further commands or reads.\n" +
            "Access Password: Locks the tag from unauthorized reading.\n" +
            "PC Word: Specifies the length of the EPC. Optional.\n" +
            "Number of Tags to Generate: Determines how many tags will be added to the buffer."
            , "Done");
    }

    private async void fileSelectHelp_Clicked(object sender, EventArgs e)
    {
        await DisplayAlert("Help: File Select", "Press 'Select File' to choose a file as a source.\n" +
            "After selecting a file, read file will load the file into the edit tag data section.\n" +
            "Once the tag data has been validated it is ready to be loaded to the HSP by pressing" +
            " 'Load to HSP Buffer' or saved to a file by pressing 'Save Tag Data to file'", "Done");
    }
}