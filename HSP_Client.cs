using HSPGUI.Resources;
using System.Text.RegularExpressions;

/// <summary>
/// The HSPClient class is used for managing the connection and communication with a Telnet server.
/// It also validates the data going into the database.
/// </summary>
public class HSPClient
{
    /// <summary>
    /// The TelnetClient object used for the connection.
    /// </summary>
    private TelnetClient _client;

    /// <summary>
    /// Indicates whether the client is connected to the server.
    /// </summary>
    public bool _connected { get; private set; }
    public event Action<bool>? connectionStatusChanged;

    /// <summary>
    /// Array of data types used in buffer commands.
    /// </summary>
    private string[] dataTypes = new string[] { "EPC", "USR", "KIL", "ACC", "PCW" };

    /// <summary>
    /// Initializes a new instance of the HSPClient class.
    /// </summary>
    public HSPClient()
    {
        _client = new TelnetClient();
        _connected = false;
    }

    /// <summary>
    /// Connects to the HSP telnet server.
    /// </summary>
    /// <param name="IpAddress">The IP address of the server.</param>
    /// <param name="Port">The port number of the server.</param>
    public async Task connectToHSP(string IpAddress, int Port)
    {
        await _client.ConnectAsync(IpAddress, Port);
        _connected = _client.connected;
        connectionStatusChanged?.Invoke(_client.connected);
    }

    /// <summary>
    /// Generates a buffer command data string.
    /// </summary>
    /// <param name="bufferCmdData">Array of command data strings.</param>
    /// <param name="numberOfTags">The number of tags to be added to the buffer.</param>
    /// <param name="resetBuffer">Indicates whether to reset the buffer.</param>
    public async void GenerateBuffer(string?[] bufferCmdData, int numberOfTags, bool resetBuffer)
    {
        var command = "GENERATE=";
        var i = 0;

        foreach (var data in bufferCmdData)
        {
            if (data != "") command += dataTypes[i] + data;
            i++;
        }

        command += "NUM" + numberOfTags.ToString();

        if (resetBuffer)
        {
            System.Diagnostics.Debug.WriteLine("RESETBUFFER");

            if (_connected)
            {
                await _client.WriteAsync("RESETBUFFER");
            }
        }

        System.Diagnostics.Debug.WriteLine(command);
        if (_connected) await _client.WriteAsync(command);
    }

    /// <summary>
    /// Loads the indvidual tags from a CSV file into the HSP buffer.
    /// </summary>
    /// <param name="file">The path to the CSV file.</param>
    /// <param name="progress">Progress reporter for the loading operation.</param>
    /// <param name="resetBuffer">Indicates whether to reset the buffer of tags already on the HSP.</param>
    /// <returns>An integer indicating the result of the operation.</returns>
    public async Task<int> LoadFromFile(string file, IProgress<double> progress, bool resetBuffer)
    {
        if (resetBuffer)
        {
            System.Diagnostics.Debug.WriteLine("RESETBUFFER");

            if (_connected)
            {
                await _client.WriteAsync("RESETBUFFER");
            }
        }

        try
        {
            var csvReader = new CSVReader(file);
            var data = csvReader.ReadCSV();
            progress.Report(0);
            if (data == null) throw new Exception();
            var lengths = new int[] { -1, -1, 8, 8, 4 };

            for (int i = 1; i < data.Count; i++)
            {
                var command = "WRITEITEM=";

                for (int j = 0; j < data[i].Length; j++)
                {
                    var length = lengths[j] < 0 ? data[i][j].Length : lengths[j];

                    if (data[i][j].Length > 0 && validateInput(data[i][j], length, false) == false)
                    {
                        System.Diagnostics.Debug.WriteLine($"Invalid entry {dataTypes[j]} on row {i}");
                        return i;
                    }

                    command += data[i][j].Length > 0 ? dataTypes[j] + data[i][j] : "";
                }

                if (command != "WriteItem=")
                {
                    if (i < -1)
                    {
                        System.Diagnostics.Debug.WriteLine(command);
                    }

                    if (_client.connected)
                    {
                        await _client.WriteAsync(command);
                    }
                }
                else
                {
                    return i;
                }

                if (i % (data.Count / Math.Min(200, data.Count)) == 0)
                {
                    var progressVal = (double)i / (double)(data.Count - 1);
                    progress.Report(progressVal);
                    if (!_client.connected) await Task.Delay(20);
                }
            }

            progress.Report(1.0);
            System.Diagnostics.Debug.WriteLine("completed loading");
            return 0;
        }
        catch
        {
            return -2;
        }
    }

    /// <summary>
    /// Reads the the curretent antena settings from the HSP and returns them as an arrray.
    /// </summary>
    /// <returns>An array of integers representing the antenna settings.</returns>
    public Task<int[]> readAntenaSettings()
    {
        var settings = new int[6];
        return Task.FromResult(settings);
    }

    /// <summary>
    /// Writes antenna settings to the HSP.
    /// </summary>
    /// <param name="Settings">An array of integers representing the antenna settings.</param>
    public async Task writeAntenaSettings(int[] Settings)
    {
        if (_client != null && _client.connected)
        {
            await _client.WriteAsync("NB");
        }
    }

    /// <summary>
    /// Validates that the tag is hexidecimal unless sequential is true then ! is allowed
    /// </summary>
    /// <param name="input">The input string to validate.</param>
    /// <param name="length">The expected length of the input string.</param>
    /// <param name="sequential">Indicates whether the input is sequential.</param>
    /// <returns>True if the input is valid; otherwise, false.</returns>
    public bool validateInput(string input, int length, bool sequential)
    {
        var regExpresion = (sequential) ? "^[A-Fa-f0-9!]*$" : "^[A-Fa-f0-9]*$";
        var regex = new Regex(regExpresion);
        if (length > 32) length = sequential ? 32 : 33;

        if (!regex.IsMatch(input))
        {
            System.Diagnostics.Debug.WriteLine("bad expression");
        }

        if (input.Length != length)
        {
            System.Diagnostics.Debug.WriteLine($"length incorrect expected length {length} found length {input.Length}");
        }

        return regex.IsMatch(input) && input.Length == length;
    }

}