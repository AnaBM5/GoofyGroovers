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

        private float timer = 0;

        static GameClient()
        {
        }

        public async void ConnectAndCommunicate(GameState gameState)
        {
            Debug.WriteLine("update");
            try
            {
                client = AsyncConnect(client, serverName, port);
                stream = client.GetStream();
                switch (gameState)
                {
                    case GameState.RaceScreen:
                        {
                            if (Globals._gameManager.playerBlob.isStartingTheRace && !Globals._gameManager.raceHasStarted)
                            {
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
                Debug.WriteLine("There was a problem connecting to server");
                Debug.WriteLine(ex.Source.ToString());
                Debug.WriteLine(ex.Message.ToString());
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
                                        }
                                        else
                                        {
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
                                    BlobEntity localPlayer = blobs.Find(player => player.blobUserId == jsonData.playerList[iterator].blobUserId);
                                    lock (Globals._gameManager.toKeepEntitiesIntact)
                                    {
                                        if (localPlayer != null)
                                        {
                                            localPlayer.finishTime = jsonData.playerList.ElementAt(iterator).finishTime;
                                            Debug.WriteLine(localPlayer.finishTime);
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