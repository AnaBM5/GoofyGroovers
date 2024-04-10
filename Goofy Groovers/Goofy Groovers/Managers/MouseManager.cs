using Goofy_Groovers;
using Goofy_Groovers.Managers;
using Microsoft.Xna.Framework;
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
    private Boolean newJumpAttempted = false;
    private Boolean newJumpCancelled = false;

    private Point mouseClickStartPoint;
    private Point mouseClickEndPoint;

    private Point projectedLaunchPosition;

    //Don't need, but
    private Vector2 jumpDirection;

    private int jumpForceOutOfTen;
    private int clickDragDistance;
    private float theta;

    private float maxVectorLength;
    private float movementVelocity;

    //Sprites and coordinates used for testing

    private Point directionVector;
    private Point horizontalVector;

    public MouseManager()
    {
        //
        // TODO: Add constructor logic here
        //
        maxVectorLength = 400;
    }

    public void Update(GoofyGroovers game)
    {
        mouseState = Mouse.GetState();

        if (newJumpCancelled)
            if (mouseState.LeftButton == ButtonState.Released) //if the jump has just been cancelled, waits for the left click to be released 
                newJumpCancelled = false;                       //before starting a new jump
            else
                newJumpCancelled = true;
        else
        {
            if (!newJumpInitiated)              //if the jump hasn't been initiated it checks for mouse click
                CheckMousePressLeftButton(game);
            else                                //else, checks if it has been released
                CheckMouseReleaseLeftButton();
        }
    }

    public void CheckMousePressLeftButton(GoofyGroovers game)
    {
        if (mouseState.LeftButton == ButtonState.Pressed &&
            !(mouseState.RightButton == ButtonState.Pressed) &&
            game.IsActive)
        {
            mouseClickStartPoint = new Point(mouseState.X, mouseState.Y);
            newJumpInitiated = true;

            // Debug.WriteLine("Start: " + mouseClickStartPoint);
        }
        // TODO: Show visual cues on p-p-p-power-meter!
    }

    public void CheckMouseReleaseLeftButton()
    {
        if (mouseState.RightButton == ButtonState.Pressed) //right click to cancel jump
        {
            newJumpInitiated = false;
            newJumpCancelled = true;

            mouseClickStartPoint = new Point(-10, -10);  //give points position outside visible area
            mouseClickEndPoint = mouseClickStartPoint;
        } 
        else if (mouseState.LeftButton == ButtonState.Pressed)
        {
            mouseClickEndPoint = new Point(mouseState.X, mouseState.Y);
            CalculateAngle();
        }
        else
        {
            newJumpInitiated = false;
            newJumpAttempted = true;
        }

        

        //  Check if the distance is bigger than accidental click (>10px for example?)
    }

    private void CalculateAngle()
    {
        //take difference between points and adds it to the original point to go to the other direction
        directionVector = mouseClickStartPoint + (mouseClickStartPoint - mouseClickEndPoint);
        //horizontal vector as reference for calculating angle
        horizontalVector = new Point(mouseClickStartPoint.X - 100, mouseClickStartPoint.Y);

        Point originDirectionVector = directionVector - mouseClickStartPoint;
        Point originHorizontalVector = horizontalVector - mouseClickStartPoint;

        //lengths of vectors
        float invertedVectorLength = (float)Math.Sqrt((Math.Pow(originDirectionVector.X, 2) + Math.Pow(originDirectionVector.Y, 2)));
        float horizontalVectorLength = (float)Math.Sqrt((Math.Pow(originHorizontalVector.X, 2) + Math.Pow(originHorizontalVector.Y, 2)));

        //uses length to define Speed
        DefineVelocity(invertedVectorLength);

        //dot product between them
        float dotProduct = (float)originDirectionVector.X * originHorizontalVector.X + originDirectionVector.Y * originHorizontalVector.Y;

        if (invertedVectorLength == 0)
            theta = (float)Math.PI;
        else
            theta = (float)(Math.Acos(dotProduct / (invertedVectorLength * horizontalVectorLength)));

        //if the mouse was dragged to a lower y position than the starting point, adjusts it so it takes angles from 180 degrees to 360
        if (mouseClickEndPoint.Y < mouseClickStartPoint.Y)
            theta = (float)(2 * Math.PI) - theta;

        float thetaDeegres = (float)(180 / Math.PI) * theta;

        // Debug.WriteLine(thetaDeegres);
    }

    public void Draw()
    {
        //draw the dots on the positions
        //Globals._spriteBatch.Draw(Globals._dotTexture, new Rectangle(mouseClickStartPoint.X - 12, mouseClickStartPoint.Y - 12, 25, 25), Color.White);
        //Globals._spriteBatch.Draw(dotTexture, new Rectangle(mouseClickEndPoint.X - 12, mouseClickEndPoint.Y - 12, 25, 25), Color.Green);

        //Globals._spriteBatch.Draw(Globals._dotTexture, new Rectangle(directionVector.X - 12, directionVector.Y - 12, 25, 25), Color.LightGreen);
        //Globals._spriteBatch.Draw(dotTexture, new Rectangle(horizontalVector.X - 12, horizontalVector.Y - 12, 25, 25), Color.Yellow);
    }

    private void DefineVelocity(float currentVectorLength)
    {
        float maxSpeed = 110;

        movementVelocity = 20;

        if (currentVectorLength >= maxVectorLength)
            movementVelocity += maxSpeed;
        if (currentVectorLength < maxVectorLength)
        {
            maxSpeed *= currentVectorLength / maxVectorLength;
            movementVelocity += maxSpeed;
        }

        //Debug.WriteLine("Speed: " + movementVelocity);
    }

    public bool IsNewJumpAttempted()
    {
        return newJumpAttempted;
    }

    public void EndNewJumpAttempt()
    {
        this.newJumpAttempted = false;
    }

    public float GetTheta()
    {
        return theta;
    }

    public float GetVelocity()
    {
        return movementVelocity;
    }

    public bool IsNewJumpInitiated()
    {
        return newJumpInitiated;
    }

    public bool IsJumpCancelled()
    {
        return newJumpCancelled;
    }
}