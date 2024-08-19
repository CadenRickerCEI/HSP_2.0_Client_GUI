using HSPGUI.Resources;
using Microsoft.Maui.Controls;
using MinimalisticTelnet;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

/// <summary>
/// The HSPClient class is used for managing the connection and communication with a Telnet server.
/// It also validates the data going into the HSP Buffer.
/// </summary>
public class HSPClient
{
    /// <summary>
    /// The TelnetClient object used for the connection.
    /// </summary>
    private TelnetConnection? _clientCMD;
    private TelnetConnection? _clientDATA; //5001
    private TelnetConnection? _clientDIAG; //5003

    /// <summary>
    /// Occurs when the connection status changes.
    /// </summary>
    public event Action<bool>? connectionStatusChanged;
    public event Action<bool>? dialogUpdated;
    public event Action<bool>? dataUpdated;
    /// <summary>
    /// Array of data types used in buffer commands.
    /// </summary>
    private string[] dataTypes = new string[] { "EPC", "USR", "KIL", "ACC", "PCW" };
    private Queue<string> dialog = new Queue<string>();
    /// <summary>
    /// last 100 lines of dialog from the HSP.
    /// </summary>
    public string dialogbuffer = "";
    private Queue<string> data = new Queue<string>();
    /// <summary>
    /// last 100 lines of Data from the HSP.
    /// </summary>
    public string dataBuffer = "";
    /// <summary>
    /// The single instance of the HSPClient class.
    /// </summary>
    private static HSPClient? _instance;
    /// <summary>
    /// Lock object for thread safety.
    /// </summary>
    private static readonly object _lock = new object();
    /// <summary>
    /// Initializes a new instance of the HSPClient class.
    /// </summary>
    private HSPClient() { 
        _clientCMD = null;
        _clientDATA = null;
        _clientDIAG = null;
    }
    /// <summary>
    /// Gets the single instance of the HSPClient class.
    /// </summary>
    public static HSPClient Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new HSPClient();
                    }
                }
            }

            return _instance;
        }
    }
    /// <summary>
    /// String used when connection is lost.
    /// </summary>
    private string connectionLost = "Connection Lost";
    /// <summary>
    /// Connects to the HSP Server
    /// </summary>
    /// <param name="IPAddress"></param>
    /// <param name="PortCMD"></param>
    /// <param name="PortDiag"></param>
    /// <param name="PortDATA"></param>
    /// <returns>String containg message from the HSP or Failed to conect</returns>
    public string connectToHSP(string IPAddress, int PortCMD,
                                int PortDiag, int PortDATA )
    {
        try
        {
            _clientCMD = new TelnetConnection(IPAddress, PortCMD);
            _clientDIAG = new TelnetConnection(IPAddress, PortDiag);
            _clientDATA = new TelnetConnection(IPAddress, PortDATA);
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
            string? result = _clientCMD?.Read();

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

        if (_clientCMD != null && _clientCMD.IsConnected)
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

                if (_clientCMD is not null && isConnected())
                {
                    try
                    {
                        _clientCMD.WriteLine("RESETBUFFER");
                        HSPResponse += readServerMSg() + "\n";
                    }
                    catch
                    {
                        HSPResponse = connectionLost;
                    }
                }
            }

            if (_clientCMD is not null && isConnected())
            {
                try
                {
                    _clientCMD.WriteLine($"{command}");
                }
                catch
                {
                    return "Connection Lost";
                }

                string result = readServerMSg();
                // System.Diagnostics.Debug.Write($"{result}");

                if (result != null && result.Contains("GENERATED 0 RECORDS"))
                {
                    HSPResponse += result + "\n";
                }
                else
                {
                    await Task.Delay(1000);//Delay to give the HSP a chance to process the data. 
                    HSPResponse += result == ""? "":$"{result}\n"+ readServerMSg() + "\n";
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
            if (_clientCMD is not null && isConnected())
            {
                try
                {
                    _clientCMD.WriteLine("RESETBUFFER");
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
                            // System.Diagnostics.Debug.WriteLine($"Invalid entry {dataTypes[j]} on row {i}");
                            return $"File loading halted.\nError on Line {i}\n {dataTypes[j]} {errMessage}\n";
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

                    if (_clientCMD is not null && isConnected())
                    {
                        command = command.EndsWith(",") ? command.Substring(0, Math.Max(command.Length - 1, 1)) : command;

                        try
                        {
                            _clientCMD.WriteLine($"{command}");
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

            if (_clientCMD is not null && isConnected())
            {
                // string writtenMsg = Regex.Replace(readServerMSg(), @"\r\n\r\n", "\n") ;
                string writtenMsg = readServerMSg();
                string[] lines = writtenMsg.Split(new[] { "\r\n\r\n" }, StringSplitOptions.None);
                string message = string.Join("\n", lines, 0, Math.Min(lines.Length, 100));

                if (lines.Length > 100)
                {
                    var secondPart = new string[100];
                    Array.Copy(lines, lines.Length - secondPart.Length, secondPart, 0, secondPart.Length);

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
        if (_clientCMD != null && isConnected())
        {
            _clientCMD.WriteLine("NB");
        }
    }

    /// <summary>
    /// Validates that the tag is hexidecimal unless sequential is true then ! is allowed
    /// </summary>
    /// <param name="input">The input string to validate.</param>
    /// <param name="length">The expected length of the input string.</param>
    /// <param name="sequential">Indicates whether the input is sequential.</param>
    /// <returns>True if the input is valid; otherwise, false.</returns>
    public string validateInput(string input, int length, bool sequential)
    {
        var regExpresion = (sequential) ? "^[A-Fa-f0-9!]*$" : "^[A-Fa-f0-9]*$";
        var regex = new Regex(regExpresion);
        if (length > 32) length = sequential ? 32 : 33;
        bool isMultipleOfFour = (sequential && input.Contains("!") ? input.Length - input.Count(c => c == '!') : input.Length) % 4 == 0;
        string result = "";

        if (!regex.IsMatch(input))
        {
            result = "\tData contains invalid characters.\n";
        }

        if (input.Length != length)
        {
            result += $"\tLength of data must equal {length}.\n";
        }

        if (!isMultipleOfFour)
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
        var connected = _clientCMD != null && _clientCMD.IsConnected;
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
        if (_clientCMD != null && isConnected())
        {
            var message = readServerMSg();
            _clientCMD.WriteLine("GETBUFFERCOUNT\n");
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
    /// When the ENGAGE command is issued the RF is turned 
    /// on, and the trigger line is processed according to
    /// the edge control currently set(rising edge or falling edge).
    /// </summary>
    /// <returns>Server response</returns>
    public string EngageHSP()
    {
        if (_clientCMD != null && isConnected())
        {
            _clientCMD.WriteLine("ENGAGE");
            return readServerMSg();
        }

        return "Engage Failed Not Connected";
    }

    /// <summary>
    /// From a reset the system is in DISENGAGE state. The ENGAGE 
    /// command must be issued before a programming run.When a 
    /// programming run is complete, the disengage command 
    /// should be issued to turn RF power off, and return the 
    /// system to idle state.
    /// </summary>
    /// <returns>Server response</returns>
    public string disengageHSP()
    {
        if (_clientCMD != null && isConnected())
        {
            _clientCMD.WriteLine("DISENGAGE");
            return readServerMSg();
        }

        return "Disengage Failed Not Connected";
    }

    /// <summary>
    /// command will delete all records, and the buffer count
    /// will be set to zero.
    /// </summary>
    /// <returns>Server Message</returns>
    public string resetbuffer()
    {
        if (_clientCMD != null && isConnected())
        {
            _clientCMD.WriteLine("RESETBUFFER");
            return readServerMSg();
        }

        return "Reset Buffer Failed Not Connected";
    }

    /// <summary>
    /// Writes user command directly to the HSP.
    /// </summary>
    /// <param name="cmd">command</param>
    /// <returns>User command followed</returns>
    public string writeUsrCMD(string cmd)
    {
        if (_clientCMD != null && isConnected())
        {
            _clientCMD.WriteLine($"{cmd}");
            var message = readServerMSg();
            Task.Delay(100).Wait();
            return message + "\n" + readServerMSg();
        }

        return "Not connected to HSP";
    }
    public void disconect()
    {
        if (_clientCMD is not null) {
            _clientCMD.Dispose(); }
        if (_clientDATA is not null)
        {
            _clientDATA.Dispose();
        }
        if (_clientDATA is not null)
        {
            _clientDATA.Dispose();
        }
        if (_clientDIAG is not null)
        {
            _clientDIAG.Dispose();
        }

    }

    public async Task readDialogPorts()
    {
        var oldDataBuffer = dataBuffer;
        dataBuffer = await readClient(_clientDATA, data,48);        
        dataUpdated?.Invoke(dataBuffer != oldDataBuffer);
        var oldDialogBuffer = dialogbuffer;
        dialogbuffer = await readClient(_clientDIAG, dialog,300);
        dialogUpdated?.Invoke(dialogbuffer != oldDialogBuffer);
        await Task.Delay(10);
        dialogUpdated?.Invoke(false);
        dataUpdated?.Invoke(false);
    }
    private async Task<string> readClient(TelnetConnection? client, Queue<string> queue, int quesize)
    {
        if (client is not null)
        {
            string? msg = null;
            try
            {
               msg = client.Read();
            }
            catch { }
            if (msg != null) {
                await Task.Run(() =>
                {
                    var newLines = msg.Split(new string[] { "\r\n", "\r\n\r\n" }, StringSplitOptions.None);
                    foreach (var line in newLines)
                    {
                        var item = line.Trim();
                        if (item != "")
                        {
                            queue.Enqueue(item);
                        }                        
                        while (queue.Count > quesize)
                        {
                            queue.Dequeue();
                        }
                    }
                    
                    msg = string.Join("\n", queue);
                });
                return msg;
            }

        }
        return "";
    }
}