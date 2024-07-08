using HSPGUI.Resources;
using Microsoft.Maui.Controls;
using System.Text.RegularExpressions;

/// <summary>
/// The HSPClient class is used for managing the connection and communication with a Telnet server.
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
    public bool _connected { get; }
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
    /// Connects to the HSP server.
    /// </summary>
    /// <param name="IpAddress">The IP address of the server.</param>
    /// <param name="Port">The port number of the server.</param>
    public async Task connectToHSP(string IpAddress, int Port)
    {
        await _client.ConnectAsync(IpAddress, Port);
    }

    /// <summary>
    /// Generates a buffer command data string.
    /// </summary>
    /// <param name="bufferCmdData">["EPC", "USR", "KIL", "ACC", "PCW"].</param>
    /// <param name="numberOfTags">The number of tags to be added to the buffer.</param>
    /// <param name="resetBuffer">Indicates whether to reset the buffer.</param>
    public async void GenerateBuffer(string?[] bufferCmdData, int numberOfTags, bool resetBuffer)
    {
        
        string command = "GENERATE=";
        int i = 0;
        foreach (var data in bufferCmdData)
        {
            if (data != "")
            {                
                command += dataTypes[i] + data;
            }
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
        if (_connected)
        {
            await _client.WriteAsync(command);
        }
    }
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
            CSVReader csvReader = new CSVReader(file);
            List<string[]>? data = csvReader.ReadCSV();
            progress.Report( 0);
            if (data == null)
            {
                throw new Exception();
            }
            int[] lengths = new int[] { -1, -1, 8, 8, 4 };
            // start i at 1 to ignore the header
            
            for (int i = 1; i < data.Count; i++)
            {                
                string command = "WRITEITEM=";
                for (int j = 0; j < data[i].Length; j++)
                { 
                    int length = lengths[j] < 0 ? data[i][j].Length : lengths[j];
                    if (data[i][j].Length > 0 && validateInput(data[i][j], length , false) == false)
                    {
                            System.Diagnostics.Debug.WriteLine($"Invalid entry {dataTypes[j]} on row {i}");
                        return i;
                    }
                    command += data[i][j].Length > 0 ? dataTypes[j] + data[i][j] : "";//only add if the section is not blank
                }                
                if (command != "WriteItem=") //make sure we aren't adding blank tags
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
                
                //System.Diagnostics.Debug.WriteLine($"row {i} total rows {data.Count} value {progressVal}");
                if(  i % (data.Count/Math.Min(200,data.Count)) == 0)
                {
                    double progressVal = (double)i / (double)(data.Count - 1);
                    progress.Report((double)i / (double)(data.Count - 1));
                    if (!_client.connected)
                    {
                        await Task.Delay(20);
                    }
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
    public Task<int[]> readAntenaStatus()
    {
        int[] settings = new int[6];             

        return Task.FromResult(settings);
    }
    public async Task writeAntenaSettigns( int [] Settings)
    {
        if (_client != null && _client.connected)
        {
            await _client.WriteAsync("NB");
            //await _client.ReadAsync();
        }
        return;
    }
    public bool validateInput(string input,int length,bool sequential)
    {
        string regExpresion = "^[A-Za-z0-9]*$";
        if (sequential)
        {
            regExpresion = "^[A-Za-z0-9!]*$";
        } 
        Regex regex = new Regex(regExpresion);
        if (length > 32 )//EPC and user data both take a maximum length of 32
        {            
            length = sequential? 32:33;//if sequential the lenght will be one longer
        }
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

