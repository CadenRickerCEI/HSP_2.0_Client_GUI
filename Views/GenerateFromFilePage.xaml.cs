namespace HSPGUI.Views;
using HSPGUI.Resources;

public partial class GenerateFromFilePage : ContentPage
{
    
	public GenerateFromFilePage()
	{
		InitializeComponent();
        updateFromFileBtn.IsVisible = false;
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
        
        bool loadContents = await DisplayAlert("File Read Complete", "Load Contents into viewer?", "Load", "Cancel");
        if (loadContents)
        {
            foreach (var dataItem in data)
            {
                
                System.Diagnostics.Debug.WriteLine(string.Join(",",dataItem));
            }
        }
    }
    void OnCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        KillPass_Entry.IsVisible = e.Value;
        killPassLabel.IsVisible = e.Value;
    }
}
