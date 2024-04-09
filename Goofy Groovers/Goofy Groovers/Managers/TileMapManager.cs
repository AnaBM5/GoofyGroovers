using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Goofy_Groovers.Managers
{
    public class TileMapManager
    {
        private String fileName;

        private Int16 tileSize;

        private int mapWidth;
        private int mapHeight;

        //Pixel coord to know what part of the map to show
        private int mapOffsetX;
        private int mapOffsetY;

        //Coord to determine if any individual tile is visible and thus should be drawn
        private int tileXPosition;
        private int tileYPosition;

        private Texture2D backgroundTile;
        private Texture2D platformTile;
        private Texture2D goalTile;

        private int finishLineX;

        private List<String[]> levelData;

        private List<Vector2> levelOutline;
        private List<Vector2[]> levelObstacles;

        private List<Vector2> levelOutlinePixelCoordinates;
        private List<Vector2[]> levelObstaclesPixelCoordinates;

        public TileMapManager()
        {
            fileName = "Content\\tile_map.txt";

            tileSize = 128;

            mapOffsetX = 100;
            mapOffsetY = 0;

            levelOutline = new List<Vector2>();
            levelObstacles = new List<Vector2[]>();

            levelOutlinePixelCoordinates = new List<Vector2>();
            levelObstaclesPixelCoordinates = new List<Vector2[]>();

            finishLineX = -1;

            ReadFile();
            ModifyOffset(new Vector2(0, 0));
        }

        private void ReadFile()
        {
            String path = Path.Combine(Environment.CurrentDirectory, fileName);
            Debug.WriteLine("PATH: " + path);

            //Search how can we avoid using strings for this, too expensive
            List<String[]> levelMap = new List<String[]>();

            try
            {
                if (File.Exists(path))
                {
                    var lines = File.ReadAllLines(path);                                           //We Read from text file

                    String[] line;
                    Vector2 newCoordinates;
                    Vector2[] newObstacle;

                    int lineCount = lines.Length;

                    mapWidth = int.Parse(lines.ElementAt(0));
                    mapHeight = int.Parse(lines.ElementAt(1));

                    int lineCounter;
                    //Take tile info for sprites
                    for (lineCounter = 2; lineCounter < lineCount; lineCounter++)
                    {
                        Debug.WriteLine(lineCounter);
                        if (lineCounter < mapHeight + 2)
                        {
                            line = lines.ElementAt(lineCounter).Split(' ');

                            if (line.Length == mapWidth)
                                levelMap.Add(line);
                            else
                                throw new Exception("Map line " + lineCounter + " doesn't have right width");

                            if (finishLineX == -1 &&  line.Contains<string>("2"))
                                DefineFinishLineCoordinate(line);
                        }
                        else if (lineCounter == mapHeight + 2)
                        {
                            line = lines.ElementAt(lineCounter).Split('|');

                            //split each coord into 2
                            foreach (string coordinate in line)
                            {
                                newCoordinates.X = int.Parse(coordinate.Split(",").ElementAt(0));
                                newCoordinates.Y = int.Parse(coordinate.Split(",").ElementAt(1));

                                levelOutline.Add(newCoordinates);
                            }
                        }
                        else
                        {
                            line = lines.ElementAt(lineCounter).Split('|');

                            newObstacle = new Vector2[line.Length];

                            int coordinateCounter;
                            int lineLength = line.Length;
                            //split each coord into 2
                            for (coordinateCounter = 0; coordinateCounter < lineLength; coordinateCounter++)
                            {
                                newCoordinates = new Vector2();

                                newCoordinates.X = int.Parse(line.ElementAt(coordinateCounter).Split(",").ElementAt(0));
                                newCoordinates.Y = int.Parse(line.ElementAt(coordinateCounter).Split(",").ElementAt(1));

                                newObstacle[coordinateCounter] = newCoordinates;
                            }

                            levelObstacles.Add(newObstacle);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading level");
            }

            levelData = levelMap;
        }

        private void DetectMapLimits()
        {
            //Convert the tile positions into pixel coordinates for where the obstacle is on the screen

            Vector2 pixelCoordinates;
            Vector2[] obstacleCoordinates;

            int coordinateCounter, obstacleCounter;
            int levelObstaclesAmount, lineLength;

            levelOutlinePixelCoordinates.Clear();
            levelObstaclesPixelCoordinates.Clear();

            foreach (Vector2 coordinate in levelOutline)
            {
                pixelCoordinates = new Vector2(coordinate.X * tileSize - mapOffsetX, coordinate.Y * tileSize - mapOffsetY);
                levelOutlinePixelCoordinates.Add(pixelCoordinates);
            }
            levelObstaclesAmount = levelObstacles.Count;

            for (obstacleCounter = 0; obstacleCounter < levelObstaclesAmount; obstacleCounter++)
            {
                lineLength = levelObstacles[obstacleCounter].Length;
                obstacleCoordinates = new Vector2[lineLength];

                for (coordinateCounter = 0; coordinateCounter < lineLength; coordinateCounter++)
                {
                    pixelCoordinates = new Vector2(levelObstacles[obstacleCounter].ElementAt(coordinateCounter).X * tileSize - mapOffsetX,
                                                    levelObstacles[obstacleCounter].ElementAt(coordinateCounter).Y * tileSize - mapOffsetY);
                    obstacleCoordinates[coordinateCounter] = pixelCoordinates;
                }
                levelObstaclesPixelCoordinates.Add(obstacleCoordinates);
            }
        }

        public Vector2 ModifyOffset(Vector2 playerWorldPosition)
        {
            //Depending on distance from camera is offset velocity/value

            Vector2 playerScreenPosition = new Vector2(playerWorldPosition.X - mapOffsetX, playerWorldPosition.Y - mapOffsetY);

            int offsetMultiplier = 1;
            int offsetCorrection = 0;
            int pixelRange = 20; //after how many pixels does the speed increases
            //Depending on the player's position on the screen, either adds or subtracts from the x and y offsets
            //Called every player update

            float playerCameraDistance;

            if (playerScreenPosition.X < Globals.windowWidth / 3 && mapOffsetX > 0)
            {
                playerCameraDistance = Globals.windowWidth / 3 - playerScreenPosition.X;
                offsetMultiplier = 1 + (int)(playerCameraDistance / pixelRange);

                if (mapOffsetX - offsetMultiplier < 0)
                    offsetCorrection = offsetMultiplier - mapOffsetX;

                mapOffsetX -= offsetMultiplier - offsetCorrection;
                playerScreenPosition.X += offsetMultiplier - offsetCorrection;
                finishLineX += offsetMultiplier - offsetCorrection;
            }
            else if (playerScreenPosition.X > Globals.windowWidth * 2 / 3 && mapOffsetX < mapWidth * tileSize - Globals.windowWidth) //not sure if should be mapWidth*64 -1 or -64 (maybe - screen length?)
            {
                playerCameraDistance = playerScreenPosition.X - (Globals.windowWidth * 2 / 3);
                offsetMultiplier = 1 + (int)(playerCameraDistance / pixelRange);

                if (mapOffsetX + offsetMultiplier > mapWidth * tileSize - Globals.windowWidth)
                    offsetCorrection = (offsetMultiplier + mapOffsetX) - (mapWidth * tileSize - Globals.windowWidth);

                mapOffsetX += offsetMultiplier - offsetCorrection;
                playerScreenPosition.X -= offsetMultiplier - offsetCorrection;
                finishLineX -= offsetMultiplier - offsetCorrection;
            }

             offsetCorrection = 0;

            if (playerScreenPosition.Y < Globals.windowHeight / 3 && mapOffsetY > 0)
            {
                playerCameraDistance = Globals.windowHeight / 3 - playerScreenPosition.Y;
                offsetMultiplier = 1 + (int)(playerCameraDistance / pixelRange);

                if (mapOffsetY - offsetMultiplier < 0)
                    offsetCorrection = offsetMultiplier - mapOffsetY;

                mapOffsetY -= offsetMultiplier - offsetCorrection;
                playerScreenPosition.Y += offsetMultiplier - offsetCorrection;
            }
            else if (playerScreenPosition.Y > Globals.windowHeight * 2 / 3 && mapOffsetY < mapHeight * tileSize - Globals.windowHeight)
            {
                playerCameraDistance = playerScreenPosition.Y - (Globals.windowHeight * 2 / 3);
                offsetMultiplier = 1 + (int)(playerCameraDistance / pixelRange);

                if (mapOffsetY + offsetMultiplier > mapHeight * tileSize - Globals.windowHeight)
                    offsetCorrection = (offsetMultiplier + mapOffsetY) - (mapHeight * tileSize - Globals.windowHeight);


                mapOffsetY += offsetMultiplier - offsetCorrection ;
                playerScreenPosition.Y -= offsetMultiplier - offsetCorrection;
            }

            DetectMapLimits();

            //Return player screen position?
            return playerScreenPosition;
        }


        private void DefineFinishLineCoordinate(String[] line)
        {
            int lineLength = line.Length;
            for (int counter = 0; counter < lineLength; counter++)
            {
                if (line[counter].Equals("2"))
                {
                    finishLineX = counter * tileSize - mapOffsetX;
                    return;
                }
            }
        }

        public void Draw()
        {
            //goes through the map and, if the tile is within visible parameters taking into account the offset, draws it
            int xCounter, yCounter;

            int xPosition, yPosition;

            String currentTile;

            for (yCounter = 0; yCounter < mapHeight; yCounter++)
            {
                for (xCounter = 0; xCounter < mapWidth; xCounter++)
                {
                    currentTile = levelData.ElementAt(yCounter).ElementAt(xCounter);

                    xPosition = xCounter * tileSize - mapOffsetX;
                    yPosition = yCounter * tileSize - mapOffsetY;

                    if (yPosition > -tileSize && yPosition < Globals.windowHeight
                        && xPosition > -tileSize && xPosition < Globals.windowWidth)
                    {
                        //Debug.WriteLine("Show tile in position" + xCounter + " , " + yCounter);
                        switch (currentTile)
                        {
                            case "0":
                                Globals._spriteBatch.Draw(backgroundTile, new Rectangle(xPosition, yPosition, tileSize, tileSize), Color.White); //Color.PeachPuff
                                break;

                            case "1":
                                Globals._spriteBatch.Draw(platformTile, new Rectangle(xPosition, yPosition, tileSize, tileSize), Color.White); //Color.BlueViolet
                                break;

                            case "2":
                                Globals._spriteBatch.Draw(goalTile, new Rectangle(xPosition, yPosition, tileSize, tileSize), Color.LightSeaGreen);
                                break;

                            default:
                                Globals._spriteBatch.Draw(platformTile, new Rectangle(xPosition, yPosition, tileSize, tileSize), Color.White);
                                break;
                        }
                    }
                    //else
                    //Debug.WriteLine("DON'T show tile in position" + xCounter + " , " + yCounter);
                }
            }
        }

        public Vector2 GetWorldPosition(Vector2 cameraPosition)
        {
            Vector2 worldPosition = new Vector2(cameraPosition.X + mapOffsetX, cameraPosition.Y + mapOffsetY);
            return worldPosition;
        }

        public Vector2 GetCameraPosition(Vector2 worldPosition)
        {
            Vector2 cameraPosition = new Vector2(worldPosition.X - mapOffsetX, worldPosition.Y - mapOffsetY);

            return cameraPosition;
        }


        public void setBackgroundSprite(Texture2D backgroundTile)
        {
            this.backgroundTile = backgroundTile;
        }

        public void setPlatformSprite(Texture2D platformTile)
        {
            this.platformTile = platformTile;
        }

        public void setGoalSprite(Texture2D goalTile)
        {
            this.goalTile = goalTile;
        }

        public List<Vector2> getLevelOutlinePixelCoordinates()
        {
            return levelOutlinePixelCoordinates;
        }

        public List<Vector2[]> getObstaclesPixelCoordinates()
        {
            return levelObstaclesPixelCoordinates;
        }

        public int getFinishLineXCoordinate()
        {
            return this.finishLineX;
        }
    }
}