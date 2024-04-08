/*using Goofy_Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
internal static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new ServerUI());
    }
}*/

using System.Net.Sockets;
using System.Net;
using System;
using System.Threading;
using System.Threading.Tasks;
using Goofy_Server;
using System.Diagnostics;

namespace Goofy_Server
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            Server server = new Server();
            Server.lobbyList.Add(new Lobby("Test!"));
            Server.listener.Start(); // start the listener
            Debug.WriteLine("uh...");

            // Create a Stopwatch instance
            Stopwatch stopwatch = new Stopwatch();
            while (true)
            {
                try
                {
                    
                    TcpClient client = new TcpClient();
                    stopwatch.Restart();
                    client = await Server.listener.AcceptTcpClientAsync(); // wait the user to connect
                    
                    stopwatch.Stop(); // Stop the stopwatch
                    Debug.WriteLine($"Connection time: {stopwatch.Elapsed.TotalMilliseconds} milliseconds"); // Display the elapsed time
                                                                                     // Console.WriteLine("Client connected form " + ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString());
                    _ = Task.Run(() => Server.HandleClient(client));
                }
                catch (Exception)
                {
                }
            }
        }
    }
}