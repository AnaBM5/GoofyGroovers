using Goofy_Groovers;
using Goofy_Groovers.Entity;
using Goofy_Groovers.Managers;
using Goofy_Groovers.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using static Goofy_Groovers.GoofyGroovers;
using static System.Reflection.Metadata.BlobBuilder;

namespace Goofy_Groovers.Managers
{
    public class GameManager
    {
        private MouseManager _mouseManager;
        private TileMapManager _levelManager;

        public readonly Object toKeepEntitiesIntact = new Object();
        public List<BlobEntity> blobEntities;
        private Thread serverTransmitterThread;

        private List<Vector2> map;
        private List<Vector2[]> obstacles;

        public BlobEntity playerBlob;

        //Variables for controlling the timing of the start and end of the race
        private bool countdownStarted;
        private float countdownTimer;
        private byte countdownMessages;
        private bool raceStarted;
        private bool showEndScreen;
        private float endScreenTimer; //Time that the game waits after the player finished the race to show the leaderboard
        private float overlayTransparency;

        //Variables for position control and collision detection
        private Vector2 position;
        private Vector2 lastValidPosition;
        private List<Vector2> parabolicMovementVisualisation;
        private double intersectionTime;

        private double parabolicVisualisationTimeDelta;
        private double parabolicVisualisationTimeMax;
        private double parabolicVisualisationOffset;
        private double elapsedTimeSeconds;

        //Sprite textures
        public Texture2D dotTexture;
        public Texture2D squareTexture;
        public Texture2D overlayScreen;
        public  SpriteFont countdownFont;

        private double elapsedSecondsSinceVisualisationShift;
        private double elapsedSecondsSinceTransmissionToServer;
        private bool hasFinishedTransmission = true;

        public GameManager(GoofyGroovers game)
        {
            _mouseManager = new MouseManager();
            _levelManager = new TileMapManager();

            blobEntities = new List<BlobEntity>();
            blobEntities.Add(new BlobEntity(new Vector2(150, 440), _levelManager.GetCameraPosition(new Vector2(450, 440)), true));
            playerBlob = blobEntities.ElementAt(0);

            playerBlob.SetUserName("Player 1");
            playerBlob.SetUserColor(Color.Blue);

            lastValidPosition = playerBlob.GetWorldPosition();

            /*
            parabolicVisualisationTimeDelta = 0.2;
            parabolicVisualisationTimeMax = 20;
            */
            //increased for full screen
            parabolicVisualisationTimeDelta = 0.27;
            parabolicVisualisationTimeMax = 40;
            

            parabolicVisualisationOffset = parabolicVisualisationTimeDelta;

            map = _levelManager.getLevelOutlinePixelCoordinates();
            obstacles = _levelManager.getObstaclesPixelCoordinates();

            parabolicMovementVisualisation = new List<Vector2>();

            endScreenTimer = 0.5f;
            overlayTransparency = 0f;
            countdownMessages = 3;
            showEndScreen = false;
            raceStarted = false;
        }

        public void Update(GameTime elapsedSeconds, GoofyGroovers game)
        {
            elapsedTimeSeconds = elapsedSeconds.ElapsedGameTime.TotalSeconds;

            KeyboardState state = Keyboard.GetState();

            // If they hit esc, exit
            if (state.IsKeyDown(Keys.S))
            {
                countdownStarted = true;
                return;
            }
            
            
            if(!raceStarted)
            {
                _levelManager.ModifyOffset(playerBlob.GetWorldPosition());
                {
                    lock (Globals._gameManager.toKeepEntitiesIntact)
                        foreach (var blob in blobEntities)
                        {
                            blob.SetCameraPosition(_levelManager.GetCameraPosition(blob.GetWorldPosition()));
                            blob.Update(elapsedSeconds);
                        }
                }

                if(countdownStarted)
                {
                    if(countdownTimer < 0.8f)
                    {
                        countdownTimer += (float) elapsedTimeSeconds;
                        //modify transparency and position

                        if (countdownTimer > 0.8f)
                        {
                            countdownTimer = 0f;
                            countdownMessages--;
                            if(countdownMessages > 3)
                            {
                                countdownStarted = false;
                                raceStarted = true;
                            }
                                
                        }
                        
                    }
                }
            }
                
            else
            {
                
                elapsedSecondsSinceVisualisationShift += elapsedTimeSeconds;
                elapsedSecondsSinceTransmissionToServer += elapsedTimeSeconds;
                _mouseManager.Update(game);

                if (!playerBlob.GetJumpingState() && !playerBlob.finishedRace) //if the player has crossed the finish line, it can't move anymore
                {
                    if (_mouseManager.IsJumpCancelled())
                        parabolicMovementVisualisation.Clear();

                else if (_mouseManager.IsNewJumpInitiated())
                {
                    VisualizeTrajectory(map, playerBlob, _mouseManager.GetTheta(), _mouseManager.GetVelocity());
                }
                else if (_mouseManager.IsNewJumpAttempted())
                {
                    // Calculate the force of the jump, pass it to the blob
                    // Calculate the new intersection point FIRST, pass it to the blob
                    // playerBlob.jumpTheta = _mouseManager.GetTheta(); //or smth

                    parabolicMovementVisualisation.Clear();
                    VerifyIntersenction(playerBlob, _mouseManager.GetTheta(), _mouseManager.GetVelocity());
                }
            }



            _levelManager.ModifyOffset(playerBlob.GetWorldPosition());

                {
                    lock (Globals._gameManager.toKeepEntitiesIntact)
                        foreach (var blob in blobEntities)
                        {
                            blob.SetCameraPosition(_levelManager.GetCameraPosition(blob.GetWorldPosition()));
                            blob.Update(elapsedSeconds);
                        }
                }

                if (!playerBlob.finishedRace && playerBlob.GetCameraPosition().X >= _levelManager.getFinishLineXCoordinate())
                {
                    playerBlob.finishedRace = true;
                    //change colour to know when the condition its true
                    playerBlob.blobUserColor = Color.White;
                    //TODO: send message to get leaderboard position

                }

                //After the player crosses the finish line, waits for a determined amount of time before showing the end screen
                if (playerBlob.finishedRace && !showEndScreen)
                {
                    endScreenTimer -= (float)elapsedTimeSeconds;
                    overlayTransparency += (float)elapsedTimeSeconds * 1.2f;

                    if (endScreenTimer < 0)
                        showEndScreen = true;

                    if (overlayTransparency > 0.8f)
                        overlayTransparency = 0.8f;

                }


                //Adjust player pos if out of bounds?
                //I dont like how its working tbh, still gets stuck sometimes
                if (!LineUtil.PointInPolygon(map, playerBlob.GetCameraPosition()) || !OutsideObstacles(playerBlob.GetCameraPosition()))
                {
                    playerBlob.worldPosition = lastValidPosition;
                    playerBlob.SetCameraPosition(_levelManager.GetCameraPosition(playerBlob.worldPosition));
                }


           
            if (elapsedSecondsSinceTransmissionToServer > 0.16)
            {
                elapsedSecondsSinceTransmissionToServer = 0;
                _ = Task.Run(() => Globals._gameClient.ConnectAndCommunicate(gameState));
            }
             /*
            if (elapsedSecondsSinceVisualisationShift > 1)
            {
                elapsedSecondsSinceVisualisationShift = 0;
                parabolicVisualisationOffset += 0.01;
            }*/
        }

        private void VisualizeTrajectory(List<Vector2> map, BlobEntity playerBlob, float theta, float velocity)
        {
            intersectionTime = 0;
            parabolicMovementVisualisation.Clear();

            for (double time = 0; time <= parabolicVisualisationTimeMax; time += parabolicVisualisationTimeDelta)
            {
                position = playerBlob.GetCameraPosition() + new Vector2(
                    -velocity * (float)(Math.Cos(theta) * (time + parabolicVisualisationOffset)),
                    -velocity * (float)(Math.Sin(theta) * (time + parabolicVisualisationOffset)) - 0.5f * -9.8f * (float)Math.Pow((time + parabolicVisualisationOffset), 2));
                if (LineUtil.PointInPolygon(map, position) && OutsideObstacles(position))
                {
                    parabolicMovementVisualisation.Add(position);
                }
                else
                {
                    intersectionTime = time - parabolicVisualisationTimeDelta;

                    playerBlob.SetJumpEndPoint(_levelManager.GetWorldPosition(position));
                    playerBlob.DefineJumpDirection();

                    time = parabolicVisualisationTimeMax;
                }
                if (parabolicVisualisationOffset >= parabolicVisualisationTimeDelta)
                {
                    parabolicVisualisationOffset = 0;
                }
            }
        }

        public void VerifyIntersenction(BlobEntity playerBlob, float theta, float velocity)
        {

            // depending on the direction of the jump, adds or subtracts half the player size to the position before checking if its inside bounds

            bool[] jumpDirection = playerBlob.jumpDirection;
            Vector2 blobArea = new Vector2(playerBlob.blobRadius, playerBlob.blobRadius);

            /*
            if (jumpDirection[0])
                blobArea.X = -12;
            else
                blobArea.X = 12;

            if(jumpDirection[1])
                blobArea.Y = -12;
            else
                blobArea.Y = 12;

            */

            if (intersectionTime <= 0.2)
                playerBlob.SetJumpEndPoint(playerBlob.GetWorldPosition());
            else
            {
                //resimulates the last half second and the consequent one
                //to confirm where the blob should stick to

                for (double time = intersectionTime - 0.8; time <= intersectionTime + 0.2; time += 0.02)
                {
                    position = playerBlob.GetCameraPosition() + new Vector2(
                        -velocity * (float)(Math.Cos(theta) * (time)),
                        -velocity * (float)(Math.Sin(theta) * (time)) - 0.5f * -9.8f * (float)Math.Pow((time), 2));
                    if (LineUtil.PointInPolygon(map, position + blobArea) && OutsideObstacles(position + blobArea)
                        && LineUtil.PointInPolygon(map, position - blobArea) && OutsideObstacles(position - blobArea))
                    {
                        lastValidPosition = _levelManager.GetWorldPosition(position);
                        playerBlob.SetJumpEndPoint(_levelManager.GetWorldPosition(position));
                    }
                    else
                        break;
                }
            }

            playerBlob.SetJumpStartPoint(playerBlob.GetWorldPosition());
            playerBlob.SetVelocity(_mouseManager.GetVelocity());
            playerBlob.SetThetha(_mouseManager.GetTheta());
            playerBlob.DefineJumpDirection();
            playerBlob.SetSecondsSinceJumpStarted(0);
            playerBlob.SetJumpingState(true);
            _mouseManager.EndNewJumpAttempt();
        }

        private bool OutsideObstacles(Vector2 position)
        {
            List<Vector2> coordList;
            foreach (Vector2[] obstacle in obstacles)
            {
                coordList = obstacle.ToList();
                if (LineUtil.PointInPolygon(coordList, position))
                    return false;
            }
            return true;
        }

        private void StartRace()
        {

        }

        public void Draw(GameTime gameTime)
        {
            _levelManager.Draw();
            for (int iterator = 0; iterator < parabolicMovementVisualisation.Count(); iterator++)
            {
                Globals._spriteBatch.Draw(Globals._dotTexture, new Rectangle((int)parabolicMovementVisualisation.ElementAt(iterator).X - 2, (int)parabolicMovementVisualisation.ElementAt(iterator).Y - 2, 5, 5), Color.White);
            }

           // Vector2 endPointCameraPos = _levelManager.GetCameraPosition(playerBlob.GetEndpoint());
            //Globals._spriteBatch.Draw(Globals._dotTexture, new Rectangle((int)endPointCameraPos.X - 12, (int)endPointCameraPos.Y - 12, 25, 25), Color.BlueViolet);

            foreach (var blob in blobEntities)
            {
                blob.Draw(gameTime);
            }

            if(countdownStarted)
            {
                if (countdownMessages > 0)
                    Globals._spriteBatch.DrawString(countdownFont, countdownMessages.ToString(), new Vector2(Globals.windowWidth / 2 - 50, Globals.windowHeight / 2 - 50), Color.Yellow);
               
                else
                    Globals._spriteBatch.DrawString(countdownFont, "GO!!!", new Vector2(Globals.windowWidth / 2 - 170, Globals.windowHeight / 2 - 50), Color.Yellow);
            }

            if(showEndScreen)
                Globals._spriteBatch.Draw(overlayScreen, new Rectangle(0, 0, Globals.windowWidth, Globals.windowHeight), Color.Black * overlayTransparency);

        }

        public MouseManager getMouseManager()
        {
            return _mouseManager;
        }

        public TileMapManager getLevelManager()
        {
            return _levelManager;
        }

        public bool getShowEndScreen()
        {
            return showEndScreen;
        }
    }
}