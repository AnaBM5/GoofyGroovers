using Goofy_Groovers;
using Goofy_Groovers.Managers;
using Goofy_Groovers.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlatformGame.GameClient;
using System;
using System.Diagnostics;
using System.Linq;

namespace PlatformGame.Managers
{
    public class GameManager
    {
        private MouseManager _mouseManager;
        private BlobEntity[] blobEntities;
        private BlobEntity playerBlob;
        private Vector2[] map;
        private Vector2 position;

        public Texture2D dotTexture;
        public Texture2D squareTexture;

        public GameManager(GoofyGroovers game)
        {
            _mouseManager = new MouseManager();
            blobEntities = new BlobEntity[1];
            blobEntities[0] = new BlobEntity(new Vector2(192, 192), true);
            playerBlob = blobEntities[0];
            _mouseManager = new MouseManager();

            map = new Vector2[4];
            map.SetValue(new Vector2(128, 128), 0);
            map.SetValue(new Vector2(128, 256), 1);
            map.SetValue(new Vector2(256, 256), 2);
            map.SetValue(new Vector2(256, 128), 3);
        }

        public void Update(GameTime elapsedSeconds)
        {
            _mouseManager.Update();
            if (_mouseManager.IsNewJumpAttempted())
            {
                // Calculate the force of the jump, pass it to the blob
                // Calculate the new intersection point FIRST, pass it to the blob
                // playerBlob.jumpTheta = _mouseManager.GetTheta(); //or smth

                FindIntersection(map, playerBlob, _mouseManager.GetTheta(), _mouseManager.GetVelocity());
            }
            foreach (var blob in blobEntities)
            {
                blob.Update(elapsedSeconds);
            }
        }

        public bool FindIntersection(Vector2[] map, BlobEntity blob, float theta, float velocity)
        {
            double timeDelta = 0.2;
            Vector2 positionOld, intervalStartPoint, intervalEndPoint = Vector2.Zero, intersection = Vector2.Zero;
            position = blob.GetPosition();
            for (double time = timeDelta; time <= 10; time += timeDelta)
            {
                Debug.WriteLine("Position: " + position.ToString());
                positionOld = position;
                position = playerBlob.GetPosition() + new Vector2(
                -velocity * (float)(Math.Cos(theta) * time),
                -velocity * (float)(Math.Sin(theta) * time) + 0.5f * 9.8f * (float)Math.Pow(time, 2));
                
                for (int iterator = 0; iterator < 4; iterator++)
                {
                    if (iterator == 0)
                    {
                        intervalStartPoint = map.Last();
                    }
                    else
                    {
                        intervalStartPoint = intervalEndPoint;
                    }

                    intervalEndPoint = map[iterator];
                    if (LineUtil.IntersectLineSegments2D(positionOld, position, intervalStartPoint, intervalEndPoint, out intersection))
                    { // TODO: Fix, constantly identifies (0,0) as an intersection point.
                        blob.SetJumpEndPoint(intersection);
                        _mouseManager.EndNewJumpAttempt();
                        break;
                    }
                }
            } // Adjust the step size as needed

            // Sanity~Debug check
            Debug.WriteLine("EndPoint: " + blob.GetEndpoint().ToString());
            return false;
        }

        public void Draw()
        {
            Globals._spriteBatch.Draw(squareTexture, new Rectangle((int)map[0].X, (int)map[0].Y, 128, 128), Color.LightSkyBlue);


            Globals._spriteBatch.Draw(dotTexture, new Rectangle((int)playerBlob.GetPosition().X - 12, (int)playerBlob.GetPosition().Y - 12, 25, 25), Color.Red);
            Globals._spriteBatch.Draw(dotTexture, new Rectangle((int)playerBlob.GetEndpoint().X - 12, (int)playerBlob.GetEndpoint().Y - 12, 25, 25), Color.BlueViolet);
        }

        public void HandleNetworkCommunication()
        {
            GreetingClient.RunClient();  //We call the RunClient method
        }

        public MouseManager getMouseManager()
        {
            return _mouseManager;
        }
    }
}