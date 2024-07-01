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
            System.Diagnostics.Debug.WriteLine(e);
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

        if (result != null)
        {
            /*var fileName = result.FileName;
            var fileData = await result.OpenReadAsync();*/
            // Now you can use the file data

            // Display the file location in the Entry
            loadFileBtn.IsVisible = File.Exists(result.FullPath);
            fileLocationEntry.Text = result.FullPath;
        }
    }

    private void loadFileBtn_Clicked(object sender, EventArgs e)
    {
        loadFile();
        

    }
    private async void loadFile()
    {
        progressBar.IsVisible = false;
        if (_progress != null)
        {
            await Task.Run(() => client.LoadFromFile(fileLocationEntry.Text, _progress));

        }
        else
        {
            System.Diagnostics.Debug.WriteLine("progress null");
        }

        progressBar.IsVisible = true;

    }
}
