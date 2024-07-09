// using Windows.Media.Protection.PlayReady;

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
    /// <summary>
    /// The constructor for the LoadFromFilePage class. It initializes the client field,
    /// sets up the progress tracking, and configures the UI elements.
    /// </summary>
    public LoadFromFilePage()
    {
        if (((App)Application.Current!) != null)
        {
            client = ((App)Application.Current)._client;
        }

        _progress = new Progress<double>();
        InitializeComponent();
        loadFileBtn.IsVisible = false;

        _progress.ProgressChanged += (s, e) =>
        {
            // System.Diagnostics.Debug.WriteLine(e);
            progressBar.Progress = e;
        };
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
        var loadResult = -1;

        if (_progress != null && client != null)
        {
            loadResult = await Task.Run(() =>
            {
                return client.LoadFromFile(fileLocationEntry.Text, _progress, resetBuffer);
            });

            System.Diagnostics.Debug.WriteLine("file complete");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("progress null");
        }

        if (loadResult == 0)
        {
            await DisplayAlert("File Loading Complete", "File has been loaded into the HSP.", "Ok");
        }
        else if (loadResult < 0)
        {
            await DisplayAlert("File Error", "File is missing, open in another program, or empty.", "Ok");
        }
        else
        {
            await DisplayAlert("Tag Error", $"Tag error on row {loadResult}. Only tags before this row have been added.", "Ok");
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
            loadFileBtn.IsVisible = File.Exists(fileLocationEntry.Text);
        }
    }
}