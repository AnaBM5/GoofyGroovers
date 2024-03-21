using Goofy_Groovers;
using Goofy_Groovers.Managers;
using Goofy_Groovers.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlatformGame.GameClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlatformGame.Managers
{
    public class GameManager
    {
        private MouseManager _mouseManager;
        private BlobEntity[] blobEntities;
        private List<Vector2> map;

        public BlobEntity playerBlob;

        private Vector2 position;
        private List<Vector2> parabolicMovement;
        private List<Vector2> tilePositions;

        public Texture2D dotTexture;
        public Texture2D squareTexture;

        public GameManager(GoofyGroovers game)
        {
            _mouseManager = new MouseManager();
            blobEntities = new BlobEntity[1];
            blobEntities[0] = new BlobEntity(new Vector2(192, 192), true);
            playerBlob = blobEntities[0];
            _mouseManager = new MouseManager();

            map = new List<Vector2>
            {
                new Vector2(128, 128),
                new Vector2(128, 384),
                new Vector2(512, 384),
                new Vector2(512, 256),
                new Vector2(640, 256),
                new Vector2(640, 128),
                new Vector2(384, 128),
                new Vector2(384, 256),
                new Vector2(256, 256),
                new Vector2(256, 128)
            };

            tilePositions = new List<Vector2>
            {
                new Vector2(128, 128),
                new Vector2(128, 256),
                new Vector2(256, 256),
                new Vector2(384, 256),
                new Vector2(384, 128),
                new Vector2(512, 128),
            };

            parabolicMovement = new List<Vector2>();
        }

        public void Update(GameTime elapsedSeconds)
        {
            _mouseManager.Update();
            if (_mouseManager.IsNewJumpAttempted())
            {
                // Calculate the force of the jump, pass it to the blob
                // Calculate the new intersection point FIRST, pass it to the blob
                // playerBlob.jumpTheta = _mouseManager.GetTheta(); //or smth
                if (!playerBlob.GetJumpingState())
                FindIntersection(map, playerBlob, _mouseManager.GetTheta(), _mouseManager.GetVelocity());
            }
            foreach (var blob in blobEntities)
            {
                blob.Update(elapsedSeconds);
            }
        }

        public bool FindIntersection(List<Vector2> map, BlobEntity blob, float theta, float velocity)
        {
            //parabolicMovement.Clear(); // DEBUG: Remove
            double timeDelta = 0.1;
            double timeLimit = 20;
            Vector2 positionOld, intervalStartPoint, intervalEndPoint = Vector2.Zero, intersection = Vector2.Zero;
            position = playerBlob.GetPosition() + new Vector2(
                    -velocity * (float)(Math.Cos(theta) * timeDelta),
                    -velocity * (float)(Math.Sin(theta) * timeDelta) - 0.5f * -9.8f * (float)Math.Pow(timeDelta, 2));
            if (LineUtil.PointInPolygon(map, position))
                for (double time = timeDelta * 2; time <= timeLimit; time += timeDelta)
                {
                    // Debug.WriteLine("Position: " + position.ToString());
                    positionOld = position;
                    position = playerBlob.GetPosition() + new Vector2(
                        -velocity * (float)(Math.Cos(theta) * time),
                        -velocity * (float)(Math.Sin(theta) * time) - 0.5f * -9.8f * (float)Math.Pow(time, 2));
                    parabolicMovement.Add(position);
                    for (int iterator = 0; iterator < map.Count; iterator++)
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
                        {
                            playerBlob.SetJumpStartPoint(position);
                            playerBlob.SetJumpEndPoint(intersection);
                            playerBlob.SetVelocity(_mouseManager.GetVelocity());
                            playerBlob.SetThetha(_mouseManager.GetTheta());
                            playerBlob.SetJumpingState(true);
                            _mouseManager.EndNewJumpAttempt();
                            timeLimit = 0;
                            break;
                        }
                    }
                } // Adjust the step size as needed

            // Debug.WriteLine("EndPoint: " + blob.GetEndpoint().ToString());
            return false;
        }

        public void Draw(GameTime gameTime)
        {
            //Globals._spriteBatch.Draw(squareTexture, new Rectangle((int)map[0].X, (int)map[0].Y, 128, 128), Color.LightSkyBlue);
            for (int iterator = 0; iterator < tilePositions.Count(); iterator++)
            {
                Globals._spriteBatch.Draw(squareTexture, new Rectangle((int)tilePositions.ElementAt(iterator).X, (int)tilePositions.ElementAt(iterator).Y, 128, 128), Color.PeachPuff);
            }

            for (int iterator = 0; iterator < parabolicMovement.Count(); iterator++)
            {
                Globals._spriteBatch.Draw(playerBlob.GetTexture(), new Rectangle((int)parabolicMovement.ElementAt(iterator).X - 2, (int)parabolicMovement.ElementAt(iterator).Y - 2, 5, 5), Color.Black);
            }
            Globals._spriteBatch.Draw(playerBlob.GetTexture(), new Rectangle((int)playerBlob.GetEndpoint().X - 12, (int)playerBlob.GetEndpoint().Y - 12, 25, 25), Color.BlueViolet);
            playerBlob.Draw(gameTime);
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