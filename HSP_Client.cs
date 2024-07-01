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
    string[] dataTypes = ["EPC", "USR", "KIL", "ACC", "PCW"];
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
    public async Task LoadFromFile(string file, IProgress<double> progress)
    {
        try
        {
            CSVReader csvReader = new CSVReader(file);
            List<string[]> data = csvReader.ReadCSV();
            progress.Report( 0);
            // start i at 1 to ignore the header
            for (int i = 1; i < data.Count; i++)
            {
                
                string command = "WRITEITEM=";
                //EPC
                if (data[i].Length >0 & data[i][0].Length>0 & validateInput(data[i][0], data[i][0].Length ,false) )
                {
                    command += dataTypes[0] + data[i][0];
                }
                //user data
                if (data[i].Length > 1 & data[i][1].Length > 0 & validateInput(data[i][1], data[i][1].Length, false))
                {
                    command += dataTypes[1] + data[i][1];
                }
                //kill password
                if (data[i].Length > 2 & data[i][2].Length > 0 & validateInput(data[i][2], 8, false))
                {
                    command += dataTypes[2] + data[i][2];
                }
                //Access password
                if (data[i].Length > 3 & data[i][3].Length > 0 & validateInput(data[i][3], 8, false))
                {
                    command += dataTypes[3] + data[i][3];
                }
                //PC Word
                if (data[i].Length > 4 & data[i][4].Length > 0 & validateInput(data[i][0], 4, false))
                {
                    command += dataTypes[4] + data[i][4];
                }
                if (command != "WriteItem=") //make sure we aren't creating an invalid tag
                {
                    //System.Diagnostics.Debug.WriteLine(command);
                    if (_client.connected)
                    {
                        await _client.WriteAsync(command);
                    }
                }
                progress.Report((double)i/(double)data.Count*100.0);
            }
            System.Diagnostics.Debug.WriteLine("completed loading");
        }
        catch
        {
            return;
        }
        
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
        
        return regex.IsMatch(input) & input.Length == length;
    }


}

