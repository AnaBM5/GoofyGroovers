using Goofy_Groovers.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PlatformGame.Managers;
using System;
using System.Collections.Generic;
using static Goofy_Groovers.GoofyGroovers;

namespace Goofy_Groovers
{
    public class GoofyGroovers : Game
    {
        private GameManager _gameManager;
        public enum GameState {LoginScreen, LobbyScreen,RaceScreen,LeaderBoardScreen};
        private List<Color> availableColors = new List<Color>{Color.Red,Color.Blue,Color.Green,Color.Yellow,Color.Purple, Color.Orange,Color.Pink,Color.Cyan};  //available colours
        private List<Color> assignedColors = new List<Color>();                                                                                                 //Colour that we already use
        private Random random = new Random();


        GameState gameState = GameState.LoginScreen;
        private bool isKeyPressed = false;
        public string initials = string.Empty;
        public GoofyGroovers()
        {
            Globals._graphics = new GraphicsDeviceManager(this);

            //Not necessary for now but is going to be used later to have the game in full screen
            /*
            Globals._graphics.IsFullScreen = true;
            Globals._graphics.PreferredBackBufferWidth = 1920;
            Globals._graphics.PreferredBackBufferHeight = 1080;
            */

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

            Texture2D dotSprite = Content.Load<Texture2D>("dotSprite");
            _gameManager.getMouseManager().setDotSprite(dotSprite);
            _gameManager.dotTexture = dotSprite;
            _gameManager.playerBlob.SetTexture(dotSprite);

            Texture2D squareSprite = Content.Load<Texture2D>("squareSprite");
            _gameManager.squareTexture = squareSprite;
        }

        public string GetPlayerInitialsFromUser()
        {
            
            KeyboardState keyboardState = Keyboard.GetState();

            Keys[] pressedKeys = keyboardState.GetPressedKeys();                                           //We get the pressed keys per frame 
            
            if (pressedKeys.Length > 0 && !isKeyPressed)                                                   //We verify is the key is being press
            {
                Keys firstKey = pressedKeys[0];                                                            //We get the first pressed key 

                if (char.IsLetter((char)firstKey) && initials.Length <= 13)                                  //We verify is the key is a letter 
                {
                    initials += ((char)firstKey).ToString().ToLower();                                     //We make them uppercase  
                    isKeyPressed = true;
                }
                else if (firstKey == Keys.Back && initials.Length > 0)                                     //If backspace is pressed
                {
                    initials = initials.Substring(0, initials.Length - 1);                                 //if it is pressed we eliminate one character 
                    isKeyPressed = true;
                }
                else if (firstKey == Keys.Space && initials.Length > 0)                                      //We allow the user to press space 
                {
                    initials += " ";
                    isKeyPressed = true;
                }
                else if (gameState == GameState.LoginScreen && firstKey == Keys.Enter && initials.Length >=3)    //When the player press enter we set a random colour 
                {
                    int randomIndex = random.Next(availableColors.Count);
                    Color randomColor = availableColors[randomIndex];
                    _gameManager.playerBlob.SetUserColor(randomColor);
                    assignedColors.Add(randomColor);
                    availableColors.RemoveAt(randomIndex);

                    isKeyPressed = true;
                    return "EnterPressed";                                                                 // Return a specific string to indicate Enter key is pressed
                }
            }
            else if (pressedKeys.Length == 0)                                                              //if there's nothing written, we understand that ther's has not been a key pressed 
            {
                isKeyPressed = false;
            }

            return initials;                                                                               //We return the player's  initilas 
        }
        public int GetInitialsLenght()                                                                     //We create a method to get the players initials lenght
        { 
            return initials.Length;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            string inputResult = GetPlayerInitialsFromUser();
            if (inputResult == "EnterPressed")
            {
                gameState = GameState.RaceScreen;  // Change game state to RaceScreen
            }

            _gameManager.Update(gameTime);
            // We do not execute network operations in this main thread, but in a task.
            // https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.run?view=net-8.0
            // Task.Run(() => _gameManager.HandleNetworkCommunication());

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        { 
            // Add your drawing code here
            Globals._spriteBatch.Begin();
            switch (gameState)
            {
                case GameState.LoginScreen:
                    LogIn();
                    break;
                case GameState.RaceScreen:
                    race(gameTime);
                    break;
            }
            Globals._spriteBatch.End();

            base.Draw(gameTime);
        }

        public void race(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateBlue);
            // Draw game objects using _spriteBatch
            _gameManager.Draw(gameTime);
            _gameManager.getMouseManager().Draw();
        }
        public void LogIn()
        {
            initials = GetPlayerInitialsFromUser();
            _gameManager.playerBlob.SetUserName(initials);
            GraphicsDevice.Clear(Color.RoyalBlue);
            string message = "PLEASE, ENTER YOUR INITIALS BELOW: ";

            // Calculate the position to center the message horizontally and vertically
            Vector2 messageSize = Globals._gameFont.MeasureString(message);
            Vector2 position = new Vector2((GraphicsDevice.Viewport.Width - messageSize.X) / 2 - 25,(GraphicsDevice.Viewport.Height - messageSize.Y) / 2 - 30);

            Vector2 initialsSize = Globals._gameFont.MeasureString(initials);
            Vector2 initialsPosition = new Vector2((GraphicsDevice.Viewport.Width - initialsSize.X - 30) / 2, 250);                                                  // Fixed vertical position for initials

            Globals._spriteBatch.DrawString(Globals._gameFont, message, position, Color.White,0f, Vector2.Zero, 1.2f, SpriteEffects.None, 0f);
            Globals._spriteBatch.DrawString(Globals._gameFont, initials, initialsPosition, Color.Black,0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0f);
        }
    }
}