using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

public class InputManager
{
    private KeyboardState prevKeyState, keyState;
    private MouseState prevMouseState, mouseState;

    public KeyboardState PrevKeyState
    {
        get { return prevKeyState; }
    }

    public KeyboardState KeyState
    {
        get { return keyState ; }
    }

    public void Update()
    {
        prevKeyState = keyState;
        prevMouseState = mouseState;
        keyState = Keyboard.GetState();
        mouseState = Mouse.GetState();
    }

    public bool KeyPressed(Keys key)
    {
        if (keyState.IsKeyDown(key) && prevKeyState.IsKeyUp(key)) return true;
        return false;
    }

    public bool KeyPressed(params Keys[] keys)
    {
        foreach(Keys key in keys)
            if (keyState.IsKeyDown(key) && prevKeyState.IsKeyUp(key)) return true;

        return false;
    }

    public bool KeyReleased(Keys key)
    {
        if (keyState.IsKeyUp(key) && prevKeyState.IsKeyDown(key)) return true;
        return false;
    }

    public bool KeyReleased(params Keys[] keys)
    {
        foreach (Keys key in keys)
            if (keyState.IsKeyUp(key) && prevKeyState.IsKeyDown(key)) return true;

        return false;
    }

    public bool KeyDown(Keys key)
    {
        if (keyState.IsKeyDown(key)) return true;
        return false;
    }

    public bool KeyDown(params Keys[] keys)
    {
        foreach(Keys key in keys)
            if (keyState.IsKeyDown(key)) return true;
        return false;
    }

    public Vector2 MousePos()
    {
        return new Vector2(mouseState.X / ScreenManager.Instance.DrawScale().M11, mouseState.Y / ScreenManager.Instance.DrawScale().M22);
    }

    public Vector2 MousePosClean()
    {
        return new Vector2(mouseState.X, mouseState.Y);
    }

    public bool MouseLeftButtonPressed()
    {
        return mouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released && ScreenManager.Instance.Game.IsActive;
    }

    public bool MouseLeftButtonReleased()
    {
        return mouseState.LeftButton == ButtonState.Released && prevMouseState.LeftButton == ButtonState.Pressed && ScreenManager.Instance.Game.IsActive;
    }

    public bool MouseLeftButtonDown()
    {
        return mouseState.LeftButton == ButtonState.Pressed && ScreenManager.Instance.Game.IsActive;
    }


    public bool MouseRightButtonPressed()
    {
        return mouseState.RightButton == ButtonState.Pressed && prevMouseState.RightButton == ButtonState.Released && ScreenManager.Instance.Game.IsActive;
    }

    public bool MouseRightButtonReleased()
    {
        return mouseState.RightButton == ButtonState.Released && prevMouseState.RightButton == ButtonState.Pressed && ScreenManager.Instance.Game.IsActive;
    }

    public bool MouseRightButtonDown()
    {
        return mouseState.RightButton == ButtonState.Pressed && ScreenManager.Instance.Game.IsActive;
    }

    public bool MouseMiddleButtonPressed()
    {
        return mouseState.MiddleButton == ButtonState.Pressed && prevMouseState.MiddleButton == ButtonState.Released && ScreenManager.Instance.Game.IsActive;
    }

    public bool MouseMiddleButtonReleased()
    {
        return mouseState.MiddleButton == ButtonState.Released && prevMouseState.MiddleButton == ButtonState.Pressed && ScreenManager.Instance.Game.IsActive;
    }

    public bool MouseMiddleButtonDown()
    {
        return mouseState.MiddleButton == ButtonState.Pressed && ScreenManager.Instance.Game.IsActive;
    }

    public int MouseWheelScrolled()
    {
        return mouseState.ScrollWheelValue - prevMouseState.ScrollWheelValue;
    }

}
