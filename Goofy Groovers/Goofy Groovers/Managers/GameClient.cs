using Goofy_Groovers.Entity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Goofy_Groovers.Managers
{
    public class GameClient
    {
        private static string serverName = "localhost";
        private static BlobEntity bufferEntity;
        private static int port = 6066;
        private static TcpClient client;
        private static NetworkStream stream;
        private static StreamReader reader;
        private static StreamWriter writer;
        private static int iterator;

        static GameClient()
        {
        }

        public async void ConnectAndCommunicate()
        {
            while (true)
            {
                client = null;
                try
                {
                    client = await ConnectAsync();
                    //switch (Globals._gameState)
                    //{
                    //    case GoofyGroovers.GameState.RaceScreen:
                    //       {
                    Task.Run(() =>
                    {
                        MakeTransmission(client, Globals._gameManager.playerBlob);
                    });
                    //           break;
                    //       }
                    //   case GoofyGroovers.GameState.LobbyScreen:
                    //       {
                    //           break;
                    //       }
                    //   case GoofyGroovers.GameState.LeaderBoardScreen:
                    //       {
                    //           break;
                    //       }
                    //   default:
                    //       {
                    //          break;
                    //       }
                    
                    await ReceiveTransmission(Globals._gameManager.blobEntities);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("There was a problem connecting to server");
                }
                finally
                {
                    client?.Dispose();
                }
            }
        }

        public static async Task<TcpClient> ConnectAsync()
        {
            client = new TcpClient();
            await client.ConnectAsync(serverName, port);   //We create the socket
            return client;
        }

        public static void MakeTransmission(TcpClient connectedClient, BlobEntity playerBlob)
        {
            stream = connectedClient.GetStream();

            // Create objects reading and writing to the network stream
            writer = new StreamWriter(stream);

            writer.WriteLine(JsonConvert.SerializeObject(playerBlob));
            writer.FlushAsync();   //We empty the buffer and make sure that all data is sent to the server
        }

        public static async Task ReceiveTransmission(List<BlobEntity> blobs)
        {
            stream = client.GetStream();  //get the network stream to send and receive data
            reader = new StreamReader(stream);
            string jsonResponse = await reader.ReadLineAsync();
            Debug.WriteLine(jsonResponse);

            if (jsonResponse != null)
            {
                Response jsonData = JsonConvert.DeserializeObject<Response>(jsonResponse);
                if (jsonData != null)
                {
                    switch (jsonData.responseType)
                    {
                        case "AvaialbleLobbys":
                            {
                                break;
                            }
                        case "Update":
                            {
                                for (iterator = 0; iterator < jsonData.playerList.Count; iterator++)
                                {
                                    bufferEntity = new BlobEntity(
                                            jsonData.playerList[iterator].blobUserName, jsonData.playerList[iterator].blobUserId, jsonData.playerList[iterator].blobUserColor, jsonData.playerList[iterator].worldPosition, false);
                                    bufferEntity.SetTexture(Globals._dotTexture);
                                    bufferEntity.SetUserColor(jsonData.playerList[iterator].blobUserColor);
                                    bufferEntity.SetUserName(jsonData.playerList[iterator].blobUserName);

                                    BlobEntity localPlayer = blobs.Find(player => player.blobUserId == jsonData.playerList[iterator].blobUserId);
                                    lock (Globals._gameManager.toKeepEntitiesIntact)
                                    {
                                        if (localPlayer != null)
                                        {
                                            //if (!localPlayer.isJumping)
                                            //{
                                                blobs.Remove(localPlayer);
                                                blobs.Add(bufferEntity);
                                            //}
                                            //else
                                            //{
                                            //    Debug.WriteLine("Busy...");
                                            //}
                                        }
                                        else
                                        {
                                            Debug.WriteLine("New user!");
                                            blobs.Add(bufferEntity);
                                        }
                                    }
                                }
                                break;
                            }
                        case "Error":
                            {
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }
            }
        }
    }
}