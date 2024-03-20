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

    public void Update(float elapsedSeconds)
    {
        if (isJumping)
        {
            elapsedSecondsSinceJumpStart += elapsedSeconds;
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

    public BlobEntity()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public BlobEntity(Vector2 position, bool isOwnedByUser)
    {
        this.position = position;
        this.isOwnedByUser = isOwnedByUser;
    }

    public void SetJumpEndPoint(Vector2 endpoint)
    {
        this.jumpEndPoint = endpoint;
    }

    internal float GetTheta()
    {
        return this.jumpTheta;
        throw new NotImplementedException();
    }

    internal float GetVelocity()
    {
        return this.velocity;
        throw new NotImplementedException();
    }
}