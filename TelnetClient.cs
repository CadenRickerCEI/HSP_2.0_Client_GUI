using System.Net.Sockets;
using System.Text;

/// <summary>
/// The TelnetClient class manages the connection and communication with a Telnet server.
/// </summary>
public class TelnetClient
{
    /// <summary>
    /// The TCP client used for the connection.
    /// </summary>
    private TcpClient? client;

    /// <summary>
    /// The network stream used for reading and writing data.
    /// </summary>
    private NetworkStream? stream;

    /// <summary>
    /// The stream reader used for reading data from the server.
    /// </summary>
    private StreamReader? reader;

    /// <summary>
    /// The stream writer used for writing data to the server.
    /// </summary>
    private StreamWriter? writer;
    /// <summary>
    /// The single instance of the TelnetClient class.
    /// </summary>
    private static TelnetClient? _instance;

    /// <summary>
    /// Lock object for thread safety.
    /// </summary>
    private static readonly object _lock = new object();
    /// <summary>
    /// Initializes a new instance of the <see cref="TelnetClient"/> class.
    /// </summary>
    private TelnetClient() => System.Diagnostics.Debug.WriteLine("TelnetClient created");
    /// <summary>
    /// Gets the single instance of the TelnetClient class.
    /// </summary>
    public static TelnetClient Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new TelnetClient();
                    }
                }
            }
            return _instance;
        }
    }
    /// <summary>
    /// Asynchronously connects to the Telnet server.
    /// </summary>
    /// <param name="hostname">The hostname of the server.</param>
    /// <param name="port">The port number of the server.</param>
    public async Task ConnectAsync(string hostname, int port)
    {
        client = new TcpClient();

        try
        {
            await client.ConnectAsync(hostname, port);

            if (client.Connected)
            {
                stream = client.GetStream();
                reader = new StreamReader(stream, Encoding.UTF8);
                writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
            }
        }
        catch
        {
            ;
        }
    }

    /// <summary>
    /// Asynchronously reads a line of data from the server.
    /// </summary>
    /// <returns>A task that represents the asynchronous read operation. The value of the TResult parameter contains the read line as a string, or null if the reader is not initialized.</returns>
    public async Task<string?> ReadAsync()
    {
        if (reader != null)
        {
            return await reader.ReadLineAsync();
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Asynchronously reads a line of data from the server.
    /// </summary>
    /// <returns>A task that represents the asynchronous read operation. The value of the TResult parameter contains the read line as a string, or null if the reader is not initialized.</returns>
    public async Task<string?> ReadAllAsync()
    {
        if (reader != null)
        {
            var result = "";
            string? responce = await reader.ReadLineAsync();

            while (!string.IsNullOrEmpty(responce))
            {
                await Task.Delay(1000);
                result += responce;
                responce = await reader.ReadLineAsync();
            }

            return result;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Asynchronously writes a message to the server.
    /// </summary>
    /// <param name="message">The message to write.</param>
    public async Task WriteAsync(string message)
    {
        if (writer != null)
        {
            await writer.WriteLineAsync(message);
        }
    }

    /// <summary>
    /// Disconnects from the Telnet server.
    /// </summary>
    public void Disconnect()
    {
        if (client != null) client.Close();
    }

    /// <summary>
    /// Checks that the client is connected to HSP server.
    /// </summary>
    public bool IsConnected() => client != null && client.Connected;
}