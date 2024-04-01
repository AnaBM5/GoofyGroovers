using Goofy_Groovers.Entity;
using Goofy_Groovers.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

        public Vector2 position { get; set; }
        public bool isJumping { get; set; } = false;
        public Vector2 jumpStartPoint { get; set; }
        public Vector2 jumpEndPoint { get; set; }

        public int frameNumber { get; set; }
        public int animationNumber { get; set; }
        public float animationEndTime { get; set; }

        public bool isOwnedByUser { get; set; }
        private double timeSinceLastUpdate = 0;

        public float velocity;
        private float elapsedSecondsSinceJumpStart;
        private float elapsedSecondsSinceLastSprite;

        public float jumpTheta { get; set; }

        //private Sprite sprite;
        private Texture2D dotTexture;

        public void Update(GameTime elapsedSeconds)
        {
            timeSinceLastUpdate += (float)elapsedSeconds.ElapsedGameTime.TotalSeconds;
            if (timeSinceLastUpdate > 1)
            {
                timeSinceLastUpdate = 0;

                if (isJumping)
                {
                    position = jumpEndPoint;
                    // TODO: Proper moving across the map
                    isJumping = false;
                }
            }
        }

        public void Draw(GameTime gameTime)
        {
            if (blobUserName.Length >= 13)
            {
                Globals._spriteBatch.DrawString(Globals._gameFont, blobUserName.Substring(0, 10) + "...", GetPosition() + new Vector2(-35, 20), Color.Black);
            }
            else
            {
                Globals._spriteBatch.DrawString(Globals._gameFont, blobUserName, GetPosition() + new Vector2(-blobUserName.Length * 3, 20), Color.Black);
            }
            Globals._spriteBatch.Draw(dotTexture, new Rectangle((int)GetPosition().X - 12, (int)GetPosition().Y - 12, 25, 25), blobUserColor);
        }

        public BlobEntity() { }

        public BlobEntity(string name, Color color, Vector2 position)
        {
            this.blobUserName = name;
            this.blobUserColor = color;
            this.position = position;
            isOwnedByUser = false;

            Random random = new();
            blobUserId = random.Next(1000);
        }

        public BlobEntity(string name, bool isOwnedByUser, Color blobUserColor, Vector2 position) : this(name, blobUserColor, position)
        {
            this.isOwnedByUser = isOwnedByUser;
        }

        public BlobEntity(string name, bool isOwnedByUser, int id, Color blobUserColor, Vector2 position) : this(name, isOwnedByUser, blobUserColor, position)
        {
            this.blobUserId = id;
        }

        public BlobEntity(string name, bool isOwnedByUser, int blobUserId, Color color, Vector2 position, bool isJumping, Vector2 jumpStartPoint, Vector2 jumpEndPoint) : this(name, isOwnedByUser, blobUserId, color, position)
        {
            this.isJumping = isJumping;
            this.jumpStartPoint = jumpStartPoint;
            this.jumpEndPoint = jumpEndPoint;
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
            return this.position;
        }

        public Vector2 GetEndpoint()
        {
            return this.jumpEndPoint;
        }

        public void SetJumpingState(bool isJumping)
        {
            this.isJumping = isJumping;
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
        public string responseType;
        public List<BlobEntity> playerList;

        public Response()
        { }
    }
}