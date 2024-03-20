using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

/// <summary>
/// Summary description for Class1
/// </summary>
public class MouseManager
{
    private MouseState mouseState;
    private MouseState oldState;
    private Boolean newJumpInitiated = false;
    private Point mouseClickStartPoint;
    private Point mouseClickEndPoint;

    private Point projectedLaunchPosition;

    //Don't need, but
    private Vector2 jumpDirection;

    private int jumpForceOutOfTen;
    private int clickDragDistance;

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

            //Calculate angle
            jumpForceOutOfTen = 10; //potential units, kinda?
            newJumpInitiated = false;
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
        Point directionVector = mouseClickStartPoint + (mouseClickStartPoint - mouseClickEndPoint);
        //horizontal vector as reference for calculating angle
        Point horizontalVector = new Point(mouseClickStartPoint.X-10, mouseClickStartPoint.Y);


        //Squared lengths of vectors
        float invertedVectorLength = (float) (Math.Pow(directionVector.X, 2) + Math.Pow(directionVector.Y, 2));
        float horizontalVectorLength = (float) (Math.Pow(horizontalVector.X, 2) + Math.Pow(horizontalVector.Y, 2));

        //Squared dot product between them
        float dotProduct = (float)(Math.Pow(directionVector.X * horizontalVector.X + directionVector.Y * horizontalVector.Y,2));

        float theta = (float)(Math.Acos(dotProduct / (invertedVectorLength * horizontalVectorLength)));

        float thetaDeegres = (float)(180 / Math.PI) * theta;

    }

    internal void update()
    {
        throw new NotImplementedException();
    }
}