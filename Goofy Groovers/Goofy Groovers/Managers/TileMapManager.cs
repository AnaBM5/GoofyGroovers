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

        public TileMapManager()
        {
            fileName = "Content\\TileMap\\tile_map.txt";
            mapOffsetX = 2400;
            mapOffsetY = 0;


            ReadFile();
            ModifyOffset(new Vector2(0,0));
        }
        private void ReadFile()
        {
            String path = Path.Combine(Environment.CurrentDirectory, fileName);
            //Debug.WriteLine("PATH: " + path);


            //Search how can we avoid using strings for this, too expensive
            List<String[]> levelMap = new List<String[]>();

            try
            {
                if (File.Exists(path))
                {
                    var lines = File.ReadAllLines(path);                                                                                                                                //We Read from text file
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
            //Convert the tile positions into pixel coordinates for where the shape is
        }

        public void ModifyOffset(Vector2 playerPosition)
        {
            //Depending on the player's position on the screen, either adds or subtracts from the x and y offsets
            //Called every player update

            if (playerPosition.X < 640 && mapOffsetX > 0)
                mapOffsetX--;
            else if (playerPosition.X > 1280 && mapOffsetX < mapWidth*64 - Globals.windowWidth) //not sure if should be mapWidth*64 -1 or -64 (maybe - screen length?)
                mapOffsetX++;


            if (playerPosition.Y < 360 && mapOffsetY > 0)
                mapOffsetY--;
            else if (playerPosition.Y > 1280 && mapOffsetY < mapHeight * 64 - Globals.windowHeight)
                mapOffsetY++;


            //for x: 640 & 1280
            //for y: 360 & 720
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

                    xPosition = xCounter * 64 - mapOffsetX;
                    yPosition = yCounter * 64 - mapOffsetY;

                    if (yPosition > -64 && yPosition < Globals.windowHeight
                        && xPosition > -64 && xPosition < Globals.windowWidth)
                    {
                        //Debug.WriteLine("Show tile in position" + xCounter + " , " + yCounter);
                        switch(currentTile)
                        {
                            case "0":
                                Globals._spriteBatch.Draw(backgroundTile, new Rectangle(xPosition, yPosition, 64, 64), Color.PeachPuff);
                                break;
                            case "1":
                                Globals._spriteBatch.Draw(platformTile, new Rectangle(xPosition, yPosition, 64, 64), Color.BlueViolet);
                                break;
                            case "2":
                                Globals._spriteBatch.Draw(platformTile, new Rectangle(xPosition, yPosition, 64, 64), Color.Green);
                                break;
                            default:
                                Globals._spriteBatch.Draw(platformTile, new Rectangle(xPosition, yPosition, 64, 64), Color.White);
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
    }


}
