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
using System.Threading.Tasks;

public class Response
{
    public string responseType;
    public List<BlobEntity> playerList;

    public Response()
    { }

    public Response(string responseType)
    {
        this.responseType = responseType;
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

    public static readonly Object toKeepEntitiesIntact = new Object();

    public Server()
    {
    }

    public static void HandleClient(TcpClient client)
    {
        // Create a Stopwatch instance
        Stopwatch stopwatch = new Stopwatch();
        // Start the stopwatch
        stopwatch.Start();

        NetworkStream stream = client.GetStream();
        StreamReader reader = new(stream);
        StreamWriter writer = new(stream);

        try
        {
            string message = reader.ReadLine(); //read the message from the client
            writer.WriteLine(HandleMessage(message));
            writer.Flush();
            //Debug.WriteLine("Server says: " + HandleMessage(message));
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.ToString());
        }
        finally
        {
            client.Close(); // We close the connection

            stopwatch.Stop(); // Stop the stopwatch
            //Debug.WriteLine($"Execution time: {stopwatch.Elapsed.TotalMilliseconds} milliseconds"); // Display the elapsed time
        }
    }

    private static string HandleMessage(string jsonMessage)
    {
        Response jsonResponse = new Response("Update", new List<BlobEntity>());
        if (jsonMessage != null)
        {
            BlobEntity newPlayerData = JsonConvert.DeserializeObject<BlobEntity>(jsonMessage);
            Debug.WriteLine("update from:" + newPlayerData.blobUserName);

            if (newPlayerData != null)
            {
                if (lobbyList.ElementAt(0).playerList.Any(it => it.blobUserId == newPlayerData.blobUserId))
                {
                    lock (toKeepEntitiesIntact)
                    {
                        foreach (BlobEntity existingPlayerData in lobbyList.ElementAt(0).playerList)
                        {
                            {
                                if (existingPlayerData.blobUserId == newPlayerData.blobUserId)
                                {
                                    existingPlayerData.blobUserName = newPlayerData.blobUserName;
                                    existingPlayerData.blobUserColor = newPlayerData.blobUserColor;
                                    existingPlayerData.worldPosition = newPlayerData.worldPosition;
                                    existingPlayerData.isJumping = newPlayerData.isJumping;
                                    existingPlayerData.jumpStartPoint = newPlayerData.jumpStartPoint;
                                    existingPlayerData.jumpEndPoint = newPlayerData.jumpEndPoint;
                                    existingPlayerData.velocity = newPlayerData.velocity;
                                    existingPlayerData.jumpTheta = newPlayerData.jumpTheta;
                                }
                                else
                                {
                                    jsonResponse.playerList.Add(existingPlayerData);
                                }
                            }
                        }
                    }
                }
                else
                {
                    lock (toKeepEntitiesIntact)
                        lobbyList.ElementAt(0).playerList.Add(newPlayerData);
                }
            }
        }
        else if (lobbyList.ElementAt(0).playerList.Count != 0)
        {
            jsonResponse = new Response("Error", new List<BlobEntity>());
        }

        // Response type:
        // Error, Confirmation, Update, Direction ?

        return JsonConvert.SerializeObject(jsonResponse);
    }
}