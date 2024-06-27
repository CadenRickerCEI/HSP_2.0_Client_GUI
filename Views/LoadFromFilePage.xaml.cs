namespace HSPGUI.Views;

public partial class LoadFromFilePage : ContentPage
{
    public LoadFromFilePage()
    {
        InitializeComponent();
        loadFileBtn.IsVisible = false;

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
}
