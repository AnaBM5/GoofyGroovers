using Goofy_Groovers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlatformGame.GameClient;

namespace PlatformGame.Managers
{
    public class GameManager
    {

        public GameManager(GoofyGroovers game)
        {
             
        }
        public void Update()
        {

          

        }

        public void Draw()
        {
 

        }

        public void HandleNetworkCommunication()
        {
            
            GreetingClient.RunClient();  //We call the RunClient method
        }

    }
}
