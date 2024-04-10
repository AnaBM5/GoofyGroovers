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
            Server.lobbyList.Add(new Lobby("Default"));
            Server.listener.Start(); // start the listener
            server.GetLocalIP();

            // Create a Stopwatch instance
            while (true)
            {
                try
                {
                    TcpClient client = await Server.listener.AcceptTcpClientAsync(); // wait the user to connect
                    
                    _ = Task.Run(() => Server.HandleClient(client));
                }
                catch (Exception)
                {
                }
            }
        }
    }
}