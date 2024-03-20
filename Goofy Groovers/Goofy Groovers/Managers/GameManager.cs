using Microsoft.Xna.Framework;
using System;
using System.Windows;
using Goofy_Groovers;
using Goofy_Groovers.Util;
using PlatformGame.GameClient;
using System.Linq;
using System.Diagnostics;

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
            _mouseManager = new MouseManager();
            blobEntities = new BlobEntity[1];
            blobEntities[0] = new BlobEntity(new Vector2(192, 192), true);
            playerBlob = blobEntities[0];

            map = new Vector2[4];
            map.SetValue(new Vector2(128, 128), 0);
            map.SetValue(new Vector2(128, 256), 1);
            map.SetValue(new Vector2(256, 256), 2);
            map.SetValue(new Vector2(256, 128), 3);
        }

        public void Update(GameTime gameTime)
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
                blob.Update(gameTime);
            }
        }

        public bool FindIntersection(Vector2[] map, BlobEntity blob, float theta, float velocity)
        {
            double timeDelta = 0.5;
            Vector2 position = blob.GetPosition(), positionOld, intervalStartPoint, intervalEndPoint = Vector2.Zero, intersection = Vector2.Zero;
            for (double time = timeDelta; time <= 1000; time += timeDelta)
            {
                positionOld = position;
                position = new Vector2(
                velocity * (float)(Math.Cos(blob.GetTheta()) * time),
                velocity * (float)(Math.Sin(blob.GetTheta()) * time) - 0.5f * 9.8f * (float)Math.Pow(time, 2));

                for (int iterator = 0; iterator < map.GetLength(0); iterator++)
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
                    if (LineUtil.IntersectLineSegments2D(positionOld, position, intervalStartPoint, intervalEndPoint, out intersection));
                    { // TODO: Fix, constantly indetifies (0,0) as an intersection point.
                        blob.SetJumpEndPoint(intersection);
                        _mouseManager.EndNewJumpAttempt();
                        break;
                    }

                }

            } // Adjust the step size as needed

            // Sanity~Debug check
            Debug.WriteLine("EndPoint: " + blob.GetEndpoint().ToString());
            Debug.WriteLine("Blob: " + blob.GetPosition().ToString());
            Debug.WriteLine("Angle: " + theta.ToString());
            Debug.WriteLine("Velocity: " + velocity.ToString());
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