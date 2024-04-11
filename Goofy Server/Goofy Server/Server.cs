using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

public class Response
{
    public string messageType;
    public List<BlobEntity> playerList;
    public DateTime startTime;
    public DateTime endTime;
    public int raceStarterId;
    public string raceStarter;
    public BlobEntity player;
    public string message;

    public Response()
    { }

    public Response(string responseType)
    {
        this.messageType = responseType;
    }

    public Response(string responseType, List<BlobEntity> playerList) : this(responseType)
    {
        this.playerList = playerList;
    }
}

public class Server
{
    public GraphicsDeviceManager _graphics;
    public SpriteBatch _spriteBatch;
    public static TcpListener listener = new TcpListener(IPAddress.Any, 6066);
    public static List<Lobby> lobbyList = new();
    public string raceStarter = "";
    private int playersFinished = 0;
    public static readonly Object toKeepEntitiesIntact = new Object();

    public Server()
    {
    }

    public string GetLocalIP()
    {
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
        socket.Connect("8.8.8.8", 65530);
        IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
        try
        {
            Debug.WriteLine(endPoint.Address.ToString());
            return endPoint.Address.ToString();
        }
        catch
        {
            return "127.0.0.1";
        }
    }

    public static void HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        StreamReader reader = new(stream);
        StreamWriter writer = new(stream);

        try
        {
            string message = reader.ReadLine(); //read the message from the client
            writer.WriteLine(HandleMessage(message));
            writer.Flush();
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.ToString());
        }
        finally
        {
            client.Close(); // We close the connection
        }
    }

    private static string HandleMessage(string jsonMessage)
    {
        Response jsonInput = JsonConvert.DeserializeObject<Response>(jsonMessage);
        Response jsonResponse = new Response();

        Debug.WriteLine(jsonMessage + "\n");

        if (jsonMessage != null)
        {
            switch (jsonInput.messageType)
            {
                case "LobbyWait":
                    {
                        if (lobbyList.ElementAt(0).raceEndTime != DateTime.MinValue) 
                        {
                            if (lobbyList.ElementAt(0).raceEndTime.AddSeconds(1) > DateTime.Now)
                            {
                                lobbyList.ElementAt(0).raceStartTime = DateTime.MinValue;
                                lobbyList.ElementAt(0).playerList.Clear();
                            }
                            else
                            {
                                lobbyList.ElementAt(0).raceEndTime = DateTime.MinValue;
                            }
                        }

                        if (lobbyList.ElementAt(0).controllingPlayerId == -1)
                            {
                                lobbyList.ElementAt(0).controllingPlayerId = jsonInput.player.blobUserId;
                                lobbyList.ElementAt(0).controllingPlayerName = jsonInput.player.blobUserName;
                            }

                        if (!lobbyList.ElementAt(0).playerList.Exists(player => player.blobUserId == jsonInput.player.blobUserId))
                        {
                            lobbyList.ElementAt(0).playerList.Add(jsonInput.player);
                        }

                        if (lobbyList.ElementAt(0).raceStartTime != DateTime.MinValue)
                        {
                            var responseMessage = new
                            {
                                messageType = "RaceStart",
                                startTime = lobbyList.ElementAt(0).raceStartTime,
                            };
                            return JsonConvert.SerializeObject(responseMessage);
                        }
                        else
                        {
                            jsonResponse = new Response("SetRaceStarter", new List<BlobEntity>())
                            {
                                messageType = "SetRaceStarter",
                                raceStarterId = lobbyList.ElementAt(0).controllingPlayerId,
                                raceStarter = lobbyList.ElementAt(0).controllingPlayerName,
                            };

                            foreach (BlobEntity existingPlayerData in lobbyList.ElementAt(0).playerList)
                            {
                                if (existingPlayerData.blobUserId != jsonInput.player.blobUserId && !existingPlayerData.disconnected )
                                {
                                    jsonResponse.playerList.Add(existingPlayerData);
                                }
                            }

                            return JsonConvert.SerializeObject(jsonResponse);
                        }
                        break;
                    }
                case "RaceStart":
                    {
                        lobbyList.ElementAt(0).raceStartTime = jsonInput.startTime;
                        Debug.WriteLine(jsonInput.startTime);
                        var responseMessage = new
                        {
                            messageType = "OK",
                        };
                    }
                    break;

                case "RaceUpdate":
                    {
                        jsonResponse = new Response("RaceUpdate", new List<BlobEntity>());

                        if (jsonInput.player != null)
                        {
                            if (lobbyList.ElementAt(0).playerList.Any(it => it.blobUserId == jsonInput.player.blobUserId))
                            {
                                lock (toKeepEntitiesIntact)
                                {
                                    foreach (BlobEntity existingPlayerData in lobbyList.ElementAt(0).playerList)
                                    {
                                        if (existingPlayerData.blobUserId == jsonInput.player.blobUserId)
                                        {
                                            existingPlayerData.blobUserName = jsonInput.player.blobUserName;
                                            existingPlayerData.blobUserColor = jsonInput.player.blobUserColor;
                                            existingPlayerData.worldPosition = jsonInput.player.worldPosition;
                                            existingPlayerData.isJumping = jsonInput.player.isJumping;
                                            existingPlayerData.jumpStartPoint = jsonInput.player.jumpStartPoint;
                                            existingPlayerData.jumpEndPoint = jsonInput.player.jumpEndPoint;
                                            existingPlayerData.velocity = jsonInput.player.velocity;
                                            existingPlayerData.jumpTheta = jsonInput.player.jumpTheta;
                                        }
                                        else
                                        {
                                            jsonResponse.playerList.Add(existingPlayerData);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                lock (toKeepEntitiesIntact)
                                    lobbyList.ElementAt(0).playerList.Add(jsonInput.player);
                            }
                        }
                    }
                    break;

                case "FinishLineUpdate":
                    {
                        jsonResponse = new Response("FinishLineUpdate", new List<BlobEntity>());

                        if (jsonInput.player != null)
                        {
                            if (lobbyList.ElementAt(0).playerList.Any(it => it.blobUserId == jsonInput.player.blobUserId))
                            {
                                lock (toKeepEntitiesIntact)
                                {
                                    foreach (BlobEntity existingPlayerData in lobbyList.ElementAt(0).playerList)
                                    {
                                        if (existingPlayerData.blobUserId == jsonInput.player.blobUserId)
                                        {
                                            if (existingPlayerData.finishTime == -1)
                                            {
                                                TimeSpan timeDifference = DateTime.Now - lobbyList.ElementAt(0).raceStartTime;
                                                existingPlayerData.finishTime = (int)timeDifference.TotalSeconds;
                                                existingPlayerData.finishedRace = true;
                                                Debug.WriteLine(existingPlayerData.finishTime);
                                            }
                                        }

                                        jsonResponse.playerList.Add(existingPlayerData);
                                    }
                                }
                            }
                            if (lobbyList.ElementAt(0).playerList.All(it => it.finishTime != -1))
                            {
                                if (lobbyList.ElementAt(0).raceEndTime == DateTime.MinValue)
                                {
                                    lobbyList.ElementAt(0).raceEndTime = DateTime.Now.AddSeconds(15);
                                }
                                else
                                {
                                    jsonResponse.endTime = lobbyList.ElementAt(0).raceEndTime;
                                }    
                            }
                        }
                        else
                        {
                            jsonResponse = new Response("Error", new List<BlobEntity>());
                            jsonResponse.message = "Player object is null";
                        }
                    }
                    break;

                case "PlayerDisconnected":
                    {
                        if (jsonInput.player != null)
                        {
                            if (lobbyList.ElementAt(0).playerList.Any(it => it.blobUserId == jsonInput.player.blobUserId))
                            {
                                lock (toKeepEntitiesIntact)
                                {
                                    foreach (BlobEntity existingPlayerData in lobbyList.ElementAt(0).playerList)
                                    {
                                        if (existingPlayerData.blobUserId == jsonInput.player.blobUserId)
                                        {
                                            existingPlayerData.disconnected = true;

                                            if (lobbyList.ElementAt(0).controllingPlayerId == existingPlayerData.blobUserId)
                                            {
                                                lobbyList.ElementAt(0).controllingPlayerId = -1;
                                                lobbyList.ElementAt(0).controllingPlayerName = "";
                                            }

                                        }
                                    }
                                }

                            }
                        }
                        else
                        {
                            jsonResponse = new Response("Error", new List<BlobEntity>());
                            jsonResponse.message = "Player object is null";
                        }
                    }
                    break;

                default:
                    {
                        jsonResponse = new Response("Error", new List<BlobEntity>());
                        jsonResponse.message = "Unsupported type of update";
                    }
                    break;
            }
        }

        return JsonConvert.SerializeObject(jsonResponse);
    }
}