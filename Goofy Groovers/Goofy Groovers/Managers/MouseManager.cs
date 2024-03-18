using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

/// <summary>
/// Summary description for Class1
/// </summary>
public class MouseManager

{

    Point mouseClickStartPoint;
    Point mouseClickEndPoint;
    //Don't need, but
    Vector2 jumpDirection;

    MouseState mouseState;
    private MouseState oldState;

    int clickDragDistance;

    public MouseManager()
	{

        //
        // TODO: Add constructor logic here
        //
    }

    public void Update()
    {
        if (CheckMousePressLeftButton())
            {
            CheckMouseReleaseLeftButton();
            // Do something scary
        }
        // Do something even scarier
    }
    public bool CheckMousePressLeftButton()
    {
        if (mouseState.LeftButton == ButtonState.Pressed)
        {
            mouseClickStartPoint = new Point(mouseState.X, mouseState.Y);
            return true;
        }
        return false;

        // TODO: Show visual cues on p-p-p-power-meter!
    }

    public void CheckMouseReleaseLeftButton(/* here we'll put a reference to the blob*/)
    {
        //  Check if the distance is bigger than accidental click (>10px for example?)

        // We will return following to the blob:
        // angle
        // velocity (vec2)
    }

    internal void update()
    {
        throw new NotImplementedException();
    }
}
