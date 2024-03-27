using Goofy_Groovers.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PlatformGame.Managers;

namespace Goofy_Groovers
{
    public class GoofyGroovers : Game
    {
        private GameManager _gameManager;

        public GoofyGroovers()
        {
            Globals._graphics = new GraphicsDeviceManager(this);

            //Not necessary for now but is going to be used later to have the game in full screen

            /*
            Globals._graphics.IsFullScreen = true;
            Globals._graphics.PreferredBackBufferWidth = 1920;
            Globals._graphics.PreferredBackBufferHeight = 1080;
            */

            Globals.windowWidth = (ushort)Globals._graphics.PreferredBackBufferWidth;
            Globals.windowHeight = (ushort) Globals._graphics.PreferredBackBufferHeight;

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

            _gameManager.getLevelManager().setPlatformSprite(squareSprite);
            _gameManager.getLevelManager().setBackgroundSprite(squareSprite);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _gameManager.Update(gameTime);
            // We do not execute network operations in this main thread, but in a task.
            // https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.run?view=net-8.0
            // Task.Run(() => _gameManager.HandleNetworkCommunication());

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateBlue);

            // Add your drawing code here
            Globals._spriteBatch.Begin();
            // Draw game objects using _spriteBatch
            _gameManager.Draw(gameTime);
            _gameManager.getMouseManager().Draw();
            Globals._spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}