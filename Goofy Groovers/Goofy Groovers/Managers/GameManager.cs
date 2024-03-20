using Microsoft.Xna.Framework;
using System;
using Goofy_Groovers;
using PlatformGame.GameClient;

namespace PlatformGame.Managers
{
    public class GameManager
    {
        private MouseManager _mouseManager;
        private BlobEntity[] blobEntities;
        private BlobEntity playerBlob;
        private Vector2[] map;

        public GameManager(GoofyGroovers game)
        {
            blobEntities = new BlobEntity[1];
            blobEntities[0] = new BlobEntity(new Vector2(50, 50), true);
            playerBlob = blobEntities[0];
        }

        public void Update(float elapsedSeconds)
        {
            _mouseManager.Update(elapsedSeconds);
            if (_mouseManager.newJumpInitiated())
            {
                // Calculate the force of the jump, pass it to the blob
                // playerBlob.jumpTheta = _mouseManager.GetTheta(); //or smth
                // Calculate the new intersection point, pass it to the blob

                FindIntersection(map, playerBlob, playerBlob.GetTheta(), playerBlob.GetVelocity());
            }
            foreach (var blob in blobEntities)
            {
                blob.Update(elapsedSeconds);
            }
        }

        public bool FindIntersection(Vector2[] map, BlobEntity blob, float theta, float velocity)
        {
            double timeDelta = 0.1;
            double x, y, distance;
            for (double time = 0; time <= 1000; time += timeDelta)
            {
            } // Adjust the step size as needed
            return false;
        }

        public void Draw()
        {
            throw new NotImplementedException();
        }

        public void HandleNetworkCommunication()
        {
            GreetingClient.RunClient();  //We call the RunClient method
        }
    }
}