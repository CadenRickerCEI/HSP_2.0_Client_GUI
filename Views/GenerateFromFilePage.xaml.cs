namespace HSPGUI.Views;
using HSPGUI.Resources;
using System.Text;
using System.IO;
using CommunityToolkit.Maui.Core;
using Microsoft.Maui.Storage;
using CommunityToolkit.Maui.Storage;

// using Windows.Media.Protection.PlayReady;

/// <summary>
/// The GenerateFromFilePage class is a Xamarin.Forms ContentPage that provides a user interface
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
    /// <value>
    /// Status of the USR fields validity
    /// </value>
    private bool USR_Valid;
    /// <value>
    /// Status of the KIL fields validity
    /// </value>
    private bool KIL_Valid;
    /// <value>
    /// Status of the ACC fields validity
    /// </value>
    private bool ACC_Valid;
    /// <value>
    /// Status of the PCW fields validity
    /// </value>
    private bool PCW_Valid;
    private CancellationTokenSource cancellationTokenSource;
    /// <value>
    /// The constructor for the GenerateFromFilePage class. It connects the HSP client variable to the one in the main app,
    /// sets the initial validity of input fields, and configures the UI elements.
    /// </value>
    public GenerateFromFilePage()
    {
        if (((App)Application.Current!) != null)
        {
            client = ((App)Application.Current)._client;
        }

        cancellationTokenSource = new CancellationTokenSource();
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
        if (EPC_Entry.Text != null && EPC_Entry.Text != "" && client != null)
        {
            EPC_Valid = client.validateInput(EPC_Entry.Text, EPC_Entry.Text.Length, false);
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
        if (UserData_Entry.Text != null && UserData_Entry.Text != "" && client != null)
        {
            USR_Valid = client.validateInput(UserData_Entry.Text, UserData_Entry.Text.Length, false);
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
            KIL_Valid = client.validateInput(KillPass_Entry.Text, 8, false);
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
            ACC_Valid = client.validateInput(AccessPass_Entry.Text, 8, false);
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
            PCW_Valid = client.validateInput(PC_Entry.Text, 4, false);
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
            var killData = (KillPass_Entry.Text != null && killPassCheckBox.IsChecked)  ? KillPass_Entry.Text : "";
            var AccData = AccessPass_Entry.Text != null  ? AccessPass_Entry.Text : "";
            var PCData = PC_Entry.Text != null ? PC_Entry.Text : "";

            EPCData = (EPCData.Length >= 4 && EPC_SeqeuntialCheck.IsChecked) ? EPCData.Insert(EPCData.Length - 4, "!") : EPCData;
            UserData = (UserData.Length >= 4 && USR_SeqeuntialCheck.IsChecked) ? UserData.Insert(UserData.Length - 4, "!") : UserData;

            string[] bufferCmd = [EPCData, UserData, killData, AccData, PCData];
            var numofItems = GeneratorNum_Entry.Text != null ? int.Parse(GeneratorNum_Entry.Text) : -1;

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
}