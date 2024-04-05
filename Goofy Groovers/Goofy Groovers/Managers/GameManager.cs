﻿using Goofy_Groovers;
using Goofy_Groovers.Entity;
using Goofy_Groovers.Managers;
using Goofy_Groovers.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;

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

        private Vector2 position;
        private Vector2 lastValidPosition;
        private List<Vector2> parabolicMovementVisualisation;
        private double intersectionTime;

        private double parabolicVisualisationTimeDelta;
        private double parabolicVisualisationTimeMax;
        private double parabolicVisualisationOffset;

        public Texture2D dotTexture;
        public Texture2D squareTexture;
        private double elapsedSecondsSinceVisualisationShift;
        private double elapsedSecondsSinceTransmissionToServer;
        private bool hasFinishedTransmission = true;

        public GameManager(GoofyGroovers game)
        {
            _mouseManager = new MouseManager();
            _levelManager = new TileMapManager();

            blobEntities = new List<BlobEntity>();
            blobEntities.Add(new BlobEntity(new Vector2(192, 192), _levelManager.GetCameraPosition(new Vector2(192, 192)), true));
            playerBlob = blobEntities.ElementAt(0);

            playerBlob.SetUserName("Player 1");
            playerBlob.SetUserColor(Color.Blue);

            lastValidPosition = playerBlob.GetWorldPosition();

            parabolicVisualisationTimeDelta = 0.2;
            parabolicVisualisationOffset = parabolicVisualisationTimeDelta;
            parabolicVisualisationTimeMax = 20;

            map = _levelManager.getLevelOutlinePixelCoordinates();
            obstacles = _levelManager.getObstaclesPixelCoordinates();

            parabolicMovementVisualisation = new List<Vector2>();
        }

        public void Update(GameTime elapsedSeconds, GoofyGroovers game)
        {
            elapsedSecondsSinceVisualisationShift += elapsedSeconds.ElapsedGameTime.TotalSeconds;
            elapsedSecondsSinceTransmissionToServer += elapsedSeconds.ElapsedGameTime.TotalSeconds;
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

            {
            lock (Globals._gameManager.toKeepEntitiesIntact)
                foreach (var blob in blobEntities)
                {
                    blob.SetCameraPosition(_levelManager.GetCameraPosition(blob.GetWorldPosition()));
                    blob.Update(elapsedSeconds);

                    if (!blob.finishedRace && blob.GetCameraPosition().X >= _levelManager.getFinishLineXCoordinate())
                    {
                        blob.finishedRace = true;
                        //change colour to know when the condition its true
                        blob.blobUserColor = Color.White;
                        //TODO: send message to get leaderboard position

                    }

                }
            }


            //Adjust player pos if out of bounds?
            //I dont like how its working tbh, still gets stuck sometimes
            if (!LineUtil.PointInPolygon(map, playerBlob.GetCameraPosition()) || !OutsideObstacles(playerBlob.GetCameraPosition()))
            {
                playerBlob.worldPosition = lastValidPosition;
                playerBlob.SetCameraPosition(_levelManager.GetCameraPosition(playerBlob.worldPosition));
            }


            /*
            if (elapsedSecondsSinceTransmissionToServer > 0.01)
            {
                elapsedSecondsSinceTransmissionToServer = 0;
                try
                {
                    //Debug.WriteLine(hasFinishedTransmission.ToString());
                    // We do not execute network operations in this thread, but in a task.
                    // https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.run?view=net-8.0

                    Task.Run(() => { GameClient.TransmitToServer(playerBlob, blobEntities); });
                    /*                    serverTransmitterThread = new Thread(
                                            () => GameClient.TransmitToServer(playerBlob, blobEntities));
                                        serverTransmitterThread.Start();
                    Task.Run(() =>
                    {
                        hasFinishedTransmission = GameClient.TransmitToServer(playerBlob, blobEntities);
                    });
                }
                catch (Exception)
                {
                    Debug.WriteLine("Oopsie");
                }
            }*/


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
            Vector2 blobArea = new Vector2();

            if (jumpDirection[0])
                blobArea.X = -12;
            else
                blobArea.X = 12;

            if(jumpDirection[1])
                blobArea.Y = -12;
            else
                blobArea.Y = 12;

            if (intersectionTime <= 0.2)
                playerBlob.SetJumpEndPoint(playerBlob.GetWorldPosition());

            else
            {
                //resimulates the last half second and the consequent one
                //to confirm where the blob should stick to

                for (double time = intersectionTime - 0.8; time <= intersectionTime; time += 0.02)
                {
                    position = playerBlob.GetCameraPosition() + new Vector2(
                        -velocity * (float)(Math.Cos(theta) * (time)),
                        -velocity * (float)(Math.Sin(theta) * (time)) - 0.5f * -9.8f * (float)Math.Pow((time), 2));
                    if (LineUtil.PointInPolygon(map, position + blobArea) && OutsideObstacles(position + blobArea))
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

        public void Draw(GameTime gameTime)
        {
            _levelManager.Draw();
            for (int iterator = 0; iterator < parabolicMovementVisualisation.Count(); iterator++)
            {
                Globals._spriteBatch.Draw(Globals._dotTexture, new Rectangle((int)parabolicMovementVisualisation.ElementAt(iterator).X - 2, (int)parabolicMovementVisualisation.ElementAt(iterator).Y - 2, 5, 5), Color.White);
            }

            Vector2 endPointCameraPos = _levelManager.GetCameraPosition(playerBlob.GetEndpoint());
            Globals._spriteBatch.Draw(Globals._dotTexture, new Rectangle((int)endPointCameraPos.X - 12, (int)endPointCameraPos.Y - 12, 25, 25), Color.BlueViolet);

            foreach (var blob in blobEntities)
            {
                blob.Draw(gameTime);
            }
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