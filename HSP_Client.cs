using HSPGUI.Resources;
using Microsoft.Maui.Controls;
using MinimalisticTelnet;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Serilog;
using System.Net;
using System.IO.Pipes;
using HSPGUI;

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
    private bool _busy;
    public int _bufferCount =0; 
    /// <summary>
    /// Occurs when the connection status changes.
    /// </summary>
    public event Action<bool>? _connectionStatusChanged;
    /// <summary>
    /// Notifiation for when the data buffer been updated
    /// </summary>
    public event Action<bool>? _cmdUpdated;   
    /// <summary>
    /// Notifiation for when the data buffer been updated
    /// </summary>
    public event Action<bool>? _dataUpdated;
    /// <summary>
    /// Array of data types used in buffer commands.
    /// </summary>
    private string[] _dataTypes = new string[] { "EPC", "USR", "KIL", "ACC", "PCW" };
    /// <summary>
    /// Queue for storing the dialog strings to be space effiecent.
    /// </summary>
    private Queue<string> _cmdQueue = new Queue<string>();
    /// <summary>
    /// last 100 lines of cmd and respones from the HSP.
    /// </summary>
    public string _cmdbuffer = "";  
    /// <summary>
    /// Queue for storing lines of data from the data port
    /// </summary>
    private Queue<string> _data = new Queue<string>();
    /// <summary>
    /// last 100 lines of Data from the HSP.
    /// </summary>
    public string _dataBuffer = "";
    /// <summary>
    /// The single instance of the HSPClient class.
    /// </summary>
    private static HSPClient? _instance;
    /// <summary>
    /// Lock object for thread safety.
    /// </summary>
    private static readonly object _lock = new object();
    public bool _engaged { get; private set; }
    private int _portData = 5001;
    
    private string _IpAddress = "192.168.50.124";
    public string systemMode;
    /// <summary>
    /// array conating the list of all possible cmd for antena
    /// 0 NB: baud rate for antena
    /// 1 P"Tari""M Bit patern""LF Base Band Frequency"
    /// 2 AA:async reciever Gain
    /// 3 AG: reciever Gain
    /// 4 RF: Frequency for the antana 902mHz to 928mHz
    /// 5 RA: Power level for RF 8db to 19db
    /// </summary>
    private string[] antenaSettingCMDs = new string[] { "P","AA","AG","RF","RA" };
    public TagLog tagLog {  get; private set; }
    /// <summary>
    /// Initializes a new instance of the HSPClient class.
    /// </summary>
    private HSPClient()
    {
        _clientCMD = null;
        _clientDATA = null;
        _busy = false;
        var downloadsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "YourAppLogs");

        if (!Directory.Exists(downloadsDirectory))
        {
            Directory.CreateDirectory(downloadsDirectory);
        }
        systemMode = "STA";
        tagLog = new TagLog();
        var logFilePath = Path.Combine(downloadsDirectory, "app.log");
        Log.Logger = new LoggerConfiguration().WriteTo
            .File(logFilePath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7).CreateLogger(); 
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
    /// <param name="PortData"></param>
    /// <returns>String containg message from the HSP or Failed to conect</returns>
    public void connectToHSP(string IPAddress, int PortCMD, int PortData)
    {
        try
        {
            _portData = PortData;
            _IpAddress = IPAddress;
            _clientCMD = new TelnetConnection(IPAddress, PortCMD);
            _clientDATA = new TelnetConnection(IPAddress, PortData);        
            _connectionStatusChanged?.Invoke(isConnected());
            _busy = false;
            readSystemMode();
            return;
        }
        catch
        {
            return ;
        }
    }
    /// <summary>
    /// Generates a buffer command data string.
    /// </summary>
    /// <param name="bufferCmdData">Array of command data strings.</param>
    /// <param name="numberOfTags">The number of tags to be added to the buffer.</param>
    /// <param name="resetBuffer">Indicates whether to reset the buffer.</param>
    public string GenerateBuffer(string?[] bufferCmdData, int numberOfTags, bool resetBuffer)
    {
        string HSPResponse = "Generate File Failed";
        _busy = false;
        if (_clientCMD != null && _clientCMD.IsConnected)
        {
            HSPResponse = "";
            var command = "GENERATE=";
            var i = 0;
            foreach (var data in bufferCmdData)
            {
                if (data != "") command += _dataTypes[i] + data + ",";
                i++;
            }

            command += "NUM" + numberOfTags.ToString();

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
            }
        }
        _busy = true;
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
                            return $"File loading halted.\nError on Line {i}\n {_dataTypes[j]} {errMessage}\n";
                        }
                    }
                    command += data[i][j].Length > 0 ? _dataTypes[j] + data[i][j] + "," : "";
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
                            if (i % 25 == 0)
                            {
                                await Task.Delay(5);
                            }
                        }
                        catch(Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex);
                            _busy = false;
                            return connectionLost;
                        }
                    }
                }
                else
                {
                    _busy = false;
                    return $"Line {i} is Empty or invalid";
                }

                if (i % (data.Count / Math.Min(1000, data.Count)) == 0)
                {
                    var progressVal = (double)i / (double)(data.Count - 1);
                    progress.Report(progressVal);
                    if (!isConnected()) await Task.Delay(20);
                }
            }
            progress.Report(1.0);
            _busy = false;
            return "";
        }
        catch (Exception e)
        {
            System.Diagnostics.Debug.WriteLine(e.ToString());
            _busy = false;
            return "File is missing, open in another program, or empty.";
        }
    }

    /// <summary>
    /// Reads the the curretent antena settings from the HSP and returns them as an arrray.
    /// </summary>
    /// <returns>An array of integers representing the antenna settings.</returns>
    public async Task<string[]> readAntenaSettings()
    {
        _busy = true;
        var settings = new string[5] {"0", "0", "0", "0", "0"};
        if (_clientCMD != null)
        {
            for (int i = 0; i < antenaSettingCMDs.Length; i++)
            {
                string? msg = "";
                if (i!=1){
                    _clientCMD.WriteLine(antenaSettingCMDs[i]);
                    await Task.Delay(10);
                    msg  = _clientCMD.Read();
                    msg = msg == null ? "" : msg;
                    
                }
                string pattern = @"-?\d+(\.\d+)?";
                MatchCollection matches;
                switch (i)
                {
                    case 0: //P interface commands
                        matches = Regex.Matches(msg, pattern);
                        string output = "";
                        // Dictionaries to map match values to outputs
                        var tariMap = new Dictionary<string, string>
                        {
                            { "6.25", "0" },{ "12.5", "1" },{ "default", "2" }
                        };
                        var bitPatternMap = new Dictionary<string, string>
                        {
                            { "0", "0" },{ "2", "1" },{ "4", "2" },{ "default", "3" }
                        };
                        var baseBandMap = new Dictionary<string, string>
                        {
                            { "40", "0" },{ "160", "1" },{ "250", "2" },{ "320", "3" },{ "default", "4" }
                        };
                        // Process matches
                        output = matches.Count > 0 && tariMap.ContainsKey(matches[0].Value) ? tariMap[matches[0].Value] : tariMap["default"];
                        output += matches.Count > 1 && bitPatternMap.ContainsKey(matches[1].Value) ? bitPatternMap[matches[1].Value] : bitPatternMap["default"];
                        output += matches.Count > 2 && baseBandMap.ContainsKey(matches[2].Value) ? baseBandMap[matches[2].Value] : baseBandMap["default"];
                        settings[0] = output;
                        break;
                    case 1: //AA async reciever gain
                        //AA does not output anything when queried. It comes in with ag
                        break;
                    case 2: //AG reciever gain
                        matches = Regex.Matches(msg, pattern);
                        var gainMap = new Dictionary<string, string>
                        {
                            { "0","0" },{ "-9","1" },{ "-6","2" }, { "-3","3" },
                            { "3","4" },{ "6","5" },{ "default","6" }
                        };
                        if (matches.Count > 0)
                        {
                            settings[2] = gainMap.ContainsKey(matches[0].Value)? gainMap[matches[0].Value] : gainMap["default"];
                        }
                        if (matches.Count > 1)
                        {
                            settings[1] = gainMap.ContainsKey(matches[1].Value) ? gainMap[matches[1].Value] : gainMap["default"];
                        }                           
                        break;
                    case 3: //RF
                        matches = Regex.Matches(msg, pattern);
                        settings[3] = matches.Count > 0 ? (decimal.Parse(matches[0].Value)/1000).ToString("F2") : "0";
                        break;
                    case 4: //RA
                        matches = Regex.Matches(msg, pattern);                        
                        var RAMap = new Dictionary<string, string>
                        {
                            {"8","0"},{"9","1"},{"10","2"},{"11","3"},{"12","4"},{"13","5"},{"14","6"},{"15","7"},{"16","8"},{"17","9"},{"18","10"}
                        };
                        settings[4] = matches.Count > 2 && RAMap.ContainsKey(matches[1].Value)? RAMap[matches[1].Value] : "0";
                        break;
                    default:
                        break;
                }
            } 
        }
        _busy = false;
        return settings ;
    }

    /// <summary>
    /// Writes antenna settings to the HSP.
    /// </summary>
    /// <param name="settings">An array of integers representing the antenna settings.</param>
    public async Task writeAntenaSettings(string[] settings)
    {
        if (_clientCMD != null && isConnected())
        {
            for (int i = 0; i < antenaSettingCMDs.Length; i++)
            {
                string cmd = $"{antenaSettingCMDs[i]}";                
                switch (i)
                {
                    case 0: //P interface commands
                        var splitString = settings[i] != null ? settings[i].Split(",") : new string[] { "0","0","0"};
                        cmd += $"{(splitString.Length > 0 && int.Parse(splitString[0])>= 0 && int.Parse(splitString[0]) < 3 ? splitString[0] : "0")}" +
                               $"{(splitString.Length > 1 && int.Parse(splitString[1])>= 0 && int.Parse(splitString[1]) < 4 ? splitString[1] : "0")}" +
                               $"{(splitString.Length > 2 && int.Parse(splitString[2])>= 0 && int.Parse(splitString[2]) < 4 ? splitString[2] : "0")}";
                        break;
                    case 1: //AA async reciever gain                       
                        // same rules for AG
                    case 2: //AG reciever gain
                        cmd += $"{(settings[i] != null && int.Parse(settings[i]) >= 0 && int.Parse(settings[i]) <= 6 ? settings[i] : "0")}";                                               
                        break;
                    case 3: //RF
                        cmd += $"{(settings[i] != null && decimal.Parse(settings[i]) >= 902 && decimal.Parse(settings[i]) <= 928 ? (decimal.Parse(settings[i])*100).ToString("F0") : "91500")}";
                        break;
                    case 4: //RA
                        cmd += $"{(settings[i] != null && int.Parse(settings[i]) >= 0 && int.Parse(settings[i]) <= 10 ? (int.Parse(settings[i])+8).ToString() : "13")}";
                        break;
                    default:
                        break;
                }
                _clientCMD.WriteLine(cmd);
                await Task.Delay(10);
            }
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
        _connectionStatusChanged?.Invoke(connected);
        return connected;
    }

    /// <summary>
    /// gets the current buffer count in HSP.
    /// </summary>
    public void getBufferCount()
    {
        if (_clientCMD != null && isConnected())
            _clientCMD.WriteLine("GETBUFFERCOUNT\n");
    }

    /// <summary>
    /// When the ENGAGE command is issued the RF is turned 
    /// on, and the trigger line is processed according to
    /// the edge control currently set(rising edge or falling edge).
    /// </summary>
    /// <returns>Server response</returns>
    public void EngageHSP()
    {
        if (_clientCMD != null && isConnected())
            _clientCMD.WriteLine("ENGAGE");
            
    }

    /// <summary>
    /// From a reset the system is in DISENGAGE state. The ENGAGE 
    /// command must be issued before a programming run.When a 
    /// programming run is complete, the disengage command 
    /// should be issued to turn RF power off, and return the 
    /// system to idle state.
    /// </summary>
    /// <returns>Server response</returns>
    public void disengageHSP()
    {
        if (_clientCMD != null && isConnected())
        {
            _clientCMD.WriteLine("DISENGAGE");
        }
    }

    /// <summary>
    /// command will delete all records, and the buffer count
    /// will be set to zero.
    /// </summary>
    /// <returns>Server Message</returns>
    public void resetbuffer()
    {
        if (_clientCMD != null && isConnected())
        {
            _clientCMD.WriteLine("RESETBUFFER");
        }
    }

    /// <summary>
    /// Writes user command directly to the HSP.
    /// </summary>
    /// <param name="cmd">command</param>
    /// <returns>User command followed</returns>
    public void writeUsrCMD(string cmd)
    {
        if (_clientCMD != null && isConnected())
        {
            _cmdQueue.Enqueue($"USER_>{cmd}");
            _clientCMD.WriteLine($"{cmd}");
        }
    }
    /// <summary>
    /// Disconnects and disposes of all client connections.
    /// </summary>
    public void disconect()
    {
        if (_clientCMD is not null)
        {
            _clientCMD.Dispose();
        }
        if (_clientDATA is not null)
        {
            _clientDATA.Dispose();
        }
    }

    /// <summary>
    /// Reads data from dialog ports and updates the buffers accordingly.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task readDialogPorts()
    {
        
        if (_clientDATA != null)
        { 
            // Read data from the client and update the data buffer
            var dataMSG = _clientDATA.Read() ;
            if (dataMSG != null)
            {
                bool newTag = await tagLog.addData(dataMSG);
                _dataUpdated?.Invoke(newTag);
            }            
        }
        await parseCMDMsg();
                
    }
    /// <summary>
    /// parse message takes in a string splits by new lines then adds them to a queue.
    /// the oldest items are removed from the queue when queue is larger than size.
    /// it then returns the message containg the newest lines up to the quesize.
    /// </summary>
    /// <parm> if add to log is true add the output to a log and doesnt add to the queue</parm>
    /// <returns>the queue as joined as a string</returns>
    private async Task parseCMDMsg()
    {
        if (_clientCMD is not null && ! _busy)
        {
            int lastCount = _bufferCount;
            string? msg = null;
            var quesize = 15;
            try
            {
                // Attempt to read a message from the client
                msg = "HSPSA>"+ _clientCMD.Read();
            }
            catch
            {
                // Handle any exceptions that occur during the read operation
            }

            if (!string.IsNullOrEmpty(msg))
            {
                var parsedMsg = "";

                await Task.Run(() =>
                {
                    var newLines = msg.Split(new string[] { "\r\n", "\r\n\r\n" }, StringSplitOptions.None);
                    parsedMsg = "";
                    if (newLines.Length > 0)
                    {
                        if (newLines.Length > quesize)
                        {
                            _cmdQueue.Clear();
                        }
                        foreach (var line in newLines.TakeLast(Math.Min(quesize, newLines.Length)))
                        {
                            var item = line.Trim();
                            if (item != "" && !item.Contains("PIPUMP") && !( item.Length -1 > -1 && item[item.Length-1] == '>'))
                            {
                                _cmdQueue.Enqueue(item);
                                if (item.Contains("ENGAGED"))
                                {
                                    _engaged = true;
                                }else if (item.Contains("DISENGAGED"))
                                {
                                    _engaged = false;
                                }
                            }
                            else if (item.Contains("PIPUMP"))
                            {
                                MatchCollection matches = Regex.Matches(line, @"\d+");
                                int sum = 0;
                                foreach (Match match in matches)
                                    sum += int.Parse(match.Value);
                                _bufferCount = sum;
                            }
                            while (_cmdQueue.Count > quesize)
                            {
                                _cmdQueue.Dequeue();
                            }
                        }
                        parsedMsg = string.Join("\n", _cmdQueue);
                    }
                });
                if ((parsedMsg != "" && parsedMsg != _cmdbuffer)|| _bufferCount != lastCount)
                {
                    _cmdbuffer = parsedMsg;
                    _cmdUpdated?.Invoke(true);
                }
                else
                {
                    _cmdUpdated?.Invoke(false);
                }
            }
        }
    }
    public void readSystemMode()
    {
        _busy = true;
        if (_clientCMD != null && _clientCMD.IsConnected)
        {
            _clientCMD.WriteLine("SYSTEMTYPE");
            string? line = _clientCMD.Read();
            line = line == null ? "" : line;
            int index = line.IndexOf("=");
            if (index != -1 && index < line.Length )
            {
                var systemTypes = new string[] { "VER", "STA", "ENC", "TRE" };
                foreach ( string  systemType in systemTypes)
                {
                    if (line.Contains(systemType))
                    {
                        systemMode = systemType;
                    }
                }
            }
        }
        _busy = false;
        return ;
    }
    public async Task writeSystemMode(string systemType)
    {
        var systemTypes = new string[] { "VER","STA", "ENC", "TRE" };
        if (systemTypes.Contains(systemType) && _clientCMD != null && _clientCMD.IsConnected )
        {
            _clientCMD.WriteLine("DISENGAGE");
            await Task.Delay(10);
            systemMode = systemType;
            _clientCMD.WriteLine("SYSTEMTYPE="+systemType);
            await Task.Delay(10);
            _clientCMD.WriteLine("ENGAGE");
        }
        return;
    }
}