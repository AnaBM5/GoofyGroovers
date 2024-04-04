using Goofy_Groovers.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PlatformGame.Managers;
using System;
using System.Collections.Generic;

namespace Goofy_Groovers
{
    public class GoofyGroovers : Game
    {
        private GameManager _gameManager;

        public enum GameState
        { LoginScreen, LobbyScreen, RaceScreen, LeaderBoardScreen };

        private List<Color> availableColors = new List<Color> { Color.Red, Color.Blue, Color.Green, Color.Yellow, Color.Purple, Color.Orange, Color.Pink, Color.Cyan };  //available colours
        private List<Color> assignedColors = new List<Color>();                                                                                                 //Colour that we already use
        private Random random = new Random();

        private GameState gameState = GameState.LoginScreen;
        private bool isKeyPressed = false;
        public string initials = string.Empty;

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
            _gameManager = new GameManager(this);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Load game content here if needed
            Globals._spriteBatch = new SpriteBatch(GraphicsDevice);
            Globals._gameFont = Content.Load<SpriteFont>("Fonts/Minecraft");

            Texture2D squareSprite = Content.Load<Texture2D>("squareSprite");
            _gameManager.squareTexture = squareSprite;
            _gameManager.getLevelManager().setGoalSprite(squareSprite);
            Globals._dotTexture = Content.Load<Texture2D>("dotSprite");
            //_gameManager.playerBlob.SetTexture(Globals._dotTexture);

            Texture2D platformSprite = Content.Load<Texture2D>("Sprites/foregroundSprite");
            _gameManager.getLevelManager().setPlatformSprite(platformSprite);
            Texture2D bgSprite = Content.Load<Texture2D>("Sprites/backgroundSprite");
            _gameManager.getLevelManager().setBackgroundSprite(bgSprite);
        }

        public bool GetPlayerInitialsFromUser()
        {
            KeyboardState keyboardState = Keyboard.GetState();
            Keys[] pressedKeys = keyboardState.GetPressedKeys();

            if (pressedKeys.Length > 0 && !isKeyPressed)
            {
                Keys firstKey = pressedKeys[0];

                if ((char.IsLetterOrDigit((char)firstKey) || firstKey == Keys.Space) && initials.Length <= 13)
                {
                    if (firstKey == Keys.Space && initials.Length == 0)
                    {
                        //We do nothing if the first key is space and there are no initials yet
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
                else if (gameState == GameState.LoginScreen && firstKey == Keys.Enter && initials.Length >= 3)
                {
                    int randomIndex = random.Next(availableColors.Count);
                    Color randomColor = availableColors[randomIndex];
                    _gameManager.playerBlob.SetUserColor(randomColor);
                    assignedColors.Add(randomColor);
                    availableColors.RemoveAt(randomIndex);

                    isKeyPressed = true;
                    _gameManager.playerBlob.SetUserName(initials);
                    return true;
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            switch (gameState)
            {
                case GameState.LoginScreen:
                    if (GetPlayerInitialsFromUser())
                    {
                        gameState = GameState.RaceScreen;  // Change game state to RaceScreen
                    }
                    break;

                case GameState.RaceScreen:
                    _gameManager.Update(gameTime, this);
                    break;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Add your drawing code here
            Globals._spriteBatch.Begin();
            switch (gameState)
            {
                case GameState.LoginScreen:
                    {
                        GraphicsDevice.Clear(Color.RoyalBlue);
                        string message = "PLEASE, ENTER YOUR INITIALS BELOW: ";

                        // Calculate the position to center the message horizontally and vertically
                        Vector2 messageSize = Globals._gameFont.MeasureString(message);
                        Vector2 position = new Vector2(
                            (GraphicsDevice.Viewport.Width - messageSize.X) / 2 - 25,
                            (GraphicsDevice.Viewport.Height - messageSize.Y) / 2 - 30);

                        Vector2 initialsSize = Globals._gameFont.MeasureString(initials);                                        // FIX: Breaks on (unsupported) characters, such as ":" 
                                                                                                                                 // FIX (UPDATE):  I made a few changes and the symbols are not allowed now,
                                                                                                                                 // but I do not know why the only symbols that are causing problems are:
                                                                                                                                 // ; // ' // [ , ] //  and // ` // but the other characters are not causing problems, as an example !,@,#,$,etc

                        Vector2 initialsPosition = new Vector2((GraphicsDevice.Viewport.Width - initialsSize.X - 30) / 2, 250); // Fixed vertical position for initials

                        Globals._spriteBatch.DrawString(Globals._gameFont, message, position, Color.White);
                        Globals._spriteBatch.DrawString(Globals._gameFont, initials, initialsPosition, Color.Black);
                    }
                    break;

                case GameState.RaceScreen:
                    {
                        GraphicsDevice.Clear(Color.DarkSlateBlue);
                        // Draw game objects using _spriteBatch
                        _gameManager.Draw(gameTime);
                        _gameManager.getMouseManager().Draw();
                    }
                    break;
            }
            Globals._spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}