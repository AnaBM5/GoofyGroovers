using Goofy_Groovers.Entity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using static Goofy_Groovers.GoofyGroovers;

namespace Goofy_Groovers.Managers
{
    public class GameClient
    {
        private string serverName = "127.0.0.1";
        private static BlobEntity bufferEntity;
        private static int port = 6066;
        private static TcpClient client;
        private static NetworkStream stream;
        private static StreamReader reader;
        private static StreamWriter writer;
        private static int iterator;
        private static int iteratorSecond;
        private static bool objectFound;

        private float timer = 0;

        static GameClient()
        {
        }

        public async void ConnectAndCommunicate(GameState gameState)
        {
            try
            {
                client = AsyncConnect(client, serverName, port);
                stream = client.GetStream();
                switch (gameState)
                {
                    case GameState.RaceScreen:
                        {
                            if (Globals._gameManager.playerBlob.isStartingTheRace && !Globals._gameManager.raceStarted)
                            {
                                Debug.WriteLine("RaceStarted" + Globals._gameManager.raceStarted);
                                MakeStartingTransmission();
                            }
                            else
                            {
                                MakeRaceTransmission(Globals._gameManager.playerBlob);
                            }
                            await ReceiveTransmission(Globals._gameManager.blobEntities);
                            break;
                        }
                    case GameState.LobbyScreen:
                        {
                            MakeLobbyTransmission(Globals._gameManager.playerBlob);
                            await ReceiveTransmission(Globals._gameManager.blobEntities);
                            break;
                        }
                    case GameState.LeaderBoardScreen:
                        {
                            MakeFinishLineTransmission(Globals._gameManager.playerBlob);
                            await ReceiveTransmission(Globals._gameManager.blobEntities);
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
                Debug.WriteLine(ex.Data.ToString());
            }
            finally
            {
                client?.Close();
            }
        }

        public static TcpClient AsyncConnect(TcpClient client, string serverName, int port)
        {
            client = new TcpClient(serverName, port);   //We create the socket
            return client;
        }

        private void MakeLobbyTransmission(BlobEntity playerBlob)
        {
            // Create objects reading and writing to the network stream
            writer = new StreamWriter(stream);
            var jsonMessage = new
            {
                messageType = "LobbyWait",
                player = playerBlob,
            };
            writer.WriteLine(JsonConvert.SerializeObject(jsonMessage));
            writer.FlushAsync();   //We empty the buffer and make sure that all data is sent to the server
        }

        private void MakeStartingTransmission()
        {
            // Create objects reading and writing to the network stream
            writer = new StreamWriter(stream);
            var jsonMessage = new
            {
                messageType = "RaceStart",
                startTime = Globals._gameManager.raceStartTime,
            };
            writer.WriteLine(JsonConvert.SerializeObject(jsonMessage));
            writer.FlushAsync();   //We empty the buffer and make sure that all data is sent to the server
        }

        public static void MakeRaceTransmission(BlobEntity playerBlob)
        {
            // Create objects reading and writing to the network stream
            writer = new StreamWriter(stream);

            var jsonMessage = new
            {
                messageType = "RaceUpdate",
                player = playerBlob,
            };
            writer.WriteLine(JsonConvert.SerializeObject(jsonMessage));
            writer.FlushAsync();   //We empty the buffer and make sure that all data is sent to the server
        }

        public static void MakeFinishLineTransmission(BlobEntity playerBlob)
        {
            // Create objects reading and writing to the network stream
            writer = new StreamWriter(stream);

            var jsonMessage = new
            {
                messageType = "FinishLineUpdate",
                player = playerBlob,
            };
            writer.WriteLine(JsonConvert.SerializeObject(jsonMessage));
            writer.FlushAsync();   //We empty the buffer and make sure that all data is sent to the server
        }

        public static async Task ReceiveTransmission(List<BlobEntity> blobs)
        {
            reader = new StreamReader(stream);
            string jsonResponse = await reader.ReadLineAsync();
            Debug.WriteLine(jsonResponse + "\n");

            if (jsonResponse != null)
            {
                Response jsonData = JsonConvert.DeserializeObject<Response>(jsonResponse);

                if (jsonData != null)
                {
                    switch (jsonData.messageType)
                    {
                        case "SetRaceStarter":
                            {
                                try
                                {
                                    Globals._gameManager.raceStarter = jsonData.raceStarter;

                                    if (Globals._gameManager.playerBlob.blobUserId == jsonData.raceStarterId)
                                    {
                                        Globals._gameManager.playerBlob.isStartingTheRace = true;
                                    }

                                    for (iterator = 0; iterator < jsonData.playerList.Count; iterator++)
                                    {
                                        lock (Globals._gameManager.toKeepEntitiesIntact)
                                        {
                                            BlobEntity localPlayer = blobs.FirstOrDefault(player => player.blobUserId == jsonData.playerList[iterator].blobUserId);
                                            if (localPlayer == null)
                                            {
                                                bufferEntity = new BlobEntity();

                                                bufferEntity.SetUserId(jsonData.playerList[iterator].blobUserId);
                                                bufferEntity.SetUserName(jsonData.playerList[iterator].blobUserName);
                                                bufferEntity.SetTexture(Globals._dotTexture);
                                                bufferEntity.SetUserColor(jsonData.playerList[iterator].blobUserColor);
                                                bufferEntity.SetSecondsSinceJumpStarted(jsonData.playerList[iterator].elapsedSecondsSinceJumpStart);

                                                Debug.WriteLine("Added new user");

                                                blobs.Add(bufferEntity);
                                            }

                                            if (localPlayer != null)
                                            {
                                                Debug.WriteLine("localPlayer: " + localPlayer.blobUserId + "/" + localPlayer.blobUserName);
                                            }
                                            else
                                            {
                                                Debug.WriteLine("Nothing");
                                            }
                                        }
                                    }
                                }
                                catch (Exception)
                                { }
                            }
                            break;

                        case "RaceStart":
                            {
                                try
                                {
                                    Globals._gameManager.raceStartTime = jsonData.startTime;
                                    gameState = GameState.RaceScreen;
                                }
                                catch (Exception)
                                { }
                            }
                            break;

                        case "RaceUpdate":
                            {
                                for (iterator = 0; iterator < jsonData.playerList.Count; iterator++)
                                {
                                    lock (Globals._gameManager.toKeepEntitiesIntact)
                                    {
                                        objectFound = false;
                                        for (iteratorSecond = 0; iteratorSecond < blobs.Count; iteratorSecond++)
                                        {
                                            // ref BlobEntity localPlayer = ref blobs.FirstOrDefault(player => player.blobUserId == jsonData.playerList[iterator].blobUserId);
                                            if (blobs.ElementAt(iteratorSecond).blobUserId == jsonData.playerList[iterator].blobUserId)
                                            {
                                                objectFound = true;

                                                if (!blobs.ElementAt(iteratorSecond).isJumping)
                                                {
                                                    if (jsonData.playerList.ElementAt(iterator).isJumping)
                                                    {
                                                        blobs.ElementAt(iteratorSecond).SetJumpStartPoint(jsonData.playerList[iterator].jumpStartPoint);
                                                        blobs.ElementAt(iteratorSecond).SetJumpEndPoint(jsonData.playerList[iterator].jumpEndPoint);
                                                        blobs.ElementAt(iteratorSecond).SetThetha(jsonData.playerList[iterator].jumpTheta);
                                                        blobs.ElementAt(iteratorSecond).SetVelocity(jsonData.playerList[iterator].velocity);
                                                        blobs.ElementAt(iteratorSecond).SetSecondsSinceJumpStarted(0);

                                                        blobs.ElementAt(iteratorSecond).SetJumpingState(jsonData.playerList[iterator].isJumping);
                                                        Debug.WriteLine(blobs.ElementAt(iterator).ToString());
                                                    }
                                                }
                                                iteratorSecond = blobs.Count;
                                                break;
                                            }
                                        }

                                        if (!objectFound)
                                        {
                                            bufferEntity = new BlobEntity();
                                            bufferEntity.SetUserId(jsonData.playerList[iterator].blobUserId);
                                            bufferEntity.SetUserName(jsonData.playerList[iterator].blobUserName);
                                            bufferEntity.SetTexture(Globals._dotTexture);
                                            bufferEntity.SetUserColor(jsonData.playerList[iterator].blobUserColor);
                                            bufferEntity.SetSecondsSinceJumpStarted(jsonData.playerList[iterator].elapsedSecondsSinceJumpStart);

                                            bufferEntity.SetJumpStartPoint(jsonData.playerList[iterator].jumpStartPoint);
                                            bufferEntity.SetJumpEndPoint(jsonData.playerList[iterator].jumpEndPoint);
                                            bufferEntity.SetJumpingState(jsonData.playerList[iterator].isJumping);
                                            bufferEntity.SetThetha(jsonData.playerList[iterator].jumpTheta);
                                            bufferEntity.SetVelocity(jsonData.playerList[iterator].velocity);
                                            blobs.Add(bufferEntity);
                                        }
                                    }
                                }
                            }
                            break;

                        case "FinishLineUpdate":
                            {
                                for (iterator = 0; iterator < jsonData.playerList.Count; iterator++)
                                {
                                    lock (Globals._gameManager.toKeepEntitiesIntact)
                                    {
                                        objectFound = false;
                                        for (iteratorSecond = 0; iteratorSecond < blobs.Count; iteratorSecond++)
                                        {
                                            // ref BlobEntity localPlayer = ref blobs.FirstOrDefault(player => player.blobUserId == jsonData.playerList[iterator].blobUserId);
                                            if (blobs.ElementAt(iteratorSecond).blobUserId == jsonData.playerList[iterator].blobUserId)
                                            {
                                                Debug.WriteLine(blobs[iteratorSecond].blobUserName + "Has been found");
                                                Debug.WriteLine(blobs.ElementAt(iteratorSecond).finishTime + " / " + jsonData.playerList.ElementAt(iterator).finishTime + " / " + jsonData.playerList[iterator].finishTime);
                                                objectFound = true;
                                                if (jsonData.playerList.ElementAt(iterator).finishTime != -1)
                                                {
                                                    blobs.ElementAt(iteratorSecond).finishTime = jsonData.playerList.ElementAt(iterator).finishTime;
                                                }

                                                iterator = blobs.Count;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            break;

                        case "OK":
                            break;

                        case "Error":
                            {
                                Debug.WriteLine("Server encountered an error:");
                                Debug.WriteLine(jsonData.message);
                            }
                            break;

                        default:
                            {
                                break;
                            }
                    }
                }
            }
        }

        public void SetServerName(string serverName)
        {
            this.serverName = serverName;
        }
    }
}