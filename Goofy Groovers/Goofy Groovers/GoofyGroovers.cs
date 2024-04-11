using Goofy_Groovers.Entity;
using Goofy_Groovers.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Goofy_Groovers
{
    public class GoofyGroovers : Game
    {
        public enum GameState
        { LoginScreen, LobbyScreen, RaceScreen, LeaderBoardScreen, QuitScreen, Closed };

        public static GameState gameState = GameState.LoginScreen;
        private GameState previousGameState;

        private List<Color> availableColors = new List<Color> { Color.Red, Color.Blue, Color.Green, Color.Yellow, Color.Purple, Color.Orange, Color.Pink, Color.Cyan };  //available colours
        private List<Color> assignedColors = new List<Color>();                                                                                                 //Colour that we already use
        private Random random = new Random();

        private bool isKeyPressed = false;
        private bool isRectClicked = false;
        private bool isIPRectClicked = false;

        private string ipAddress = string.Empty;
        public string initials = string.Empty;

        //We calculate the position and size of the rectangle
        private int rectWidth = 400;

        private int rectHeight = 40;
        private int positionX = 230;
        private int positionY = 275;
        private int rect2PositionY = 165;

        //We draw the rectangle 3
        private int rect3Width = 140;

        private int rect3Height = 70;
        private int position3X = 330;
        private int position3Y = 360;

        private float rectQWidth;
        private float rectQHeight;
        private float rect1QX;
        private float rect1QY;
        private float rect2QX;
        private float rect2QY;
        private readonly GameManager gm = Globals._gameManager;

        //Interfaces
        private Texture2D menuInterface;

        private Texture2D quitInterface;
        private Texture2D leaderBoardInterface;
        private Texture2D lobbyInterface;
        private Texture2D _blankTexture;

        private int leaderBoardPosition;

        public GoofyGroovers()
        {
            Globals._graphics = new GraphicsDeviceManager(this);

            //Not necessary for now but is going to be used later to have the game in full screen

            //Comment this section to deactivate full screen
            /*
            Globals._graphics.IsFullScreen = true;
            Globals._graphics.PreferredBackBufferWidth = 1920;
            Globals._graphics.PreferredBackBufferHeight = 1080;
         */

            Globals.windowWidth = (ushort)Globals._graphics.PreferredBackBufferWidth;
            Globals.windowHeight = (ushort)Globals._graphics.PreferredBackBufferHeight;

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Communicate information to server as the name/color have been chosen
            // and load the information about the other blobs.
            Globals._gameManager = new GameManager(this);
            Globals._gameClient = new GameClient();

            // Thread thread = new Thread(() =>
            // {
            //     Globals._gameClient.ConnectAndCommunicate();
            // });
            // thread.Start();

            // _ = Task.Run(() => Globals._gameClient.ConnectAndCommunicate());

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Load game content here if needed
            Globals._spriteBatch = new SpriteBatch(GraphicsDevice);
            Globals._gameFont = Content.Load<SpriteFont>("Fonts/Minecraft");

            Texture2D squareSprite = Content.Load<Texture2D>("squareSprite");

            Globals._gameManager.squareTexture = squareSprite;
            Globals._gameManager.getLevelManager().setGoalSprite(squareSprite);
            Globals._dotTexture = Content.Load<Texture2D>("dotSprite");
            Globals._gameManager.countdownFont = Content.Load<SpriteFont>("Fonts/CountdownFont");
            Globals._gameManager.playerBlob.SetTexture(Globals._dotTexture);

            Globals._dotJumpTexture = Content.Load<Texture2D>("dotSpriteJump");
            Globals._gameManager.playerBlob.SetTexture(Globals._dotJumpTexture);

            Globals._dotClickTexture = Content.Load<Texture2D>("dotClickSprite");
            Globals._gameManager.playerBlob.SetTexture(Globals._dotClickTexture);
            Globals._dotClickTextureUP = Content.Load<Texture2D>("dotClickSpriteUP");
            Globals._gameManager.playerBlob.SetTexture(Globals._dotClickTextureUP);
            Globals._dotClickTextureRigth = Content.Load<Texture2D>("dotClickSpriteRigth");
            Globals._gameManager.playerBlob.SetTexture(Globals._dotClickTextureRigth);
            Globals._dotClickTexturLeft = Content.Load<Texture2D>("dotClickSpriteLeft");
            Globals._gameManager.playerBlob.SetTexture(Globals._dotClickTexturLeft);

            Globals._dotLeftTexture = Content.Load<Texture2D>("dotSpriteLeft");
            Globals._gameManager.playerBlob.SetTexture(Globals._dotLeftTexture);
            Globals._dotRighttTexture = Content.Load<Texture2D>("dotSpriteRight");
            Globals._gameManager.playerBlob.SetTexture(Globals._dotRighttTexture);
            Globals._dotUpTexture = Content.Load<Texture2D>("dotSpriteUp");
            Globals._gameManager.playerBlob.SetTexture(Globals._dotUpTexture);

            Globals._gameManager.countdownFont = Content.Load<SpriteFont>("Fonts/CountdownFont");

            Texture2D platformSprite = Content.Load<Texture2D>("Sprites/foregroundSprite");
            Globals._gameManager.getLevelManager().setPlatformSprite(platformSprite);
            Texture2D bgSprite = Content.Load<Texture2D>("Sprites/backgroundSprite");
            Globals._gameManager.getLevelManager().setBackgroundSprite(bgSprite);
            Globals._gameManager.overlayScreen = Content.Load<Texture2D>("Interfaces/OverlayScreen");

            menuInterface = Content.Load<Texture2D>("Interfaces/menu");
            quitInterface = Content.Load<Texture2D>("Interfaces/Quit Interface");
            leaderBoardInterface = Content.Load<Texture2D>("Interfaces/LeaderBoard");
            lobbyInterface = Content.Load<Texture2D>("Interfaces/LobbyScreen");

            _blankTexture = new Texture2D(GraphicsDevice, 1, 1);
            _blankTexture.SetData(new[] { Color.White });
        }

        public bool GetPlayerKeyInputFromUser()
        {
            KeyboardState keyboardState = Keyboard.GetState();
            Keys[] pressedKeys = keyboardState.GetPressedKeys();

            if (isRectClicked && pressedKeys.Length > 0 && !isKeyPressed)
            {
                Keys firstKey = pressedKeys[0];

                if ((char.IsLetterOrDigit((char)firstKey) || firstKey == Keys.Space) && initials.Length <= 13)
                {
                    if (firstKey == Keys.Space && initials.Length == 0)
                    {
                        // We do nothing if the first key is a space key and there are no initials yet.
                    }
                    else
                    {
                        initials += (firstKey == Keys.Space) ? " " : ((char)firstKey).ToString();
                        isKeyPressed = true;
                    }
                }
                else if (firstKey == Keys.Back && initials.Length > 0)
                {
                    initials = initials.Substring(0, initials.Length - 1);
                    isKeyPressed = true;
                }
            }
            else if (isIPRectClicked && pressedKeys.Length > 0 && !isKeyPressed)
            {
                Keys firstKey = pressedKeys[0];
                if ((char.IsLetterOrDigit((char)firstKey) || firstKey == Keys.OemPeriod || firstKey == Keys.NumPad0) && ipAddress.Length <= 13)
                {
                    if ((firstKey == Keys.Space || firstKey == Keys.OemPeriod) && ipAddress.Length == 0)
                    {
                        //We do nothing if the first key is a space or period and there is no IP address yet.
                    }
                    else
                    {
                        string keyToAdd = "";

                        if (firstKey == Keys.OemPeriod)
                        {
                            keyToAdd = ".";
                        }
                        else
                        {
                            keyToAdd = ((char)firstKey).ToString();
                        }
                        if (firstKey == Keys.NumPad0)
                        {
                            keyToAdd = "0";
                        }
                        else if (keyboardState.IsKeyDown(Keys.NumPad1))
                        {
                            keyToAdd = "1";
                        }
                        else if (keyboardState.IsKeyDown(Keys.NumPad2))
                        {
                            keyToAdd = "2";
                        }
                        else if (keyboardState.IsKeyDown(Keys.NumPad3))
                        {
                            keyToAdd = "3";
                        }
                        else if (keyboardState.IsKeyDown(Keys.NumPad4))
                        {
                            keyToAdd = "4";
                        }
                        else if (keyboardState.IsKeyDown(Keys.NumPad5))
                        {
                            keyToAdd = "5";
                        }
                        else if (keyboardState.IsKeyDown(Keys.NumPad6))
                        {
                            keyToAdd = "6";
                        }
                        else if (keyboardState.IsKeyDown(Keys.NumPad7))
                        {
                            keyToAdd = "7";
                        }
                        else if (keyboardState.IsKeyDown(Keys.NumPad8))
                        {
                            keyToAdd = "8";
                        }
                        else if (keyboardState.IsKeyDown(Keys.NumPad9))
                        {
                            keyToAdd = "9";
                        }
                        else if (keyboardState.IsKeyDown(Keys.Decimal))
                        {
                            keyToAdd = ".";
                        }
                        ipAddress += keyToAdd;
                        isKeyPressed = true;
                    }
                }
                else if (firstKey == Keys.Back && ipAddress.Length > 0)
                {
                    ipAddress = ipAddress.Substring(0, ipAddress.Length - 1);
                    isKeyPressed = true;
                }
            }
            else if (pressedKeys.Length == 0)
            {
                isKeyPressed = false;
            }

            return false;
        }

        public int GetInitialsLenght()                                                                     //We create a method to get the players initials lenght
        {
            return initials.Length;
        }

        protected override void Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();
            KeyboardState keyboardState = Keyboard.GetState();

            switch (gameState)
            {
                case GameState.LoginScreen:

                    if (keyboardState.IsKeyDown(Keys.Escape))
                    {
                        previousGameState = GameState.LoginScreen;                  //We add the previous state
                        gameState = GameState.QuitScreen;
                    }

                    if (mouseState.LeftButton == ButtonState.Pressed)
                    {
                        Rectangle rect1 = new Rectangle(positionX, positionY, rectWidth, rectHeight);
                        Rectangle rect2 = new Rectangle(positionX, rect2PositionY, rectWidth, rectHeight);
                        Rectangle rect3 = new Rectangle(position3X, position3Y, rect3Width, rect3Height);
                        if (rect1.Contains(mouseState.Position))
                        {
                            isRectClicked = true;
                            isIPRectClicked = false;
                        }
                        if (rect2.Contains(mouseState.Position))
                        {
                            isRectClicked = false;
                            isIPRectClicked = true;
                        }
                        if (rect3.Contains(mouseState.Position) && initials.Length >= 3 && ipAddress.Length >= 3)
                        {
                            int randomIndex = random.Next(availableColors.Count);
                            Color randomColor = availableColors[randomIndex];
                            Globals._gameManager.playerBlob.SetUserColor(randomColor);  //We set a color to the player
                            assignedColors.Add(randomColor);                            //We add the selected color to the temporal array
                            availableColors.RemoveAt(randomIndex);                      //We remove that color from the original array

                            isKeyPressed = true;
                            Globals._gameManager.playerBlob.SetUserName(initials);
                            Globals._gameClient.SetServerName(ipAddress);

                            gameState = GameState.LobbyScreen;                                       //We change the game state if all the requirements are met

                            //Changes to full screen when the game/lobby starts
                            /*
                            Globals._graphics.IsFullScreen = true;
                            Globals._graphics.PreferredBackBufferWidth = 1920;
                            Globals._graphics.PreferredBackBufferHeight = 1080;

                            Globals.windowWidth = (ushort)Globals._graphics.PreferredBackBufferWidth;
                            Globals.windowHeight = (ushort)Globals._graphics.PreferredBackBufferHeight;

                            Globals._graphics.ApplyChanges();
                            */
                            leaderBoardPosition = Globals.windowWidth;
                        }
                    }
                    if (GetPlayerKeyInputFromUser())
                    {
                        //gameState = GameState.RaceScreen;                                           // Change game state to RaceScreen
                    }
                    break;

                case GameState.LobbyScreen:
                    {
                        Globals._gameManager.elapsedSecondsSinceTransmissionToServer += gameTime.ElapsedGameTime.TotalSeconds;
                        if (Globals._gameManager.elapsedSecondsSinceTransmissionToServer >= 0.15)
                        {
                            Globals._gameManager.elapsedSecondsSinceTransmissionToServer = 0;
                            _ = Task.Run(() => Globals._gameClient.ConnectAndCommunicate(gameState));
                        }

                        if (keyboardState.IsKeyDown(Keys.Enter))
                        {
                            if (Globals._gameManager.playerBlob.isStartingTheRace)
                            {
                                Globals._gameManager.raceStartTime = DateTime.Now.AddSeconds(5);
                                gameState = GameState.RaceScreen;
                            }
                        }

                        if (keyboardState.IsKeyDown(Keys.Escape))
                        {
                            previousGameState = GameState.LobbyScreen;                  //We add the previous state
                            gameState = GameState.QuitScreen;
                        }
                    }
                    break;

                case GameState.RaceScreen:
                    {
                        Globals._gameManager.Update(gameTime, this);

                        Globals._gameManager.elapsedSecondsSinceTransmissionToServer += gameTime.ElapsedGameTime.TotalSeconds;
                        if (Globals._gameManager.elapsedSecondsSinceTransmissionToServer >= 0.15)
                        {
                            Globals._gameManager.elapsedSecondsSinceTransmissionToServer = 0;
                            _ = Task.Run(() => Globals._gameClient.ConnectAndCommunicate(gameState));
                        }

                        if (keyboardState.IsKeyDown(Keys.Escape))
                        {
                            previousGameState = GameState.RaceScreen;                                   //We add the previous game state
                            gameState = GameState.QuitScreen;
                        }
                    }
                    break;

                case GameState.QuitScreen:

                    rectQWidth = GraphicsDevice.Viewport.Width * 0.2f;
                    rectQHeight = GraphicsDevice.Viewport.Height * 0.12f;

                    rect1QX = GraphicsDevice.Viewport.Width * 0.22f;
                    rect1QY = GraphicsDevice.Viewport.Height * 0.77f;

                    rect2QX = GraphicsDevice.Viewport.Width * 0.59f;
                    rect2QY = GraphicsDevice.Viewport.Height * 0.77f;

                    Rectangle rect1Q = new Rectangle((int)rect1QX, (int)rect1QY, (int)rectQWidth, (int)rectQHeight);
                    Rectangle rect2Q = new Rectangle((int)rect2QX, (int)rect2QY, (int)rectQWidth, (int)rectQHeight);

                    if (mouseState.LeftButton == ButtonState.Pressed)
                    {
                        if (rect1Q.Contains(mouseState.Position))
                        {
                            Exit(); //We close the program
                        }
                        else if (rect2Q.Contains(mouseState.Position))   //If the user press the button "No" we go back to the previous game state
                        {
                            if (previousGameState == GameState.LoginScreen)
                            {
                                gameState = GameState.LoginScreen;
                            }
                            else if (previousGameState == GameState.RaceScreen)
                            {
                                gameState = GameState.RaceScreen;
                            }
                            else if (previousGameState == GameState.LeaderBoardScreen)
                            {
                                gameState = GameState.LeaderBoardScreen;
                            }
                            else if (previousGameState == GameState.LobbyScreen)
                            {
                                gameState = GameState.LobbyScreen;
                            }
                        }
                    }
                    break;

                case GameState.LeaderBoardScreen:
                    {
                        Globals._gameManager.Update(gameTime, this);

                        Globals._gameManager.elapsedSecondsSinceTransmissionToServer += gameTime.ElapsedGameTime.TotalSeconds;
                        if (Globals._gameManager.elapsedSecondsSinceTransmissionToServer >= 0.15)
                        {
                            Globals._gameManager.elapsedSecondsSinceTransmissionToServer = 0;
                            _ = Task.Run(() => Globals._gameClient.ConnectAndCommunicate(gameState));
                        }

                        if (leaderBoardPosition > 0)
                        {
                            leaderBoardPosition -= (int)gameTime.ElapsedGameTime.TotalMilliseconds * 2;

                            if (leaderBoardPosition < 0)
                                leaderBoardPosition = 0;
                        }

                        if (keyboardState.IsKeyDown(Keys.Escape))
                        {
                            previousGameState = GameState.LeaderBoardScreen;                //We add the state
                            gameState = GameState.QuitScreen;
                        }
                    }
                    break;
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            float scaleXMenu = (float)GraphicsDevice.Viewport.Width / menuInterface.Width;
            float scaleYMenu = (float)GraphicsDevice.Viewport.Height / menuInterface.Height;
            float scaleXQuit = (float)GraphicsDevice.Viewport.Width / quitInterface.Width;
            float scaleYQuit = (float)GraphicsDevice.Viewport.Height / quitInterface.Height;
            float scaleXLeader = (float)GraphicsDevice.Viewport.Width / leaderBoardInterface.Width;
            float scaleYLeader = (float)GraphicsDevice.Viewport.Height / leaderBoardInterface.Height;
            float scaleXLobby = (float)GraphicsDevice.Viewport.Width / lobbyInterface.Width;
            float scaleYLobby = (float)GraphicsDevice.Viewport.Height / lobbyInterface.Height;

            // Add your drawing code here
            Globals._spriteBatch.Begin();
            switch (gameState)
            {
                case GameState.LoginScreen:
                    {
                        GraphicsDevice.Clear(Color.RoyalBlue);

                        Globals._spriteBatch.Draw(menuInterface, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, new Vector2(scaleXMenu, scaleYMenu), SpriteEffects.None, 0f);
                        Globals._spriteBatch.DrawString(Globals._gameFont, initials, new Vector2(250, 290), Color.Black);

                        Vector2 centerPosition = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);

                        //We draw the rectangle 1
                        Rectangle rect1 = new Rectangle(positionX, positionY, rectWidth, rectHeight);
                        Globals._spriteBatch.Draw(_blankTexture, rect1, Color.White);

                        //We draw the rectangle 2
                        Rectangle rect2 = new Rectangle(positionX, rect2PositionY, rectWidth, rectHeight);
                        Globals._spriteBatch.Draw(_blankTexture, rect2, Color.White);

                        Rectangle rect3 = new Rectangle(position3X, position3Y, rect3Width, rect3Height);
                        Globals._spriteBatch.Draw(_blankTexture, rect3, Color.Transparent);

                        //We draw the player's initials
                        Globals._spriteBatch.DrawString(Globals._gameFont, initials, new Vector2(250, 290), Color.Black);

                        //We draw the Server's Ip
                        Globals._spriteBatch.DrawString(Globals._gameFont, ipAddress, new Vector2(250, 180), Color.Black);
                    }
                    break;

                case GameState.LobbyScreen:

                    float scaleX = (float)GraphicsDevice.Viewport.Width / leaderBoardInterface.Width;
                    float scaleY = (float)GraphicsDevice.Viewport.Height / leaderBoardInterface.Height;

                    // Calcular la nueva posición y tamaño del texto
                    Vector2 newPosition = new Vector2(400 * scaleX, 830 * scaleY);
                    float newSize = 5.5f * scaleX;

                    GraphicsDevice.Clear(Color.DarkSlateBlue);
                    Globals._spriteBatch.Draw(lobbyInterface, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, new Vector2(scaleXLobby, scaleYLobby), SpriteEffects.None, 0f);

                    if (Globals._gameManager.raceStarter != null)
                    {
                        Globals._spriteBatch.DrawString(Globals._gameFont, "      Waiting for: \n          "
                            + Globals._gameManager.raceStarter
                            + "\n  to start the race.", newPosition, Color.Black, 0f, Vector2.Zero, newSize, SpriteEffects.None, 0f);
                    }

                    for (int iterator = 0; iterator < Globals._gameManager.blobEntities.Count; iterator++)
                    {
                        if(!Globals._gameManager.blobEntities.ElementAt(iterator).disconnected)
                            Globals._spriteBatch.DrawString(Globals._gameFont, Globals._gameManager.blobEntities.ElementAt(iterator).blobUserName,
                                new Vector2(1390 * scaleX, (340 + iterator * 90) * scaleY), Color.Black, 0f, Vector2.Zero, newSize, SpriteEffects.None, 0f);
                    }

                    break;

                case GameState.RaceScreen:
                    {
                        GraphicsDevice.Clear(Color.DarkSlateBlue);
                        Globals._gameManager.Draw(gameTime);
                        //Globals._gameManager.getMouseManager().Draw();
                    }
                    break;

                case GameState.QuitScreen:
                    {
                        rectQWidth = GraphicsDevice.Viewport.Width * 0.2f;
                        rectQHeight = GraphicsDevice.Viewport.Height * 0.12f;

                        rect1QX = GraphicsDevice.Viewport.Width * 0.22f;
                        rect1QY = GraphicsDevice.Viewport.Height * 0.77f;

                        rect2QX = GraphicsDevice.Viewport.Width * 0.59f;
                        rect2QY = GraphicsDevice.Viewport.Height * 0.77f;
                        Globals._spriteBatch.Draw(quitInterface, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, new Vector2(scaleXQuit, scaleYQuit), SpriteEffects.None, 0f);

                        // yes button
                        Rectangle rect1Q = new Rectangle((int)rect1QX, (int)rect1QY, (int)rectQWidth, (int)rectQHeight);
                        Globals._spriteBatch.Draw(_blankTexture, rect1Q, Color.Transparent);

                        //no button
                        Rectangle rect2Q = new Rectangle((int)rect2QX, (int)rect2QY, (int)rectQWidth, (int)rectQHeight);
                        Globals._spriteBatch.Draw(_blankTexture, rect2Q, Color.Transparent);
                    }
                    break;

                case GameState.LeaderBoardScreen:
                    {
                        string spaces = "                              ";
                        scaleX = (float)GraphicsDevice.Viewport.Width / leaderBoardInterface.Width;
                        scaleY = (float)GraphicsDevice.Viewport.Height / leaderBoardInterface.Height;
                        GraphicsDevice.Clear(Color.DarkSlateBlue);
                        // Draw game objects using _spriteBatch
                        Globals._gameManager.Draw(gameTime);

                        //GraphicsDevice.Clear(Color.RoyalBlue);
                        Globals._spriteBatch.Draw(leaderBoardInterface, new Vector2(leaderBoardPosition, 0), null, Color.White, 0f, Vector2.Zero, new Vector2(scaleXLeader, scaleYLeader), SpriteEffects.None, 0f);

                        List<BlobEntity> blobEntities = new List<BlobEntity>(Globals._gameManager.blobEntities);
                        blobEntities.RemoveAll(entity => entity.finishTime == -1);
                        blobEntities.Sort((x, y) => x.finishTime.CompareTo(y.finishTime));
                        for (int iterator = 0; iterator < blobEntities.Count; iterator++)
                        {
                            Globals._spriteBatch.DrawString(Globals._gameFont, (iterator + 1) +
                                spaces + AdjustStringLength(blobEntities.ElementAt(iterator).blobUserName, 6) +
                                spaces + Globals._gameManager.FormatTime(blobEntities.ElementAt(iterator).finishTime),
                                new Vector2(430 * scaleX, (500 + iterator * 100) * scaleY), Color.Black, 0f, Vector2.Zero, (float)1.5f, SpriteEffects.None, 0f);
                        }

                        if (Globals._gameManager.raceEndTime != DateTime.MinValue)
                        {
                            if((Globals._gameManager.raceEndTime - DateTime.Now).TotalSeconds <= 15 && (Globals._gameManager.raceEndTime - DateTime.Now).TotalSeconds > 0)
                            {
                                Globals._spriteBatch.DrawString(Globals._gameFont, ((int)(Globals._gameManager.raceEndTime - DateTime.Now).TotalSeconds).ToString(),
                                    new Vector2(2702 * scaleX, 1901 * scaleY), Color.Black, 0f, Vector2.Zero, (float)2.9f, SpriteEffects.None, 0f);
                            }
                        }
                    }
                    break;
            }
            Globals._spriteBatch.End();

            base.Draw(gameTime);
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            // Send meessage to server
            Globals._gameClient.ConnectAndCommunicate(GameState.Closed);

            base.OnExiting(sender, args);
        }

        static string AdjustStringLength(string str, int length)
        {
            if (str.Length > length)
            {
                return str.Substring(0, length - 3) + "...";
            }
            else if (str.Length < length)
            {
                return str.PadRight(length);
            }
            else
            {
                return str;
            }
        }
    }
}