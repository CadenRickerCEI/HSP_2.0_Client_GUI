using HSPGUI.Resources;
using MinimalisticTelnet;
using System.Diagnostics.CodeAnalysis;
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
    private TelnetConnection? _client;

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
        _client = null;
    }

    /// <summary>
    /// Connects to the HSP telnet server.
    /// </summary>
    /// <param name="IpAddress">The IP address of the server.</param>
    /// <param name="Port">The port number of the server.</param>
    public string connectToHSP(string IpAddress, int Port)
    {
       _client = new TelnetConnection(IpAddress, Port);  
        connectionStatusChanged?.Invoke(isConnected());
        return readServerMSg();
    }

    private string readServerMSg()
    {
        string? result = _client?.Read();
        if (result != null) {
            result = result.Trim();
            string pattern = @"\s*HSPSA>$";
            return "HSPSA> " + Regex.Replace(result, pattern, "");
        }
        return "";
    }
    /// <summary>
    /// Generates a buffer command data string.
    /// </summary>
    /// <param name="bufferCmdData">Array of command data strings.</param>
    /// <param name="numberOfTags">The number of tags to be added to the buffer.</param>
    /// <param name="resetBuffer">Indicates whether to reset the buffer.</param>
    public async Task<String> GenerateBuffer(string?[] bufferCmdData, int numberOfTags, bool resetBuffer)
    {
        string HSPResponse = "Generate File Failed";
        if (_client != null && _client.IsConnected)
        {
            HSPResponse = "";
            var command = "GENERATE=";
            var i = 0;

            foreach (var data in bufferCmdData)
            {
                if (data != "") command += dataTypes[i] + data + ",";
                i++;
            }

            command += "NUM" + numberOfTags.ToString();

            if (resetBuffer)
            {
                System.Diagnostics.Debug.WriteLine("RESETBUFFER");

                if (_client is not null && isConnected())
                {
                    _client.WriteLine("RESETBUFFER");
                    HSPResponse += readServerMSg() +"\n";
                }
            }

            System.Diagnostics.Debug.WriteLine(command);
            if (_client is not null && isConnected())
            {
                _client.WriteLine($"{command}");
                string result = readServerMSg();
                System.Diagnostics.Debug.Write($"{result}");
                if (result != null && result.Contains("GENERATED 0 RECORDS"))
                {   
                    HSPResponse += result + "\n";
                }
                else
                {
                    await Task.Delay(1000);
                    HSPResponse += readServerMSg() + "\n";
                }
            }           
        }
        return HSPResponse;
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

            if (_client is not null && isConnected())
            {
                _client.WriteLine("RESETBUFFER");
            }
        }

        try
        {
            var csvReader = new CSVReader(file);
            var data = csvReader.ReadCSV();
            progress.Report(0.001);
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

                    command += data[i][j].Length > 0 ? dataTypes[j] + data[i][j] + "," : "";
                }

                if (command != "WriteItem=")
                {
                    if (i < -1)
                    {
                        System.Diagnostics.Debug.WriteLine(command);
                    }

                    if (_client is not null && isConnected())
                    {
                        command = command.EndsWith(",")? command.Substring(0,Math.Max( command.Length-1, 1)) : command;                        
                        _client.WriteLine($"{command}");
                        //var result = _client.Read();
                        //System.Diagnostics.Debug.WriteLine($"{command}\n{result}");
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
                    if (!isConnected()) await Task.Delay(20);
                    else if(_client is not null && !isConnected())
                    {
                        var result = _client.Read();
                    }
                }
            }
            if (_client is not null && isConnected())
            {
                var result = _client.Read();
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
    public void writeAntenaSettings(int[] Settings)
    {
        if (_client != null && isConnected())
        {
            _client.WriteLine("NB");
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
    /// <summary>
    /// Validates that the tag is hexidecimal unless sequential is true then ! is allowed
    /// </summary>
    public bool isConnected()
    {
        var connected = _client != null && _client.IsConnected ;
        connectionStatusChanged?.Invoke(connected);
        return connected;
    }
    /// <summary>
    /// gets the current buffer count in HSP.
    /// </summary>
    public string[] getBufferCount()
    {
        if (_client != null && isConnected())
        {
            _client.WriteLine("GETBUFFERCOUNT\n");
            var result = readServerMSg();
            result = result is not null ? result : "";
            int count = -1;
            //System.Diagnostics.Debug.WriteLine($"{count}");
            if (result != "")
            {
                MatchCollection matches = Regex.Matches(result, @"\d+");
                int sum = 0;

                foreach (Match match in matches)
                {
                    sum += int.Parse(match.Value);
                }
                count = sum;
            }
            else
            {
                count = 0;
            }
            
            return [count.ToString(), result ];
        }
        return ["0","Buffer invalid"];
    }
    /// <summary>
    /// 
    /// </summary>
    public string EngageHSP()
    {
        if ( _client != null && isConnected())
        {
            _client.WriteLine("ENGAGE");
            return readServerMSg();
        }
        return "Engage Failed Not Connected";
    }
    public string disengageHSP()
    {
        if (_client != null && isConnected())
        {
            _client.WriteLine("DISENGAGE");
             return readServerMSg();
        }
        return "Disengage Failed Not Connected";
    }
    public string resetbuffer()
    {
        if (_client != null && isConnected())
        {
            _client.WriteLine("RESETBUFFER");
            return readServerMSg();
        }
        return "Reset Buffer Failed Not Connected";
    }
}