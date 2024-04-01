using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}