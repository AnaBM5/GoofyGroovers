﻿using Goofy_Groovers.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;

/// <summary>
/// Summary description for Class1
/// </summary>
public class MouseManager
{
    private MouseState mouseState;
    private MouseState oldState;
    private Boolean newJumpInitiated = false;
    private Boolean newJumpAttempted = false;

    private Point mouseClickStartPoint;
    private Point mouseClickEndPoint;

    private Point projectedLaunchPosition;

    //Don't need, but
    private Vector2 jumpDirection;

    private int jumpForceOutOfTen;
    private int clickDragDistance;
    private float theta;


    //Sprites and coordinates used for testing
    private Texture2D dotTexture;
    private Point directionVector;
    private Point horizontalVector;

    public MouseManager()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public void Update()
    {
        mouseState = Mouse.GetState();

        Console.WriteLine(mouseState.LeftButton == ButtonState.Pressed);
        if (!newJumpInitiated)              //if the jump hasn't been initiated it checks for mouse click
            CheckMousePressLeftButton();
        else                                //else, checks if it has been released
            CheckMouseReleaseLeftButton();

        // Do something even scarier
    }

    public void CheckMousePressLeftButton()
    {
        if (mouseState.LeftButton == ButtonState.Pressed)
        {
            mouseClickStartPoint = new Point(mouseState.X, mouseState.Y);
            newJumpInitiated = true;

            Debug.WriteLine("Start: "+mouseClickStartPoint);
        }
        // TODO: Show visual cues on p-p-p-power-meter!
    }

    public void CheckMouseReleaseLeftButton(/* here we'll put a reference to the blob*/)
    {
        if (mouseState.LeftButton == ButtonState.Pressed)
        {
            mouseClickEndPoint = new Point(mouseState.X, mouseState.Y);
        }
        else
        {
            Debug.WriteLine("End: "+mouseClickEndPoint);
            //Calculate angle
            jumpForceOutOfTen = 10; //potential units, kinda?
            newJumpInitiated = false;
            newJumpAttempted = true;
            CalculateAngle();
        }

        //  Check if the distance is bigger than accidental click (>10px for example?)

        // We will return following to the blob:
        // angle
        // velocity (vec2)
    }

    private void CalculateAngle()
    {
        //TODO: Verify angles being taken, maybe check horizontal vector depending on direction???
        //take difference between points and adds it to the original point to go to the other direction
        directionVector = mouseClickStartPoint + (mouseClickStartPoint - mouseClickEndPoint);
        //horizontal vector as reference for calculating angle
        horizontalVector = new Point(mouseClickStartPoint.X-100, mouseClickStartPoint.Y);


        //Squared lengths of vectors
        float invertedVectorLength = (float) (Math.Pow(directionVector.X, 2) + Math.Pow(directionVector.Y, 2));
        float horizontalVectorLength = (float) (Math.Pow(horizontalVector.X, 2) + Math.Pow(horizontalVector.Y, 2));

        //Squared dot product between them
        float dotProduct = (float)(Math.Pow(directionVector.X * horizontalVector.X + directionVector.Y * horizontalVector.Y,2));

        theta = (float)(Math.Acos(dotProduct / (invertedVectorLength * horizontalVectorLength)));

        float thetaDeegres = (float)(180 / Math.PI) * theta;

        Debug.WriteLine(theta);

    }

    public void Draw()
    {
        //draw the dots on the positions
        Globals._spriteBatch.Draw(dotTexture, new Rectangle(mouseClickStartPoint.X - 12, mouseClickStartPoint.Y-12, 25,25), Color.White);
        Globals._spriteBatch.Draw(dotTexture, new Rectangle(mouseClickEndPoint.X - 12, mouseClickEndPoint.Y - 12, 25, 25), Color.Green);

        Globals._spriteBatch.Draw(dotTexture, new Rectangle(directionVector.X - 12, directionVector.Y - 12, 25, 25), Color.LightGreen);
        Globals._spriteBatch.Draw(dotTexture, new Rectangle(horizontalVector.X - 12, horizontalVector.Y - 12, 25, 25), Color.Yellow);

    }
    
    public bool IsNewJumpAttempted()
    {
        return newJumpAttempted;
    }

    public void EndNewJumpAttempt()
    {
        this.newJumpAttempted = false;
    }

    internal float GetTheta()
    {
        return theta;
    }

    internal float GetVelocity()
    {
        return 100;
    }
    public bool IsNewJumpInitiated()
    {
        return newJumpInitiated;
    }

    public void setDotSprite (Texture2D dotTexture)
    {
        this.dotTexture = dotTexture;
    }
}