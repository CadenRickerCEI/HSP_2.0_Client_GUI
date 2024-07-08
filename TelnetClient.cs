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
    /// Indicates whether the client is connected to the server.
    /// </summary>
    public bool connected;

    /// <summary>
    /// Initializes a new instance of the <see cref="TelnetClient"/> class.
    /// </summary>
    public TelnetClient()
    {
        connected = false;
        System.Diagnostics.Debug.WriteLine("TelnetClient created");
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
                connected = true;
                stream = client.GetStream();
                reader = new StreamReader(stream, Encoding.UTF8);
                writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
            }
        }
        catch
        {
            connected = false;
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
}