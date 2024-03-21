using Goofy_Groovers.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;

/// <summary>
/// Summary description for Class1
/// </summary>
public class BlobEntity
{
    private bool isJumping = false;
    private Vector2 position;
    private bool isOwnedByUser;
    private Vector2 positionOld; //TODO: decide fate?
    private double timeSinceLastUpdate = 0;

    private float velocity;
    private float elapsedSecondsSinceJumpStart;
    private float elapsedSecondsSinceLastSprite;

    private float jumpTheta;
    private Vector2 jumpEndPoint;
    private Vector2 jumpStartPoint;

    //private Sprite sprite;
    private SpriteBatch spriteBatch;

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
            }

            /*if (isJumping)
            {
                elapsedSecondsSinceJumpStart += (float) (elapsedSeconds.ElapsedGameTime.TotalSeconds);
                //elapsedSecondsSinceJumpStart += (float) 0.1;
                positionOld = position;
                position = jumpStartPoint + new Vector2(
                    -velocity * (float)Math.Cos(jumpTheta) * elapsedSecondsSinceJumpStart,
                    -velocity * (float)Math.Sin(jumpTheta) * elapsedSecondsSinceJumpStart - 0.5f * -9.8f * (float)Math.Pow(elapsedSecondsSinceJumpStart, 2));
                
                if (jumpTheta > 90 / (180 * Math.PI) && jumpTheta < 270 / (180 * Math.PI) && (position.X >= jumpEndPoint.X) || (position.X <= jumpEndPoint.X))
                {
                    position = jumpEndPoint; // Is it adjusted for the sprite size (radius-wise)?
                    jumpTheta = 0;
                    isJumping = false;
                }

                if (position.Y > 1000) *//*If fell through the floor *//* 
                {
                    position = new Vector2(192, 192);
                }
            }*/
        }
    }

    public void Draw(GameTime gameTime)
    {
        Globals._spriteBatch.Draw(dotTexture, new Rectangle((int)GetPosition().X - 12, (int)GetPosition().Y - 12, 25, 25), Color.Red);
    }

    public BlobEntity()
    {
    
    }

    public BlobEntity(Vector2 position, bool isOwnedByUser)
    {
        this.position = position;
        this.isOwnedByUser = isOwnedByUser;
    }

    public BlobEntity(Vector2 position, bool isOwnedByUser, Texture2D dotTexture)
    {
        this.position = position;
        this.isOwnedByUser = isOwnedByUser;
        this.dotTexture = dotTexture;
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

    internal void SetJumping(bool isJumping)
    {
        this.isJumping = isJumping;
    }

    internal void SetVelocity(float velocity)
    {
        this.velocity = velocity; 
    }

    internal void SetThetha(float thetha)
    {
        this.jumpTheta = thetha;
    }
}