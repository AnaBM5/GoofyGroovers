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
        private List<Vector2> parabolicMovementVisualisation;

        private double parabolicVisualisationTimeDelta;
        private double parabolicVisualisationTimeMax;
        private double parabolicVisualisationOffset;

        private List<Vector2> tilePositions;

        public Texture2D dotTexture;
        public Texture2D squareTexture;
        private double elapsedSecondsSinceVisualisationShift;

        public GameManager(GoofyGroovers game)
        {
            _mouseManager = new MouseManager();
            blobEntities = new BlobEntity[1];
            blobEntities[0] = new BlobEntity(new Vector2(192, 192), true);
            playerBlob = blobEntities[0];
            _mouseManager = new MouseManager();

            parabolicVisualisationTimeDelta = 0.2;
            parabolicVisualisationOffset = parabolicVisualisationTimeDelta;
            parabolicVisualisationTimeMax = 20;

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

            parabolicMovementVisualisation = new List<Vector2>();
        }

        public void Update(GameTime elapsedSeconds)
        {
            _mouseManager.Update();

            if (!playerBlob.GetJumpingState())
            {
                if (_mouseManager.IsNewJumpInitiated())
                {
                    VisualizeTrajectory(map, playerBlob, _mouseManager.GetTheta(), _mouseManager.GetVelocity());
                }

                if (_mouseManager.IsNewJumpAttempted())
                {
                    // Calculate the force of the jump, pass it to the blob
                    // Calculate the new intersection point FIRST, pass it to the blob
                    // playerBlob.jumpTheta = _mouseManager.GetTheta(); //or smth
                    FindIntersection(map, playerBlob, _mouseManager.GetTheta(), _mouseManager.GetVelocity());
                }
            }
            foreach (var blob in blobEntities)
            {
                blob.Update(elapsedSeconds);
            }

            elapsedSecondsSinceVisualisationShift += elapsedSeconds.ElapsedGameTime.TotalSeconds;
            if (elapsedSecondsSinceVisualisationShift > 0.01)
            {
                elapsedSecondsSinceVisualisationShift = 0;
                parabolicVisualisationOffset += 0.01;
            }
        }

        private void VisualizeTrajectory(List<Vector2> map, BlobEntity playerBlob, float theta, float velocity)
        {
            parabolicMovementVisualisation.Clear();
            for (double time = 0; time <= parabolicVisualisationTimeMax; time += parabolicVisualisationTimeDelta)
            {
                position = playerBlob.GetPosition() + new Vector2(
                    -velocity * (float)(Math.Cos(theta) * (time + parabolicVisualisationOffset)),
                    -velocity * (float)(Math.Sin(theta) * (time + parabolicVisualisationOffset)) - 0.5f * -9.8f * (float)Math.Pow((time + parabolicVisualisationOffset), 2));
                if (LineUtil.PointInPolygon(map, position))
                {
                    parabolicMovementVisualisation.Add(position);
                }
                else
                {
                    time = parabolicVisualisationTimeMax;
                }
                if (parabolicVisualisationOffset >= parabolicVisualisationTimeDelta)
                {
                    parabolicVisualisationOffset = 0;
                }
            }
        }

        public bool FindIntersection(List<Vector2> map, BlobEntity blob, float theta, float velocity)
        {
            parabolicMovementVisualisation.Clear();
            double timeLimit = 20;
            Vector2 positionOld, intervalStartPoint, intervalEndPoint = Vector2.Zero, intersection = Vector2.Zero;
            position = playerBlob.GetPosition() + new Vector2(
                    -velocity * (float)(Math.Cos(theta) * parabolicVisualisationTimeDelta),
                    -velocity * (float)(Math.Sin(theta) * parabolicVisualisationTimeDelta) - 0.5f * -9.8f * (float)Math.Pow(parabolicVisualisationTimeDelta, 2));
            if (LineUtil.PointInPolygon(map, position))
                for (double time = parabolicVisualisationTimeDelta * 2; time <= timeLimit && !playerBlob.GetJumpingState(); time += parabolicVisualisationTimeDelta)
                {
                    // Debug.WriteLine("Position: " + position.ToString());
                    positionOld = position;
                    position = playerBlob.GetPosition() + new Vector2(
                        -velocity * (float)(Math.Cos(theta) * time),
                        -velocity * (float)(Math.Sin(theta) * time) - 0.5f * -9.8f * (float)Math.Pow(time, 2));
                    parabolicMovementVisualisation.Add(position); // Remove for no "trace"
                    
                    // Too expensive and not yet necessary to check
                    // checkedPositions = LineUtil.CalculatePointsOnCircle(position, 12, 4);
                    // checkedPositions.Add(positionOld);

                    for (int mapBordersIterator = 0; mapBordersIterator < map.Count && !playerBlob.GetJumpingState(); mapBordersIterator++)
                    {
                        if (mapBordersIterator == 0)
                        {
                            intervalStartPoint = map.Last();
                        }
                        else
                        {
                            intervalStartPoint = intervalEndPoint;
                        }

                        intervalEndPoint = map[mapBordersIterator];
                        if (LineUtil.IntersectLineSegments2D(positionOld, position, intervalStartPoint, intervalEndPoint, out intersection))
                        {
                            playerBlob.SetJumpStartPoint(position);
                            playerBlob.SetJumpEndPoint(intersection);
                            playerBlob.SetVelocity(_mouseManager.GetVelocity());
                            playerBlob.SetThetha(_mouseManager.GetTheta());
                            playerBlob.SetJumpingState(true);
                            _mouseManager.EndNewJumpAttempt();

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

            for (int iterator = 0; iterator < parabolicMovementVisualisation.Count(); iterator++)
            {
                Globals._spriteBatch.Draw(playerBlob.GetTexture(), new Rectangle((int)parabolicMovementVisualisation.ElementAt(iterator).X - 2, (int)parabolicMovementVisualisation.ElementAt(iterator).Y - 2, 5, 5), Color.Black);
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