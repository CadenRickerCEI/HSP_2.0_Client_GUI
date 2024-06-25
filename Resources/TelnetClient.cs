using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class TelnetClient
{
    private TcpClient client;
    private StreamReader reader;
    private StreamWriter writer;
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
                var stream = client.GetStream();
                reader = new StreamReader(stream, Encoding.UTF8);
                writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
            }
        }
        catch(Exception ex) {
        
            connected = false;
        }      
    }

    public async Task<string> ReadAsync()
    {
        return await reader.ReadLineAsync();
    }

    public async Task WriteAsync(string message)
    {
        await writer.WriteLineAsync(message);
    }

    public void Disconnect()
    {
        client.Close();
    }
}
