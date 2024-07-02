//using Windows.Media.Protection.PlayReady;

namespace HSPGUI.Views;

public partial class LoadFromFilePage : ContentPage
{
    private HSPClient client;
    private Progress<double> _progress;
    public LoadFromFilePage()
    {
        client = ((App)Application.Current)._client;
        _progress = new Progress<double>();
        InitializeComponent();
        loadFileBtn.IsVisible = false;        
        _progress.ProgressChanged += (s, e) =>
        {
            //System.Diagnostics.Debug.WriteLine(e);
            progressBar.Progress = e;
        };

    }
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

    private async void loadFileBtn_Clicked(object sender, EventArgs e)
    {     
        progressBar.Progress = 0;
        bool resetBuffer = await DisplayAlert("Clear HSP Buffer", "Press reset to clear the buffer or add to add to the buffer", "Reset", "Add");
        progressBar.IsVisible = true;
        if (_progress != null)
        {
            await Task.Run(() => { Task task = client.LoadFromFile(fileLocationEntry.Text, _progress, resetBuffer); });
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("progress null");
        }        
        await DisplayAlert("File Loading Complete", "File has been loaded into the HSP.", "Ok");
        progressBar.Progress = 0;
        progressBar.IsVisible = false;
    }

    private void fileLocationEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (fileLocationEntry.Text != null)
        {
            // Display the file location in the Entry
            loadFileBtn.IsVisible = File.Exists(fileLocationEntry.Text);
        }
    }
}
