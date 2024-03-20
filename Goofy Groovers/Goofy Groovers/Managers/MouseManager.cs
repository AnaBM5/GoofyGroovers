﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

/// <summary>
/// Summary description for Class1
/// </summary>
public class MouseManager
{
    private MouseState mouseState;
    private MouseState oldState;
    private Boolean isNewJumpInitiated = false;

    private Point mouseClickStartPoint;
    private Point mouseClickEndPoint;

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

    public void Update(float elapsedSeconds)
    {
        CheckMousePressLeftButton();
        if (isNewJumpInitiated) CheckMouseReleaseLeftButton();

        // Do something even scarier
    }

    public void CheckMousePressLeftButton()
    {
        if (mouseState.LeftButton == ButtonState.Pressed)
        {
            mouseClickStartPoint = new Point(mouseState.X, mouseState.Y);
            isNewJumpInitiated = true;
        }
        // TODO: Show visual cues on p-p-p-power-meter!
    }

    public void CheckMouseReleaseLeftButton(/* here we'll put a reference to the blob*/)
    {
        if (mouseState.LeftButton == ButtonState.Released)
        {
            jumpForceOutOfTen = 10; //potential units, kinda?
            isNewJumpInitiated = false;
        }

        //  Check if the distance is bigger than accidental click (>10px for example?)

        // We will return following to the blob:
        // angle
        // velocity (vec2)
    }


    public bool newJumpInitiated()
    {
        return isNewJumpInitiated;
    }
}