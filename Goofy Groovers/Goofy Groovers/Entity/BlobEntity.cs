using Goofy_Groovers.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

/// <summary>
/// Blob... class
/// </summary>
///
namespace Goofy_Groovers.Entity
{
    public class BlobEntity
    {
        public string blobUserName { get; set; }
        public int blobUserId { get; set; }
        public Color blobUserColor { get; set; }

        public Vector2 worldPosition { get; set; }
        private Vector2 cameraPosition;

        private bool shortMovement = false;

        public bool finishedRace;
        public bool isJumping { get; set; } = false;
        public float jumpTheta { get; set; }
        public Vector2 jumpStartPoint { get; set; }
        public Vector2 jumpEndPoint { get; set; }
        public bool[] jumpDirection { get; set; }

        //private Sprite sprite;
        private Texture2D dotTexture;

        public int finishTime { get; set; } = -1;
        public int frameNumber { get; set; }
        public int animationNumber { get; set; }
        public float animationEndTime { get; set; }

        public bool isOwnedByUser { get; set; }
        private float timeSinceLastUpdate = 0;

        public float velocity;
        private float elapsedSecondsSinceJumpStart;
        public bool isStartingTheRace;

        // private float elapsedSecondsSinceLastSprite;


        private MouseState mouseState1;
        bool upsideDownSprite = false;
        bool IsOnTheLeftSprite = false;
        bool IsOnWalls = false;
        private Vector2 previousPosition;


        public BlobEntity()
        {
            jumpDirection = new bool[2];
            Random rnd = new Random();
            blobUserId = rnd.Next(1000);
<<<<<<< Updated upstream
=======
            blobRadius = 25;
>>>>>>> Stashed changes
        }

        public BlobEntity(Vector2 worldPosition, Vector2 cameraPosition, bool isOwnedByUser)
        {
            this.worldPosition = worldPosition;
            this.cameraPosition = cameraPosition;
            this.isOwnedByUser = isOwnedByUser;

            Random rnd = new Random();
            blobUserId = rnd.Next(1000);

            jumpStartPoint = worldPosition;
            jumpDirection = new bool[2];
<<<<<<< Updated upstream
=======

            blobRadius = 25;
>>>>>>> Stashed changes
        }

        public BlobEntity(Vector2 worldPosition, bool isOwnedByUser, Texture2D dotTexture)
        {
            this.worldPosition = worldPosition;
            this.isOwnedByUser = isOwnedByUser;

            Random rnd = new Random();
            blobUserId = rnd.Next(1000);
            jumpStartPoint = worldPosition;
            jumpDirection = new bool[2];
<<<<<<< Updated upstream
=======

            blobRadius = 25;
>>>>>>> Stashed changes
        }

        public BlobEntity(string userName, int userId, Color userColor, Vector2 worldPosition, bool isOwnedByUser)
        {
            blobUserName = userName;
            blobUserId = userId;

            this.worldPosition = worldPosition;
            this.isOwnedByUser = isOwnedByUser;
            dotTexture = Globals._dotTexture;

            jumpStartPoint = worldPosition;
            jumpDirection = new bool[2];
<<<<<<< Updated upstream
=======

            blobRadius = 25;
>>>>>>> Stashed changes
        }

        public void Update(GameTime elapsedSeconds)
        {
            timeSinceLastUpdate += (float)elapsedSeconds.ElapsedGameTime.TotalSeconds;
            if (timeSinceLastUpdate > 0.2f)
            {
                timeSinceLastUpdate = 0.2f;
            }

            elapsedSecondsSinceJumpStart += timeSinceLastUpdate;

            if (isJumping)
            {
                previousPosition = worldPosition;
                worldPosition = jumpStartPoint + new Vector2(
                        -velocity * (float)(Math.Cos(jumpTheta) * elapsedSecondsSinceJumpStart),
                        -velocity * (float)(Math.Sin(jumpTheta) * elapsedSecondsSinceJumpStart) - 0.5f * -9.8f * (float)Math.Pow(elapsedSecondsSinceJumpStart, 2));

                    if ((worldPosition.X < jumpEndPoint.X) != jumpDirection[0] || shortMovement)
                    {
                        worldPosition = jumpEndPoint;
                        isJumping = false;
                    }
                
                
            }
        }

        public void Draw(GameTime gameTime)
        {
            if (blobUserName.Length >= 13)
            {
                Globals._spriteBatch.DrawString(Globals._gameFont, blobUserName.Substring(0, 10) + "...", cameraPosition + new Vector2(-40, 35), Color.White);
            }
            else
            {
                Globals._spriteBatch.DrawString(Globals._gameFont, blobUserName, cameraPosition + new Vector2(-blobUserName.Length * 4, 35), Color.White);
            }
<<<<<<< Updated upstream
            Globals._spriteBatch.Draw(Globals._dotTexture, new Rectangle((int)cameraPosition.X - 12, (int)cameraPosition.Y - 12, 25, 25), blobUserColor);
=======

            Texture2D textureToDraw = SpriteDirection();

            Globals._spriteBatch.Draw(textureToDraw, new Rectangle((int)cameraPosition.X - blobRadius, (int)cameraPosition.Y - blobRadius, 50, 50), blobUserColor);

        }


        public Texture2D SpriteDirection()
        {
            mouseState1 = Mouse.GetState();
            if (isJumping)
            {
                return Globals._dotJumpTexture;
            }
            else if (IsOnWalls == false)
            {
                if (mouseState1.LeftButton == ButtonState.Pressed)
                {

                    return upsideDownSprite ? Globals._dotClickTextureUP : Globals._dotClickTexture;
                }
                if (mouseState1.LeftButton == ButtonState.Released)
                {
                    if (previousPosition.Y <= jumpEndPoint.Y)
                    {
                        return Globals._dotTexture;
                        upsideDownSprite = false;
                    }
                    else if (previousPosition.Y > jumpEndPoint.Y)
                    {
                        return Globals._dotUpTexture;
                        upsideDownSprite = true;
                    }
                }
                if (mouseState1.LeftButton != ButtonState.Released && mouseState1.LeftButton != ButtonState.Pressed)
                {
                    return Globals._dotTexture;
                    upsideDownSprite = false;
                }


            }
            else
            {
                if (mouseState1.LeftButton == ButtonState.Pressed)
                {

                    return IsOnTheLeftSprite ? Globals._dotClickTextureRigth : Globals._dotClickTexturLeft;
                }
                if (mouseState1.LeftButton == ButtonState.Released)
                {
                    if (previousPosition.X <= jumpEndPoint.X)
                    {
                        return Globals._dotRighttTexture;
                        IsOnTheLeftSprite = false;
                    }
                    else if (previousPosition.X > jumpEndPoint.X)
                    {
                        return Globals._dotLeftTexture;
                        IsOnTheLeftSprite = true;
                    }
                }
                if (mouseState1.LeftButton != ButtonState.Released && mouseState1.LeftButton != ButtonState.Pressed)
                {
                    return Globals._dotTexture;
                    upsideDownSprite = false;
                }

            }
            return Globals._dotJumpTexture;
>>>>>>> Stashed changes
        }

        public void DefineJumpDirection()
        {
            //if its true, means the end jump point has a higher X or Y value than the starting point

            if (jumpStartPoint.X <= jumpEndPoint.X)
                jumpDirection[0] = true;
            else
                jumpDirection[0] = false;

            //Y is not used but I'll keep it here just in case
            if (jumpStartPoint.Y <= jumpEndPoint.Y)
                jumpDirection[1] = true;
            else
                jumpDirection[1] = false;

            //if it moves less than 3 pixels, its considered a short movement
            if (Math.Abs(jumpStartPoint.X - jumpEndPoint.X) < 3 && Math.Abs(jumpStartPoint.Y - jumpEndPoint.Y) < 3)
                shortMovement = true;
            else
                shortMovement = false;
        }

        public void SetJumpEndPoint(Vector2 endpoint)
        {
            this.jumpEndPoint = endpoint;
        }

        public void SetJumpStartPoint(Vector2 startpoint)
        {
            this.jumpStartPoint = startpoint;
        }

        public void SetTexture(Texture2D texture)
        {
            this.dotTexture = texture;
        }

        public Texture2D GetTexture()
        {
            return dotTexture;
        }

        public float GetTheta()
        {
            return this.jumpTheta;
        }

        public float GetVelocity()
        {
            return this.velocity;
        }

        public Vector2 GetPosition()
        {
            return this.worldPosition;
        }

        public Vector2 GetEndpoint()
        {
            return this.jumpEndPoint;
        }

        public Vector2 GetCameraPosition()
        {
            return this.cameraPosition;
        }

        public void SetCameraPosition(Vector2 cameraPosition)
        {
            this.cameraPosition = cameraPosition;
        }

        public Vector2 GetWorldPosition()
        {
            return this.worldPosition;
        }

        public void SetJumpingState(bool isJumping)
        {
            this.isJumping = isJumping;
        }

        public void SetSecondsSinceJumpStarted(float seconds)
        {
            this.elapsedSecondsSinceJumpStart = seconds;
        }

        public bool GetJumpingState()
        {
            return this.isJumping;
        }

        public void SetVelocity(float velocity)
        {
            this.velocity = velocity;
        }

        public void SetThetha(float thetha)
        {
            this.jumpTheta = thetha;
        }

        public void SetUserName(string userName)
        {
            this.blobUserName = userName;
        }

        public void SetUserColor(Color color)
        {
            this.blobUserColor = color;
        }
    }

    public class Response
    {
        public string messageType;
        public string raceStarter;
        public int raceStarterId = -1;
        public DateTime startTime;
        public string message;

        public List<BlobEntity> playerList;
        public Response()
        { }

        public override string ToString()
        {
            return "\nmt: " + messageType?.ToString() +
                "\nrs: " + raceStarter?.ToString() +
                "\nrs: " + raceStarterId.ToString() +
                "\nrs: " + startTime.ToString() +
                "\nrs: " + base.ToString();
        }
    }
}