using Goofy_Groovers.Entity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Goofy_Groovers.Managers
{
    public class GameClient
    {
        private static string serverName = "localhost";
        private static int port = 6066;
        private static TcpClient client;
        private static NetworkStream stream;
        private static StreamReader reader;
        private static StreamWriter writer;

        static GameClient()
        {
        }

        public static void RunClient()
        {
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

                Console.WriteLine("The server says " + response);                 //We display the server's jsonResponse on the console

                client.Close();                 //We close the "socket"
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static bool TransmitToServer(BlobEntity player, List<BlobEntity> blobs)
        {
            try
            {
                client = new TcpClient(serverName, port);   //We create the socket
                stream = client.GetStream();  //get the network stream to send and receive data

                // Create objects reading and writing to the network stream
                reader = new StreamReader(stream);
                writer = new StreamWriter(stream);

                writer.WriteLine(JsonConvert.SerializeObject(player));

                writer.Flush();   //We empty the buffer and make sure that all data is sent to the server

                string jsonResponse = reader.ReadLine();
                Console.WriteLine("The server says " + jsonResponse); //Display the server's jsonResponse on the console
                if (jsonResponse != null)
                {
                    Response jsonData = JsonConvert.DeserializeObject<Response>(jsonResponse);

                    foreach (BlobEntity playerData in jsonData.playerList)
                    {
                        if (blobs.Any(player => player.blobUserId == playerData.blobUserId))
                        {
                            if (!blobs.First(player => player.blobUserId == playerData.blobUserId).isJumping)
                            {
                                player.position = playerData.position;
                                player.jumpStartPoint = playerData.jumpStartPoint;
                                player.jumpEndPoint = playerData.jumpEndPoint;
                                player.jumpTheta = playerData.jumpTheta;
                                player.isJumping = playerData.isJumping;
                            }
                        }
                        else
                        {
                            blobs.Add(new BlobEntity(
                                playerData.blobUserName, false, playerData.blobUserId, playerData.blobUserColor, playerData.position));
                        }
                    }
                    client.Close(); //We close the "socket"*
                    return true;
                }
                client.Close(); //We close the "socket"*
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        public static void _TransmitToServer(BlobEntity player)
        {
            Debug.WriteLine(JsonConvert.SerializeObject(player));

            /* try
             {
                 client = new TcpClient(serverName, port);   //We create the socket
                 stream = client.GetStream();  //get the network stream to send and receive data

                 // Create objects reading and writing to the network stream
                 reader = new StreamReader(stream);
                 writer = new StreamWriter(stream);

                 writer.WriteLine(jsonMessage);

                 writer.Flush();   //We empty the buffer and make sure that all data is sent to the server

                 string jsonResponse = reader.ReadLine();
                 Console.WriteLine("The server says " + jsonResponse); //We display the server's jsonResponse on the console
                 if (jsonResponse != null)
                 {
                     List<BlobEntity> blobEntities = new List<BlobEntity>();
                     var jsonResponse = JsonSerializer.Deserialize<Response>(jsonResponse);

                     foreach (Player playerData in jsonResponse.playerList)
                     {
                         blobEntities.Add(new BlobEntity(
                             playerData.name, playerData.id, playerData.color, playerData.position, playerData.isJumping,
                             playerData.jumpStart, playerData.jumpEnd));
                     }
                     client.Close(); //We close the "socket"
                 }
             }
             catch (Exception e)
             {
                 Console.WriteLine(e.ToString());
             }*/
        }
    }
}