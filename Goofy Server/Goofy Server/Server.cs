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
    public int startTime;
    public int raceStarterId;
    public string raceStarter;
    public BlobEntity player;

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

    public static readonly Object toKeepEntitiesIntact = new Object();

    public Server()
    {
    }

    private string GetLocalIP()
    {
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
        socket.Connect("8.8.8.8", 65530);
        IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
        try
        {
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
        if (jsonMessage != null)
        {
            switch (jsonInput.messageType)
            {
                case "LobbyWait":
                    {
                        if (lobbyList.ElementAt(0).playerList.Count == 0)
                        {
                            lobbyList.ElementAt(0).playerList.Add(jsonInput.player);
                            lobbyList.ElementAt(0).controllingPlayerId = jsonInput.player.blobUserId;
                            lobbyList.ElementAt(0).controllingPlayerName = jsonInput.player.blobUserName;
                        }

                        if (lobbyList.ElementAt(0).raceStartTime != -1)
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
                            var responseMessage = new
                            {
                                messageType = "SetRaceStarter",
                                raceStarterId = lobbyList.ElementAt(0).controllingPlayerId,
                                raceStarter = lobbyList.ElementAt(0).controllingPlayerName,
                            };
                            return JsonConvert.SerializeObject(responseMessage);
                        }
                    }

                case "RaceStart":
                    {
                        lobbyList.ElementAt(0).raceStartTime = jsonInput.startTime;
                        Debug.WriteLine(jsonInput.startTime);
                        var responseMessage = new
                        {
                            messageType = "OK",
                        };
                        return JsonConvert.SerializeObject(responseMessage);
                    }
                case "RaceUpdate":
                    {
                        jsonResponse = new Response("Update", new List<BlobEntity>());

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

                        return JsonConvert.SerializeObject(jsonResponse);
                    }

                case "FinishLineUpdate":
                    {
                        jsonResponse = new Response("Update", new List<BlobEntity>());

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
                                            existingPlayerData.finishTime = jsonInput.player.finishTime;
                                        }
                                        else
                                        {
                                            jsonResponse.playerList.Add(existingPlayerData);
                                        }
                                    }
                                }
                            }
                        }
                        return JsonConvert.SerializeObject(jsonResponse);
                    }

                default:
                    {

                        jsonResponse = new Response("Error", new List<BlobEntity>());
                    }
                    break;
            }
        }


        // Response type:
        // Error, Confirmation, Update, Direction ?

        return JsonConvert.SerializeObject(jsonResponse);
    }
}