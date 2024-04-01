using Goofy_Groovers;
using Goofy_Groovers.Managers;
using Goofy_Groovers.Util;
using Microsoft.VisualBasic;
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
        private TileMapManager _levelManager;
        private BlobEntity[] blobEntities;
        private List<Vector2> map;
        private List<Vector2[]> obstacles;

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

            _levelManager = new TileMapManager();

            // TODO: Pop-up window?
            playerBlob.SetUserName("Player 1");
            // TODO: Random color from an array -> tell server to update said array?
            playerBlob.SetUserColor(Color.Red);

            _mouseManager = new MouseManager();

            parabolicVisualisationTimeDelta = 0.2;
            parabolicVisualisationOffset = parabolicVisualisationTimeDelta;
            parabolicVisualisationTimeMax = 20;

            map = _levelManager.getLevelOutlinePixelCoordinates();
            obstacles = _levelManager.getObstaclesPixelCoordinates();

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
                if (LineUtil.PointInPolygon(map, position) && OutsideObstacles())
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

        public void FindIntersection(List<Vector2> map, BlobEntity blob, float theta, float velocity)
        {
            parabolicMovementVisualisation.Clear();

            Vector2[] trajectoryPositions = new Vector2[2];

            position = playerBlob.GetPosition() + new Vector2(
                    -velocity * (float)(Math.Cos(theta) * parabolicVisualisationTimeDelta),
                    -velocity * (float)(Math.Sin(theta) * parabolicVisualisationTimeDelta) - 0.5f * -9.8f * (float)Math.Pow(parabolicVisualisationTimeDelta, 2));
           
            if (LineUtil.PointInPolygon(map, position) && OutsideObstacles())
            {
                trajectoryPositions = InterceptAllObstacles(theta, velocity);
                position = trajectoryPositions[0];

                playerBlob.SetJumpStartPoint(playerBlob.GetPosition());
                playerBlob.SetJumpEndPoint(trajectoryPositions[1]);
                playerBlob.SetVelocity(_mouseManager.GetVelocity());
                playerBlob.SetThetha(_mouseManager.GetTheta());
                playerBlob.DefineJumpDirection();
                playerBlob.SetSecondsSinceJumpStarted(0);
                playerBlob.SetJumpingState(true);
                _mouseManager.EndNewJumpAttempt();

            }
            // Debug.WriteLine("EndPoint: " + blob.GetEndpoint().ToString());
        }

        private Vector2[] InterceptAllObstacles(float theta, float velocity)
        {
            Vector2 positionOld, intervalStartPoint, intervalEndPoint = Vector2.Zero, intersection = Vector2.Zero;

            double lowestTime = double.MaxValue;
            double timeLimit = 20;

            Vector2 originalPosition = position;

            List<Vector2> currentSection;
            Vector2[] positionsReturned = new Vector2[2];

            for(int counter = -1; counter< obstacles.Count; counter++)
            {
                position = originalPosition;

                //first one is map, rest is obstacles
                if (counter == -1)
                    currentSection = map;
                else
                    currentSection = obstacles[counter].ToList();

                for (double time = parabolicVisualisationTimeDelta * 2; time <= timeLimit && !playerBlob.GetJumpingState(); time += parabolicVisualisationTimeDelta)
                {
                    // Debug.WriteLine("Position: " + position.ToString());
                    positionOld = position;
                    position = playerBlob.GetPosition() + new Vector2(
                        -velocity * (float)(Math.Cos(theta) * time),
                        -velocity * (float)(Math.Sin(theta) * time) - 0.5f * -9.8f * (float)Math.Pow(time, 2));
                    //parabolicMovementVisualisation.Add(position); // Remove for no "trace"

                    //TODO: add an if here that, if it is too far, just dont calculate, question is how many pixels far is "too far"
                    for (int mapBordersIterator = 0; mapBordersIterator < currentSection.Count && !playerBlob.GetJumpingState(); mapBordersIterator++)
                    {
                        if (mapBordersIterator == 0)
                        {
                            intervalStartPoint = currentSection.Last();
                        }
                        else
                        {
                            intervalStartPoint = intervalEndPoint;
                        }

                        intervalEndPoint = currentSection[mapBordersIterator];
                        if (LineUtil.IntersectLineSegments2D(positionOld, position, intervalStartPoint, intervalEndPoint, out intersection))
                        {
                            //if the player touches this line first
                            //saves position and intersection to check of the closest hit
                            if (time < lowestTime)
                            {
                                positionsReturned[0] = positionOld;
                                positionsReturned[1] = intersection;
                                lowestTime = time;
                            }
                            

                            break;
                        }
                    }
                }
            }

            return positionsReturned;
        }

        private bool OutsideObstacles()
        {
            List<Vector2> coordList;
            foreach (Vector2[] obstacle in obstacles)
            {
                coordList = obstacle.ToList();
                if(LineUtil.PointInPolygon(coordList, position))
                    return false;
            }
            return true;
        }

        public void Draw(GameTime gameTime)
        {
            //Globals._spriteBatch.Draw(squareTexture, new Rectangle((int)map[0].X, (int)map[0].Y, 128, 128), Color.LightSkyBlue);
            /*
            for (int iterator = 0; iterator < tilePositions.Count(); iterator++)
            {
                Globals._spriteBatch.Draw(squareTexture, new Rectangle((int)tilePositions.ElementAt(iterator).X, (int)tilePositions.ElementAt(iterator).Y, 128, 128), Color.PeachPuff);
            }
            */
            _levelManager.Draw();
            for (int iterator = 0; iterator < parabolicMovementVisualisation.Count(); iterator++)
            {
                Globals._spriteBatch.Draw(playerBlob.GetTexture(), new Rectangle((int)parabolicMovementVisualisation.ElementAt(iterator).X - 2, (int)parabolicMovementVisualisation.ElementAt(iterator).Y - 2, 5, 5), Color.White);
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

        public TileMapManager getLevelManager()
        {
            return _levelManager;
        }
    }
}