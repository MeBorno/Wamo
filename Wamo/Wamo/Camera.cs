using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class Camera
{
    static Vector2 cameraPosition;
    public Camera()
    {
        cameraPosition = Vector2.Zero;
    }

    public static Vector2 CameraPosition
    {
        get { return cameraPosition; }
        set { cameraPosition = value; }
    }


}

