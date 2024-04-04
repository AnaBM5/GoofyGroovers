using Goofy_Groovers.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Goofy_Groovers
{
    public class GoofyGroovers : Game
    {

        public enum GameState
        { LoginScreen, LobbyScreen, RaceScreen, LeaderBoardScreen };

        private List<Color> availableColors = new List<Color> { Color.Red, Color.Blue, Color.Green, Color.Yellow, Color.Purple, Color.Orange, Color.Pink, Color.Cyan };  //available colours
        private List<Color> assignedColors = new List<Color>();                                                                                                 //Colour that we already use
        private Random random = new Random();

        private GameState gameState = GameState.LoginScreen;
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
        int rect3Width = 140;
        int rect3Height = 70;
        int position3X = 330;
        int position3Y = 360;

        private Texture2D menuInterface;
        private Texture2D _blankTexture;

        public GoofyGroovers()
        {
            Globals._graphics = new GraphicsDeviceManager(this);

            //Not necessary for now but is going to be used later to have the game in full screen

            //Comment this section to deactivate full screen
            /*
            Globals._graphics.IsFullScreen = true;
            Globals._graphics.PreferredBackBufferWidth = 1920;
            Globals._graphics.PreferredBackBufferHeight = 1080;
            //-------------*/

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
            Task.Run(() =>
            {
                Globals._gameClient.ConnectAndCommunicate();
                });
            // Thread communicationThread = new Thread(GameClient.ConnectAndCommunicate());
            // communicationThread.Start();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Load game content here if needed
            Globals._spriteBatch = new SpriteBatch(GraphicsDevice);
            Globals._gameFont = Content.Load<SpriteFont>("Fonts/Minecraft");

            Texture2D squareSprite = Content.Load<Texture2D>("squareSprite");
            Globals._gameManager.squareTexture = squareSprite;
            Globals._dotTexture = Content.Load<Texture2D>("dotSprite");

            Globals._gameManager.playerBlob.SetTexture(Globals._dotTexture);

            Texture2D platformSprite = Content.Load<Texture2D>("Sprites/foregroundSprite");
            Globals._gameManager.getLevelManager().setPlatformSprite(platformSprite);
            Texture2D bgSprite = Content.Load<Texture2D>("Sprites/backgroundSprite");
            Globals._gameManager.getLevelManager().setBackgroundSprite(bgSprite);

            menuInterface = Content.Load<Texture2D>("Interfaces/menu");
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
                if ((char.IsDigit((char)firstKey) || firstKey == Keys.OemPeriod) && ipAddress.Length <= 13)
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
                        ipAddress += keyToAdd;
                        isKeyPressed = true;
                    }
                }
                else if (firstKey == Keys.Back && ipAddress.Length > 0)
                {
                    ipAddress = ipAddress.Substring(0, ipAddress.Length - 1);
                    isKeyPressed = true;

                }
                int randomIndex = random.Next(availableColors.Count);
                    Color randomColor = availableColors[randomIndex];
                    Globals._gameManager.playerBlob.SetUserColor(randomColor);
                    assignedColors.Add(randomColor);
                    availableColors.RemoveAt(randomIndex);
                    Globals._gameManager.playerBlob.SetUserName(initials);
                    return true;
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            switch (gameState)
            {
                case GameState.LoginScreen:

                    MouseState mouseState = Mouse.GetState();
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
                        if ( rect3.Contains(mouseState.Position) && initials.Length >= 3 && ipAddress.Length >= 3)
                        {
                           
                            int randomIndex = random.Next(availableColors.Count); 
                            Color randomColor = availableColors[randomIndex];
                            Globals._gameManager.playerBlob.SetUserColor(randomColor);  //We set a color to the player
                            assignedColors.Add(randomColor);                    //We add the selected color to the temporal array
                            availableColors.RemoveAt(randomIndex);                  //We remove that color from the original array

                            isKeyPressed = true;
                            Globals._gameManager.playerBlob.SetUserName(initials);
                            gameState = GameState.RaceScreen;                       //We change the game state if all the requirements are met                    
                        }
                    }
                    if (GetPlayerKeyInputFromUser())
                    {
                        gameState = GameState.RaceScreen;                       // Change game state to RaceScreen            
                    }
                    break;

                case GameState.RaceScreen:
                    Globals._gameManager.Update(gameTime, this);
                    break;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            float scaleX = (float)GraphicsDevice.Viewport.Width / menuInterface.Width;
            float scaleY = (float)GraphicsDevice.Viewport.Height / menuInterface.Height;

            // Add your drawing code here
            Globals._spriteBatch.Begin();
            switch (gameState)
            {
                case GameState.LoginScreen:
                    {

                        GraphicsDevice.Clear(Color.RoyalBlue);

                        Globals._spriteBatch.Draw(menuInterface, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);

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

                case GameState.RaceScreen:
                    {
                        GraphicsDevice.Clear(Color.DarkSlateBlue);
                        // Draw game objects using _spriteBatch
                        Globals._gameManager.Draw(gameTime);
                        Globals._gameManager.getMouseManager().Draw();
                    }
                    break;
            }
            Globals._spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}