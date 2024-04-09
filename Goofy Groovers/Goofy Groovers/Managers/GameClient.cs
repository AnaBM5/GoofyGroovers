﻿using Goofy_Groovers.Entity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using static Goofy_Groovers.GoofyGroovers;

namespace Goofy_Groovers.Managers
{
    public class GameClient
    {
        private static string serverName = "127.0.0.1";
        private static BlobEntity bufferEntity;
        private static int port = 6066;
        private static TcpClient client;
        private static NetworkStream stream;
        private static StreamReader reader;
        private static StreamWriter writer;
        private static int iterator;

        private float timer = 0;

        static GameClient()
        {
        }

        public async void ConnectAndCommunicate(GameState gameState)
        {
            // Create a Stopwatch instance
            Stopwatch stopwatch = new Stopwatch();

            try
            {
                stopwatch.Restart();                // Start the stopwatch
                client = AsyncConnect(client, serverName, port);
                stopwatch.Stop();                   // Stop the stopwatch
                Debug.WriteLine($"Connection: {stopwatch.Elapsed.TotalMilliseconds} milliseconds"); // Display the elapsed time

                stopwatch.Restart();                // Start the stopwatch
                stream = client.GetStream();
                stopwatch.Stop();                   // Stop the stopwatch
                Debug.WriteLine($"Stream access: {stopwatch.Elapsed.TotalMilliseconds} milliseconds"); // Display the elapsed time
                switch (gameState)
                {
                    case GameState.RaceScreen:
                        {
                            stopwatch.Restart();    // Start the stopwatch
                            MakeTransmission(client, Globals._gameManager.playerBlob);
                            stopwatch.Stop();       // Stop the stopwatch
                            Debug.WriteLine($"Transmission: {stopwatch.Elapsed.TotalMilliseconds} milliseconds"); // Display the elapsed time

                            stopwatch.Restart();    // Start the stopwatch
                            await ReceiveTransmission(Globals._gameManager.blobEntities);
                            stopwatch.Stop();       // Stop the stopwatch
                            Debug.WriteLine($"Feedback: {stopwatch.Elapsed.TotalMilliseconds} milliseconds\n\n"); // Display the elapsed time

                            break;
                        }
                    case GameState.LobbyScreen:
                        {
                            break;
                        }
                    case GameState.LeaderBoardScreen:
                        {
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("There was a problem connecting to server");
            }
            finally
            {
                client?.Close();
            }
        }

        public async void ConnectAndCommunicateContiniously(GoofyGroovers game)
        {
            // Create a Stopwatch instance
            Stopwatch stopwatch = new Stopwatch();

            while (true)
            {
                try
                {
                    client = new TcpClient();
                    stopwatch.Restart();
                    await client.ConnectAsync(serverName, port);
                    stopwatch.Stop(); // Stop the stopwatch
                    Debug.WriteLine($"Connection time: {stopwatch.Elapsed.TotalMilliseconds} milliseconds"); // Display the elapsed time

                    stopwatch.Restart();
                    stream = client.GetStream();
                    stopwatch.Stop(); // Stop the stopwatch
                    Debug.WriteLine($"Stream time: {stopwatch.Elapsed.TotalMilliseconds} milliseconds"); // Display the elapsed time

                    stopwatch.Restart();
                    MakeTransmission(client, Globals._gameManager.playerBlob);
                    stopwatch.Stop(); // Stop the stopwatch
                    Debug.WriteLine($"Transmission time: {stopwatch.Elapsed.TotalMilliseconds} milliseconds"); // Display the elapsed time

                    stopwatch.Restart();
                    await ReceiveTransmission(Globals._gameManager.blobEntities);
                    stopwatch.Stop(); // Stop the stopwatch
                    Debug.WriteLine($"Feedback time: {stopwatch.Elapsed.TotalMilliseconds} milliseconds\n\n"); // Display the elapsed time

                }
                catch (Exception ex)
                {
                    Debug.WriteLine("There was a problem connecting to server");
                }
                finally
                {
                    //    client?.Dispose();
                }
            }
        }

        public static TcpClient AsyncConnect(TcpClient client, string serverName, int port)
        {
            client = new TcpClient(serverName, port);   //We create the socket
            return client;
        }

        public static void MakeTransmission(TcpClient connectedClient, BlobEntity playerBlob)
        {
            // Create objects reading and writing to the network stream
            writer = new StreamWriter(stream);

            writer.WriteLine(JsonConvert.SerializeObject(playerBlob));
            writer.FlushAsync();   //We empty the buffer and make sure that all data is sent to the server
        }

        public static async Task ReceiveTransmission(List<BlobEntity> blobs)
        {
            reader = new StreamReader(stream);
            string jsonResponse = await reader.ReadLineAsync();

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
                                            if (!localPlayer.isJumping)
                                            {
                                                if (jsonData.playerList[iterator].isJumping)
                                                {
                                                    bufferEntity.SetSecondsSinceJumpStarted(0);
                                                }
                                                blobs.Remove(localPlayer);
                                                blobs.Add(bufferEntity);
                                            }
                                            else
                                            {
                                                Debug.WriteLine("Busy...");
                                            }
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