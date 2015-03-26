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
        set
        {
            if (value.X > 0 || value.Y > 0 || value.X < -1240 || value.Y < -1240) //todo, JUISTE RESOLUTIE
                return;
            else
            {

                cameraPosition = value;
                
            }
        }
    }


}

