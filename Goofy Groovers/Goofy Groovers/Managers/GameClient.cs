using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PlatformGame.GameClient
{
    public class GreetingClient
    {
        public static void RunClient()
        {
            string serverName = "localhost"; //hostname
            int port = 6066; // port

            try
            {
                
                Console.WriteLine("Connecting to " + serverName + " on port: " + port);  // We try to connect to the server
                TcpClient client = new TcpClient(serverName, port);   //We create the socket

                Console.WriteLine("Successful connection to " + ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString());  //We display successful connection message along with the server's IP address it's connected to

                NetworkStream stream = client.GetStream();  //get the network stream to send and receive data

                // Create objects reading and writing to the network stream
                StreamReader reader = new StreamReader(stream);
                StreamWriter writer = new StreamWriter(stream);

                string message = "Hello from " + ((IPEndPoint)client.Client.LocalEndPoint).Address.ToString();
                writer.WriteLine(message);

                
                writer.Flush();   //We empty the buffer and make sure that all data is sent to the server

                string response = reader.ReadLine();

                Console.WriteLine("The server says " + response);                 //We display the server's response on the console

                client.Close();                 //We close the "socket"
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}