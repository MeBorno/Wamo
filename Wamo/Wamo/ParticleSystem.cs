using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class ParticleSystem
{
    /*
     * Field Region
     */
    List<Particle> particleList;
    List<Cannon> cannonList;
    Texture2D pixelParticle;
    Random r;


    /*
     * Constructor
     */
    public ParticleSystem()
    {
        pixelParticle = Wamo.manager.Content.Load<Texture2D>("Content/smoke");
        particleList = new List<Particle>();
        particleList.Clear();
        cannonList = new List<Cannon>();
        cannonList.Clear();
        r = new Random();
    }


    /*
     * Methods
     */

    /// <summary>
    /// Creates a new particle
    /// </summary>
    /// <param name="sprite"></param>
    /// <param name="position"></param>
    /// <param name="duration"></param>
    /// <param name="speed"></param>
    /// <param name="angle"></param>
    /// <param name="scale"></param>
    /// <param name="color"></param>
    /// <param name="fade"></param>
    /// <param name="opacity_change"></param>
    public void CreateParticle(Texture2D sprite, Vector2 position, float duration, float speed, float angle, float scale, Color startcolor, Color endcolor, bool fade = false, float opacity_change = 0.05f)
    {
        Particle tmp = new Particle();
        tmp.position = position;
        tmp.time_duration = duration;
        tmp.speed = speed;
        tmp.angle = angle;
        tmp.scale = scale/10;
        tmp.startColor = startcolor;
        tmp.endColor = endcolor;
        tmp.currentColor = startcolor;
        tmp.sprite = sprite;
        tmp.fade = fade;
        if (fade) tmp.opacity = 0;
        tmp.opacity_change = opacity_change;
        particleList.Add(tmp);
    }

    /// <summary>
    /// Creates an explosion with an amount of particles
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="position"></param>
    /// <param name="averageColor"></param>
    /// <param name="fade"></param>
    /// <param name="opacity_change"></param>
    /// <param name="ttl"></param>
    public void CreateExplosion(int amount, Vector2 position, Color averageColor, bool fade = false, float opacity_change = 0.05f, float ttl = 400f)
    {
        
        for (int i = 0; i < amount; i++)
        {
            float angle = (float)(r.NextDouble()) * 360f;
            float scale = (float)(r.Next(3,8));

            Color color = averageColor * (float)(r.Next(1,20) / 20.0);
            
          //  CreateParticle(pixelParticle, position, ttl, 1f, angle, scale, color, fade, opacity_change);
        }
    }


    /// <summary>
    /// Creates a particle trail with an amount of particles 
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="startPos"></param>
    /// <param name="endPos"></param>
    /// <param name="color"></param>
    /// <param name="fade"></param>
    /// <param name="opacity_change"></param>
    public void CreateTrail(int amount, Vector2 startPos, Vector2 endPos, Color color, bool fade = true, float opacity_change = 0.05f)
    {
        
        Vector2 diff = new Vector2((endPos.X - startPos.X) / amount, (endPos.Y - startPos.Y) / amount);


        for (int i = 0; i < amount; i++)
        {
            CreateExplosion(30, startPos + (diff * i), color, true, 0.15f, 200f);
           

            
        }
    }


    /// <summary>
    /// Creates a strom of particles
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="startPos"></param>
    /// <param name="endPos"></param>
    /// <param name="color"></param>
    public void CreateStorm(int amount, Vector2 startPos, Vector2 endPos, Color color)
    {

        for (int i = 0; i < amount; i++)
        {
            if (startPos.X < endPos.X)
            {
               // Vector2 tmp = new Vector2(0, r.Next(0, GameEnvironment.Screen.Y));
               // CreateParticle(pixelParticle, tmp, 6000f, r.Next(10, 20) * ((float)GameEnvironment.Random.NextDouble() + 0.6f), 90, GameEnvironment.Random.Next(2, 10) * (float)GameEnvironment.Random.NextDouble(), color, true, 0.010f);
            }
            else
            {
               // Vector2 tmp = new Vector2(GameEnvironment.Screen.X, GameEnvironment.Random.Next(0, GameEnvironment.Screen.Y));
               // CreateParticle(pixelParticle, tmp, 6000f, GameEnvironment.Random.Next(-20, -10) * ((float)GameEnvironment.Random.NextDouble() + 0.6f), 90, GameEnvironment.Random.Next(2, 10) * (float)GameEnvironment.Random.NextDouble(), color, true, 0.010f);
            }
        }
    }


    

    /// <summary>
    /// Creates a cannon with particles.
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="amount"></param>
    /// <param name="duration"></param>
    /// <param name="startPos"></param>
    /// <param name="endPos"></param>
    /// <param name="color"></param>
    /// <param name="fade"></param>
    /// <param name="opacity_change"></param>
    public void CreateCannon(Texture2D texture, int amount, int duration, Vector2 startPos, Vector2 endPos, Color startcolor, Color endcolor, bool fade = false, float opacity_change = 0.0f)
    {

        Cannon tmp = new Cannon();
        tmp.amount = amount;
        if (startPos.X > endPos.X) 
        {
            if (startPos.Y > endPos.Y)
            tmp.angle = 270 - (float)Math.Atan2(Math.Abs(endPos.Y - startPos.Y), Math.Abs(endPos.X - startPos.X)) * (180f / (float)Math.PI); // <\
            else
            {
            tmp.angle = (float)Math.Atan2(Math.Abs(endPos.Y - startPos.Y), Math.Abs(endPos.X - startPos.X)) * (180f / (float)Math.PI) - 90; // </
            }
        }
        else
        {
            if(startPos.Y > endPos.Y)
            tmp.angle = (float)Math.Atan2(Math.Abs(endPos.Y - startPos.Y), Math.Abs(endPos.X - startPos.X)) * (180f / (float)Math.PI) + 90; // >/
            else
            {
            tmp.angle = 90 - (float)Math.Atan2(Math.Abs(endPos.Y - startPos.Y), Math.Abs(endPos.X - startPos.X)) * (180f / (float)Math.PI); // >\
            }
        }
        tmp.startColor = startcolor;
        tmp.endColor = endcolor;
        tmp.duration = duration;
        tmp.endPos = endPos;
        if(texture == null)
        tmp.sprite = pixelParticle;
        else
        {
            tmp.sprite = texture;
        }
        tmp.startPos = startPos;// +new Vector2(tmp.sprite.Width / 2, tmp.sprite.Height / 2);
        cannonList.Add(tmp);

     
    }

    public void update(GameTime gameTime)
    {
        List<Particle> removeParticle = new List<Particle>();
        List<Cannon> removeCannon = new List<Cannon>();
        foreach(Particle p in particleList)
        {
            if (p.fade && p.opacity < 1.0f && p.time_alive == 0)
            {
                p.opacity += p.opacity_change;
            }
            else
            {
                p.time_alive += gameTime.ElapsedGameTime.Milliseconds;
                if (p.time_alive >= p.time_duration)
                {
                    if (p.fade && p.opacity > 0.0f)
                        p.opacity -= p.opacity_change;
                    else
                        removeParticle.Add(p);
                }
            }
            if (p.currentColor.G < p.endColor.G)
                p.currentColor.G += 5;

            p.position += new Vector2((float)(Math.Sin(toRad(p.angle)) * p.speed), (float)(Math.Cos(toRad(p.angle)) * p.speed));
        }

        foreach(Particle p in removeParticle)
        {
            particleList.Remove(p);
        }

        foreach (Cannon c in cannonList)
        {
            if(c.isFiring(gameTime)){
                for (int i = 0; i < c.amount; i++)
                {
                    float tmpSpeed = (float)r.NextDouble() * 2.0f;
                    Color tmpColor = c.startColor;
                    //angle
                    //dit is voor vuur;
                    CreateParticle(c.sprite, c.startPos, c.duration, tmpSpeed, c.angle, (float)r.NextDouble(), tmpColor, c.endColor, true);
                }
            }
            else
            {
                removeCannon.Add(c);
            }
        }

        foreach (Cannon c in removeCannon)
        {
            cannonList.Remove(c);
        }
    }

    public void draw(SpriteBatch sb)
    {
        foreach (Particle p in particleList)
        {
            sb.Draw(p.sprite, p.position, null,p.currentColor * p.opacity, 0f, new Vector2(p.sprite.Width/2 * p.scale, p.sprite.Height/2 * p.scale), p.scale, SpriteEffects.None, 0.0f);// / GameEnvironment.Camera.Scale.M11);
        }
    }


    /// <summary>
    /// returns the degrees to radians
    /// </summary>
    /// <param name="degree"></param>
    /// <returns></returns>
    float toRad(float degree)
    {
        return degree / (180 / (float)Math.PI);
    }


    /*
     * Properties
     */
    public List<Particle> ParticleList
    {
        get { return particleList; }
    }


    /*
     * Strucs & Enums and Classes
     */

    public class Particle
    {
        public float time_alive;
        public float time_duration;
        public Vector2 position;
        public float angle;
        public float speed;
        public float scale;
        public Color startColor;
        public Color endColor;
        public Color currentColor;
        public Texture2D sprite;
        public bool fade;
        public float opacity = 1.0f;
        public float opacity_change = 0.05f;
    }

    public class Cannon
    {
        public int duration;
        public int timer = 0;
        public int amount;
        public Vector2 startPos;
        public Vector2 endPos;
        public float angle;
        public Texture2D sprite;
        public Color startColor;
        public Color endColor;

        public bool isFiring(GameTime gameTime)
        {
            timer += gameTime.ElapsedGameTime.Milliseconds;
            if (timer < duration)
            {
                return true;
            }
            return false;
        }
    }

}