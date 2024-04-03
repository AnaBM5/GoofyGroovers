using Goofy_Groovers.Entity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        public static void TransmitToServer(BlobEntity playerBlob, List<BlobEntity> blobs)
        {
            try
            {
                client = new TcpClient(serverName, port);   //We create the socket
                stream = client.GetStream();  //get the network stream to send and receive data

                // Create objects reading and writing to the network stream
                reader = new StreamReader(stream);
                writer = new StreamWriter(stream);

                writer.WriteLine(JsonConvert.SerializeObject(playerBlob));

                writer.Flush();   //We empty the buffer and make sure that all data is sent to the server

                string jsonResponse = reader.ReadLine();

                if (jsonResponse != null)
                {
                    Response jsonData = JsonConvert.DeserializeObject<Response>(jsonResponse);
                    if (jsonData != null)
                    {
                        foreach (BlobEntity remotePlayerData in jsonData.playerList)
                        {
                            try
                            {
                                BlobEntity localPlayer = blobs.SingleOrDefault(player => player.blobUserId == remotePlayerData.blobUserId);
                                if (localPlayer == null)
                                {
                                    Debug.WriteLine("New user!");
                                    blobs.Add(new BlobEntity(
                                        remotePlayerData.blobUserName, remotePlayerData.blobUserId, remotePlayerData.blobUserColor, remotePlayerData.worldPosition, false));
                                }
                                else
                                {
                                    Debug.WriteLine("Update...");
                                    localPlayer.worldPosition = remotePlayerData.worldPosition;
                                    localPlayer.jumpStartPoint = remotePlayerData.jumpStartPoint;
                                    localPlayer.jumpEndPoint = remotePlayerData.jumpEndPoint;
                                    localPlayer.jumpTheta = remotePlayerData.jumpTheta;
                                    localPlayer.isJumping = remotePlayerData.isJumping;
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }
            }
            catch (SocketException)
            {
                // Case where the server is not running or cannot be reached
            }
            catch (Exception)
            {
                // Other types of exceptions
            }
            finally
            {
                // Close resources
                writer?.Close();
                reader?.Close();
                stream?.Close();
                client?.Close();
            }
        }
    }
}