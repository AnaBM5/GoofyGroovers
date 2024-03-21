using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

/// <summary>
/// Summary description for Class1
/// </summary>
public class BlobEntity
{
    private bool isJumping = false;
    private Vector2 position;
    private bool isOwnedByUser;
    private Vector2 positionOld; //TODO: decide fate?

    private float velocity;
    private float elapsedSecondsSinceJumpStart;
    private float elapsedSecondsSinceLastSprite;

    private float jumpTheta;
    private Vector2 jumpEndPoint;
    private Vector2 jumpStartPoint;

    //private Sprite sprite;
    private SpriteBatch spriteBatch;

    public void Update(GameTime elapsedSeconds)
    {
        if (isJumping)
        {
            elapsedSecondsSinceJumpStart += (float) elapsedSeconds.ElapsedGameTime.TotalSeconds;
            positionOld = position;
            position = jumpStartPoint + new Vector2(
                velocity * (float)Math.Cos(jumpTheta) * elapsedSecondsSinceJumpStart,
                velocity * (float)Math.Sin(jumpTheta) * elapsedSecondsSinceJumpStart - 0.5f * 9.8f * (float)Math.Pow(elapsedSecondsSinceJumpStart, 2));

            if (jumpTheta > 90 / (180 * Math.PI) && jumpTheta < 270 / (180 * Math.PI) && (position.X >= jumpEndPoint.X) || (position.X <= jumpEndPoint.X))
            {
                position = jumpEndPoint; //TODO: Is it adjusted for the sprite size (radius-wise)?

                jumpTheta = 0;
                isJumping = false;
            }

            // Check if we're at the end yet
        }
    }

    public void Draw()
    {
    }

    public BlobEntity() {}

    public BlobEntity(Vector2 position, bool isOwnedByUser)
    {
        this.position = position;
        this.isOwnedByUser = isOwnedByUser;
    }

    public void SetJumpEndPoint(Vector2 endpoint)
    {
        this.jumpEndPoint = endpoint;
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
}