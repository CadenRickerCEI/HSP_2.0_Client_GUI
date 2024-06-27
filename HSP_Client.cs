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
        string[] dataTypes = ["EPC", "USR", "KIL", "ACC", "PCW"];
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


}

