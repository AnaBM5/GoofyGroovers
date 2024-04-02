﻿using Goofy_Groovers.Managers;
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
    private Vector2 cameraPosition;
    private Vector2 worldPosition;
    private bool isOwnedByUser;    
    private float timeSinceLastUpdate = 0;

    private float velocity;
    private float elapsedSecondsSinceJumpStart;
    //private float elapsedSecondsSinceLastSprite;

    private float jumpTheta;
    private Vector2 jumpEndPoint;
    private Vector2 jumpStartPoint;
    private bool[] jumpDirection;

    private string blobUserName;
    private Color blobUserColor;

    //private Sprite sprite;
    private SpriteBatch spriteBatch;

    private Texture2D dotTexture;


    public BlobEntity()
    {
        jumpDirection = new bool[2];
    }

    public BlobEntity(Vector2 worldPosition, Vector2 cameraPosition, bool isOwnedByUser)
    {
        this.worldPosition = worldPosition;
        this.cameraPosition = cameraPosition;
        this.isOwnedByUser = isOwnedByUser;

        jumpStartPoint = worldPosition;
        jumpDirection = new bool[2];
    }
    /* changeOffset automatically changes worldPos so this not necessary?
    public BlobEntity(Vector2 cameraPosition, bool isOwnedByUser, Vector2 worldPosition)
    {
        this.cameraPosition = cameraPosition;
        this.worldPosition = worldPosition;
        this.isOwnedByUser = isOwnedByUser;

        jumpStartPoint = cameraPosition;
        jumpDirection = new bool[2];
    }
    */
    public BlobEntity(Vector2 worldPosition, bool isOwnedByUser, Texture2D dotTexture)
    {
        this.worldPosition = worldPosition;
        this.isOwnedByUser = isOwnedByUser;
        this.dotTexture = dotTexture;

        jumpStartPoint = worldPosition;
        jumpDirection = new bool[2];
    }

    public void Update(GameTime elapsedSeconds)
    {
        timeSinceLastUpdate += (float)elapsedSeconds.ElapsedGameTime.TotalSeconds;
        if(timeSinceLastUpdate > 0.2f)
        {
            timeSinceLastUpdate = 0.2f;
        }

        elapsedSecondsSinceJumpStart += timeSinceLastUpdate;

        //Here make it move if isJumping is true
        //We got start, end, theta and velocity, just define current position base on deltaTime


        if (isJumping)
        {
                
                worldPosition = jumpStartPoint + new Vector2(
                        -velocity * (float)(Math.Cos(jumpTheta) * elapsedSecondsSinceJumpStart),
                        -velocity * (float)(Math.Sin(jumpTheta) * elapsedSecondsSinceJumpStart) - 0.5f * -9.8f * (float)Math.Pow(elapsedSecondsSinceJumpStart, 2));

                if ((worldPosition.X < jumpEndPoint.X) != jumpDirection[0])
                {
                    worldPosition = jumpEndPoint;
                    isJumping = false;

                }
                

        }

        /*
        if (timeSinceLastUpdate > 1)
        {
            timeSinceLastUpdate = 0;

            if (isJumping)
            {
                position = jumpEndPoint;
                isJumping = false;
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
            }
        }*/
    }

    public void Draw(GameTime gameTime)
    {
        if (blobUserName.Length >= 13)
        {
            Globals._spriteBatch.DrawString(Globals._gameFont, blobUserName.Substring(0, 10) + "...", cameraPosition + new Vector2(-35, 20), Color.Black);
        }
        else
        {
            Globals._spriteBatch.DrawString(Globals._gameFont, blobUserName, cameraPosition + new Vector2(-blobUserName.Length * 3, 20), Color.Black);
        }
        Globals._spriteBatch.Draw(dotTexture, new Rectangle((int)cameraPosition.X - 12, (int)cameraPosition.Y - 12, 25, 25), blobUserColor);
    }

    public void DefineJumpDirection()
    {
        //if its true, means the end jump point has a higher X or Y value than the starting point

        if (jumpStartPoint.X <= jumpEndPoint.X)
            jumpDirection[0] = true;
        else
            jumpDirection[0] = false;

        if (jumpStartPoint.Y <= jumpEndPoint.Y)
            jumpDirection[1] = true;
        else
            jumpDirection[1] = false;
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

    public void SetCameraPosition(Vector2 position)
    {
        this.cameraPosition = position;
    }

    public Vector2 GetCameraPosition()
    {
        return this.cameraPosition;
    }

    public Vector2 GetWorldPosition()
    {
        return this.worldPosition;
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

    public void SetSecondsSinceJumpStarted(float time)
    {
        this.elapsedSecondsSinceJumpStart = time;
    }

}