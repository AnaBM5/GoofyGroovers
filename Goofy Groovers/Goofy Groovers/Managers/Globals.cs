using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Goofy_Groovers.Managers
{
    static class Globals
    {
        public static ushort windowWidth;
        public static ushort windowHeight;

        public static GraphicsDeviceManager _graphics;
        public static SpriteBatch _spriteBatch;
        public static SpriteFont _gameFont;
        public static GameClient _gameClient;
        public static GameManager _gameManager;
        public static Texture2D _dotTexture;
    }
}