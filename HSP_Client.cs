using HSPGUI.Resources;
using MinimalisticTelnet;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
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
    /// <summary>
    /// Occurs when the connection status changes.
    /// </summary>
    public event Action<bool>? connectionStatusChanged;

    /// <summary>
    /// Array of data types used in buffer commands.
    /// </summary>
    private string[] dataTypes = new string[] { "EPC", "USR", "KIL", "ACC", "PCW" };

    /// <summary>
    /// Initializes a new instance of the HSPClient class.
    /// </summary>
    public HSPClient() => _client = null;
    /// <summary>
    /// String used when connection is lost.
    /// </summary>
    private string connectionLost = "Connection Lost";
    /// <summary>
    /// Connects to the HSP telnet server.
    /// </summary>
    /// <param name="IpAddress">The IP address of the server.</param>
    /// <param name="Port">The port number of the server.</param>
    public string connectToHSP(string IpAddress, int Port)
    {
        try
        {
            _client = new TelnetConnection(IpAddress, Port);
            connectionStatusChanged?.Invoke(isConnected());
            return readServerMSg();
        }
        catch
        {
            return "HSP conncetion failed. Check that HSP is powered on.";
        }
    }
    /// <summary>
    ///  reads server message and formats the message
    /// </summary>
    /// <returns>Formated message from the server.</returns>
    public string readServerMSg()
    {
        try
        {
            string? result = _client?.Read();
            if (result != null)
            {
                result = result.Trim();
                string pattern = @"\s*HSPSA>$";
                return (result.Length > 0) ? "HSPSA>" + Regex.Replace(result, pattern, "") : "";
            }
            return "";
        }
        catch
        {
            return connectionLost;
        }        
    }

    /// <summary>
    /// Generates a buffer command data string.
    /// </summary>
    /// <param name="bufferCmdData">Array of command data strings.</param>
    /// <param name="numberOfTags">The number of tags to be added to the buffer.</param>
    /// <param name="resetBuffer">Indicates whether to reset the buffer.</param>
    public async Task<string> GenerateBuffer(string?[] bufferCmdData, int numberOfTags, bool resetBuffer)
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
                    try
                    {
                        _client.WriteLine("RESETBUFFER");
                        HSPResponse += readServerMSg() + "\n";
                    }
                    catch
                    {
                        HSPResponse = connectionLost;
                    }                    
                }
            }

            //System.Diagnostics.Debug.WriteLine(command);

            if (_client is not null && isConnected())
            {
                try
                {
                    _client.WriteLine($"{command}");
                }
                catch
                {
                    return "Connection Lost";
                }
                string result = readServerMSg();
                System.Diagnostics.Debug.Write($"{result}");

                if (result != null && result.Contains("GENERATED 0 RECORDS"))
                {
                    HSPResponse += result + "\n";
                }
                else
                {
                    await Task.Delay(1000);//Delay to give the HSP a chance to process the data. 
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
    public async Task<string> LoadFromFile(string file, IProgress<double> progress, bool resetBuffer, LoadFromFilePage page)
    {
        string HSPInfo = "";

        if (resetBuffer)
        {
            System.Diagnostics.Debug.WriteLine("RESETBUFFER");

            if (_client is not null && isConnected())
            {
                try
                {
                    _client.WriteLine("RESETBUFFER");
                }
                catch
                {
                    return connectionLost;
                }
                
                HSPInfo = readServerMSg() + "\n";
                page.updatedialog(HSPInfo);
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

                    if (data[i][j].Length > 0)
                    {
                        string errMessage = validateInput(data[i][j], length, false); 
                        if (errMessage != "")
                        {
                            //System.Diagnostics.Debug.WriteLine($"Invalid entry {dataTypes[j]} on row {i}");
                            return  $"File loading halted.\nError on Line {i}\n {dataTypes[j]} {errMessage}\n";
                        }
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
                        command = command.EndsWith(",") ? command.Substring(0, Math.Max(command.Length - 1, 1)) : command;
                        try
                        {
                            _client.WriteLine($"{command}");
                        }
                        catch
                        {
                            return connectionLost;
                        }                        
                    }
                }
                else
                {
                    return $"Line {i} is Empty or invalid";
                }

                if (i % (data.Count / Math.Min(200, data.Count)) == 0)
                {
                    var progressVal = (double)i / (double)(data.Count - 1);
                    progress.Report(progressVal);
                    if (!isConnected()) await Task.Delay(20);
                }
            }

            if (_client is not null && isConnected())
            {
                //string writtenMsg = Regex.Replace(readServerMSg(), @"\r\n\r\n", "\n") ;
                string writtenMsg = readServerMSg();
                string[] lines = writtenMsg.Split(new[] { "\r\n\r\n" }, StringSplitOptions.None);
                string message = string.Join("\n", lines, 0, Math.Min(lines.Length, 100));
                
                if (lines.Length > 100)
                {
                    var secondPart = new string[100];
                    Array.Copy(lines, lines.Length - secondPart.Length, secondPart, 0,secondPart.Length);                  
                    
                    message += "\n...\n" + string.Join("\n", secondPart, 0, secondPart.Length);
                }
                page.updatedialog(HSPInfo + message);
            }

            progress.Report(1.0);
            System.Diagnostics.Debug.WriteLine("completed loading");
            return "";
        }
        catch (Exception e)
        {
            System.Diagnostics.Debug.WriteLine(e.ToString());
            return "File is missing, open in another program, or empty.";
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
    public String validateInput(string input, int length, bool sequential)
    {
        var regExpresion = (sequential) ? "^[A-Fa-f0-9!]*$" : "^[A-Fa-f0-9]*$";
        var regex = new Regex(regExpresion);
        if (length > 32) length = sequential ? 32 : 33;
        bool isMultipleOfFour = (sequential  && input.Contains("!") ? input.Length - input.Count(c => c =='!') : input.Length)%4 ==0;
        string result = "";
        if (!regex.IsMatch(input))
        {
            //System.Diagnostics.Debug.WriteLine("bad expression");
            result = "\tData contains invalid characters.\n";
        }
        if(input.Length != length)
        {
            //System.Diagnostics.Debug.WriteLine($"length incorrect expected length {length} found length {input.Length}");
            result += $"\tLength of data must equal {length}.\n";
        }
        if(!isMultipleOfFour)
        {
            result += "\tLength of data must be a multiple of four.\n";
        }
        return result;
    }

    /// <summary>
    /// Validates that the tag is hexidecimal unless sequential is true then ! is allowed
    /// </summary>
    /// <returns> True if it is connected to the HSP.</returns>
    public bool isConnected()
    {
        var connected = _client != null && _client.IsConnected;
        connectionStatusChanged?.Invoke(connected);
        return connected;
    }

    /// <summary>
    /// reads any lingering messages then 
    /// gets the current buffer count in HSP.
    /// </summary>
    /// <returns> string array containing the count followed by the messages reviced.</returns>
    public string[] getBufferCount()
    {
        if (_client != null && isConnected())
        {
            var message = readServerMSg();
            _client.WriteLine("GETBUFFERCOUNT\n");
            var result = readServerMSg();
            message += result;
            result = result is not null ? result : "";
            int count = -1;
            if (result != "")
            {
                MatchCollection matches = Regex.Matches(result, @"\d+");
                int sum = 0;
                foreach (Match match in matches)
                    sum += int.Parse(match.Value);            
                count = sum;
            }
            else
            {
                count = 0;
            }
            return [count.ToString(), result];
        }
        return ["0", "Buffer invalid"];
    }

    /// <summary>
    /// 
    /// </summary>
    public string EngageHSP()
    {
        if (_client != null && isConnected())
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
    public string writeUsrCMD(string cmd)
    {
        if (_client != null && isConnected())
        {
            _client.WriteLine($"{cmd}");
            var message = readServerMSg();
            Task.Delay(100).Wait();
            return message +"\n"+ readServerMSg();
        }        
        return "Not connected to HSP";
    }
}