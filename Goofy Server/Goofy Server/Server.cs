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
    public int port;
    public TcpListener server;
    public List<Lobby> lobbyList;

    public Server()
    {
        lobbyList = new();
        port = 6066;
        server = new TcpListener(IPAddress.Any, port);
        server.Start(); // start the listener

        lobbyList.Add(new Lobby("Test!"));
    }

    public void RunServer()
    {
        try
        {
            TcpClient client = server.AcceptTcpClient(); // wait the user to connect
                                                         // Console.WriteLine("Client connected form " + ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString());
            HandleClient(client);
        }
        catch (Exception)
        {
        }
    }

    private void HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        StreamReader reader = new StreamReader(stream);
        StreamWriter writer = new StreamWriter(stream);

        try
        {
            string message = reader.ReadLine(); //read the message from the client
            writer.WriteLine(HandleMessage(message));
            writer.Flush();
            Debug.WriteLine("Server says: " + HandleMessage(message));
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

    private string HandleMessage(string jsonMessage)
    {
        Response jsonResponse = new Response("Update", new List<BlobEntity>());

        if (jsonMessage != null)
        {
            BlobEntity newPlayerData = JsonConvert.DeserializeObject<BlobEntity>(jsonMessage/*,
                    new JsonSerializerSettings
                    {
                        Error = delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
                        {
                            args.ErrorContext.Handled = true;
                        }
                    }*/);

            if (newPlayerData != null)
            {
                if (lobbyList.ElementAt(0).playerList.Any(it => it.blobUserId == newPlayerData.blobUserId))
                {
                    foreach (BlobEntity existingPlayerData in lobbyList.ElementAt(0).playerList)
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
                else
                {
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