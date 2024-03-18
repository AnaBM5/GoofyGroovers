using Goofy_Groovers;
using PlatformGame.GameClient;

namespace PlatformGame.Managers
{
    public class GameManager
    {
        private MouseManager mouseManager;

        public GameManager(GoofyGroovers game)
        {
        }

        public void Update()
        {
            mouseManager.update();
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