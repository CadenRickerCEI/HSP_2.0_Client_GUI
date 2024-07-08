using System.Net.Sockets;
using System.Text;

public class TelnetClient
{
    private TcpClient? client;
    private NetworkStream? stream;
    private StreamReader? reader;
    private StreamWriter? writer;
    public bool connected;
    public TelnetClient()
    {
        connected = false;
        System.Diagnostics.Debug.WriteLine("TelnetClient created");
    }
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

    public async Task<string?> ReadAsync()
    {
        if (reader != null)
        {
            return await reader.ReadLineAsync();
        }else
        {
            return null;
        }
    }

    public async Task WriteAsync(string message)
    {
        if (writer != null)
        {
            await writer.WriteLineAsync(message);
        }
    }

    public void Disconnect()
    {
        if (client != null)
        {
            client.Close();
        }        
    }
}
