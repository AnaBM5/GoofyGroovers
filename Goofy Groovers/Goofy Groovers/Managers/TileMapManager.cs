using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Xna.Framework.Graphics;
using System.Numerics;
using Microsoft.Xna.Framework;
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

        private List<String[]> levelData;

        private List<Vector2> levelOutline;
        private List<Vector2[]> levelObstacles;

        private List<Vector2> levelOutlinePixelCoordinates;
        private List<Vector2[]> levelObstaclesPixelCoordinates;

        public TileMapManager()
        {
            fileName = "Content\\tile_map.txt";

            tileSize = 64;

            mapOffsetX = 100;
            mapOffsetY = 0;

            levelOutline = new List<Vector2>();
            levelObstacles = new List<Vector2[]>();

            levelOutlinePixelCoordinates = new List<Vector2>();
            levelObstaclesPixelCoordinates = new List<Vector2[]>();

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

                    mapWidth = int.Parse (lines.ElementAt(0));
                    mapHeight = int.Parse (lines.ElementAt(1));

                    

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
                        }
                        else if (lineCounter == mapHeight + 2)
                        {

                            line = lines.ElementAt(lineCounter).Split('|');

                            //split each coord into 2
                            foreach (string coordinate in line)
                            {
                                newCoordinates.X = int.Parse (coordinate.Split(",").ElementAt(0));
                                newCoordinates.Y = int.Parse(coordinate.Split(",").ElementAt(1));

                                levelOutline.Add(newCoordinates);
                            }

                        }else
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

                    //Take corner positions for collision detection


                    /*
                    mapHeight = lines.Length;

                    int lineCounter;
                    for(lineCounter = 0; lineCounter < mapHeight; lineCounter++)
                    {
                        
                        var line = lines.ElementAt(lineCounter).Split(' ');

                        if (lineCounter == 0)
                            mapWidth = line.Length;
                        
                        if (line.Length == mapWidth)
                            levelMap.Add(line);
                        else
                            throw new Exception("Map line " +lineCounter+ " doesn't have right width");
                        
                    }
                    */
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

                for(coordinateCounter = 0; coordinateCounter < lineLength; coordinateCounter++)
                {
                    pixelCoordinates = new Vector2(levelObstacles[obstacleCounter].ElementAt(coordinateCounter).X * tileSize - mapOffsetX,
                                                    levelObstacles[obstacleCounter].ElementAt(coordinateCounter).Y * tileSize - mapOffsetY);
                    obstacleCoordinates[coordinateCounter] = pixelCoordinates;

                }
                levelObstaclesPixelCoordinates.Add(obstacleCoordinates);


            }
            //Convert the tile positions into pixel coordinates for where the shape is
        }

        public void ModifyOffset(Vector2 playerPosition)
        {
            //Depending on the player's position on the screen, either adds or subtracts from the x and y offsets
            //Called every player update

            if (playerPosition.X < 640 && mapOffsetX > 0)
                mapOffsetX--;
            else if (playerPosition.X > 1280 && mapOffsetX < mapWidth* tileSize - Globals.windowWidth) //not sure if should be mapWidth*64 -1 or -64 (maybe - screen length?)
                mapOffsetX++;


            if (playerPosition.Y < 360 && mapOffsetY > 0)
                mapOffsetY--;
            else if (playerPosition.Y > 1280 && mapOffsetY < mapHeight * tileSize - Globals.windowHeight)
                mapOffsetY++;


            //for x: 640 & 1280
            //for y: 360 & 720

            DetectMapLimits();
        }


        public void Draw()
        {
            //goes through the map and, if the tile is within visible parameters taking into account the offset, draws it
            int xCounter, yCounter;

            int xPosition, yPosition;

            String currentTile;

            for(yCounter = 0; yCounter < mapHeight; yCounter++)
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
                        switch(currentTile)
                        {
                            case "0":
                                Globals._spriteBatch.Draw(backgroundTile, new Rectangle(xPosition, yPosition, tileSize, tileSize), Color.White); //Color.PeachPuff
                                break;
                            case "1":
                                Globals._spriteBatch.Draw(platformTile, new Rectangle(xPosition, yPosition, tileSize, tileSize), Color.White); //Color.BlueViolet
                                break;
                            case "2":
                                Globals._spriteBatch.Draw(backgroundTile, new Rectangle(xPosition, yPosition, tileSize, tileSize), Color.Green);
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

        public void setBackgroundSprite(Texture2D backgroundTile)
        {
            this.backgroundTile = backgroundTile;
        }

        public void setPlatformSprite(Texture2D platformTile)
        {
            this.platformTile = platformTile;
        }

        public List<Vector2> getLevelOutlinePixelCoordinates()
        {
            return levelOutlinePixelCoordinates;
        }

        public List<Vector2[]> getObstaclesPixelCoordinates()
        {
            return levelObstaclesPixelCoordinates;
        }
    }


}
