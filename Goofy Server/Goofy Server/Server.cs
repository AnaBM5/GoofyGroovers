using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Goofy_Server
{
    public class Response
    {
        public string responseType;
        public List<BlobEntity> playerList;

        public Response() {}

        public Response(string responseType)
        {
            this.responseType = responseType;
        }

        public Response(string responseType, List<BlobEntity> playerList) : this(responseType)
        {
            this.playerList = playerList;
        }
    }

    public class Server : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private int port;
        private TcpListener server;
        private List<Lobby> lobbyList;

        public Server()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            lobbyList = new();
            port = 6066;
            server = new TcpListener(IPAddress.Any, port);
            server.Start(); // start the listener

            lobbyList.Add(new Lobby("Test!"));
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            try
            {
                TcpClient client = server.AcceptTcpClient(); // wait the user to connect
                // Console.WriteLine("Client connected form " + ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString());

                Thread t = new Thread(new ParameterizedThreadStart(HandleClient));
                t.Start(client); //We initialize a new thread
            }
            catch (Exception)
            {
            }
            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }

        private void HandleClient(object obj)
        {
            TcpClient client = (TcpClient)obj;

            NetworkStream stream = client.GetStream();
            StreamReader reader = new StreamReader(stream);
            StreamWriter writer = new StreamWriter(stream);

            try
            {
                string message = reader.ReadLine(); //read the message from the client
                Debug.WriteLine("Client says: " + message);

                Debug.WriteLine("Server says: " + HandleMessage(message));
                //writer.WriteLine(); // we send the message to the client
                //writer.Flush();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                client.Close(); // We close the connection
            }
        }

        private string HandleMessage(string jsonMessage)
        {
            Response jsonResponse = new Response("Update", new List<BlobEntity>());

            if (!jsonMessage.Equals(""))
            {
                BlobEntity newPlayerData = JsonConvert.DeserializeObject<BlobEntity>(jsonMessage,
                    new JsonSerializerSettings
                    {
                        Error = delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
                        {
                            args.ErrorContext.Handled = true;
                        }
                    });
                if (lobbyList.ElementAt(0).playerList.Count == 0)
                {
                    lobbyList.ElementAt(0).playerList.Add(newPlayerData);
                }

                if (lobbyList.ElementAt(0).playerList.Any(it => it.blobUserId == newPlayerData.blobUserId))
                {
                    foreach (BlobEntity existingPlayerData in lobbyList.ElementAt(0).playerList)
                    {
                        if (existingPlayerData.blobUserId == newPlayerData.blobUserId)
                        {
                            existingPlayerData.position = newPlayerData.position;
                            if (newPlayerData.isJumping)
                            {
                                existingPlayerData.isJumping = true;
                                existingPlayerData.jumpStartPoint = newPlayerData.jumpStartPoint;
                                existingPlayerData.jumpEndPoint = newPlayerData.jumpEndPoint;
                            }
                            else
                            {
                                existingPlayerData.isJumping = false;
                            }
                        }
                        else
                        {
                            jsonResponse.playerList.Add(existingPlayerData);
                        }
                    }
                }
                else
                {
                    jsonResponse.playerList = lobbyList.ElementAt(0).playerList;
                    lobbyList.ElementAt(0).playerList.Add(newPlayerData);
                }

                // Response type:
                // Error, Confirmation, Update, Direction ?
            }
            //return "oopsie";
            return JsonConvert.SerializeObject(jsonResponse);
        }
    }
}