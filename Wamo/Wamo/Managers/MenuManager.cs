using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

public class MenuManager
{
    private ContentManager content;
    private FileManager fileManager;

    List<string> menuItems;
    List<Texture2D> menuImages;
    List<List<Animation>> animation;
    List<Animation> tmpAnimation;
    List<string> animationTypes, linkType, linkID;
    int itemNumber;

    List<List<string>> attributes, contents;

    SpriteFont font;
    Vector2 position;
    Rectangle source;
    int axis;
    string align;

    public void LoadContent(ContentManager content)
    {
        this.content = new ContentManager(content.ServiceProvider, "Content");
        menuItems = new List<string>();
        menuImages = new List<Texture2D>();
        animation = new List<List<Animation>>();
        animationTypes = new List<string>();
        attributes = new List<List<string>>();
        contents = new List<List<string>>();
        linkType = new List<string>();
        linkID = new List<string>();
        position = Vector2.Zero;
        source = new Rectangle();
        itemNumber = 0;

        fileManager = new FileManager();
        fileManager.LoadContent("Load/menu.cme", attributes, contents);
        for (int i = 0; i < attributes.Count; i++)
        {
            for (int j = 0; j < attributes[i].Count; j++)
            {
                switch(attributes[i][j])
                {
                    case "Font": font = content.Load<SpriteFont>(contents[i][j]);
                        break;
                    case "Item": menuItems.Add(contents[i][j]);
                        break;
                    case "Image": menuImages.Add(content.Load<Texture2D>(contents[i][j]));
                        break;
                    case "Axis": axis = int.Parse(contents[i][j]);
                        break;
                    case "Animation": animationTypes.Add(contents[i][j]);
                        break;
                    case "Align": align = contents[i][j];
                        break;
                    case "LinkType": linkType.Add(contents[i][j]);
                        break;
                    case "LinkID": linkID.Add(contents[i][j]);
                        break;
                    case "Position": 
                        {
                            string[] tmp = contents[i][j].Split(' ');
                            position = new Vector2(float.Parse(tmp[0]), float.Parse(tmp[1]));
                        }
                        break;
                    case "Source":
                        {
                            string[] tmp = contents[i][j].Split(' ');
                            source = new Rectangle(int.Parse(tmp[0]), int.Parse(tmp[1]), int.Parse(tmp[2]), int.Parse(tmp[3]));
                        }
                        break;

                }
            }   
        }

        SetMenuItems();
        SetAnimations();
    }

    public void UnloadContent()
    {
        content.Unload();
        position = Vector2.Zero;
        animation.Clear();
        animationTypes.Clear();
        menuImages.Clear();
        menuItems.Clear();
        fileManager = null;
    }

    public void Update(GameTime gameTime, InputManager inputManager)
    {
        if(axis == 1) 
        {
            if(inputManager.KeyPressed(Keys.Right, Keys.D)) itemNumber++;
            else if(inputManager.KeyPressed(Keys.Left, Keys.A)) itemNumber--;
        }
        else
        {
            if(inputManager.KeyPressed(Keys.Down, Keys.S)) itemNumber++;
            else if(inputManager.KeyPressed(Keys.Up, Keys.W)) itemNumber--;
        }

        if(inputManager.KeyPressed(Keys.Enter))
        {
            if (linkType[itemNumber] == "Screen")
            {
                Type newClass = getTypeByName(linkID[itemNumber])[0];
                ScreenManager.Instance.AddScreen((GameScreen)Activator.CreateInstance(newClass), inputManager);
            }
        }

        //itemNumber = (itemNumber < 0) ? 0 : ((itemNumber > menuItems.Count - 1) ? menuItems.Count - 1 : itemNumber); //Normal
        itemNumber = (itemNumber < 0) ? menuItems.Count - 1 : ((itemNumber > menuItems.Count - 1) ? 0 : itemNumber); //Carousell

        for (int i = 0; i < animation.Count; i++)
            for (int j = 0; j < animation[i].Count; j++)
            {
                if (itemNumber == i) animation[i][j].IsActive = true;
                else animation[i][j].IsActive = false;

                animation[i][j].Update(gameTime);
            }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        for (int i = 0; i < animation.Count; i++)
            for (int j = 0; j < animation[i].Count; j++)
                animation[i][j].Draw(spriteBatch);
    }

    private void SetMenuItems()
    {
        for (int i = 0; i < menuItems.Count; i++)
            if (menuImages.Count == i) menuImages.Add(ScreenManager.Instance.NullImage);

        for (int i = 0; i < menuImages.Count; i++)
            if (menuItems.Count == i) menuItems.Add(null);
    }


    private void SetAnimations()
    {
        Vector2 pos = position;
        Vector2 dimensions = Vector2.Zero;
        tmpAnimation = new List<Animation>();

        if (align.Contains("Center"))
        {
            for (int i = 0; i < menuItems.Count; i++)
            {
                dimensions.X += font.MeasureString(menuItems[i]).X + menuImages[i].Width;
                dimensions.Y += font.MeasureString(menuItems[i]).Y + menuImages[i].Height;
            }

            if (axis == 1) pos.X = (ScreenManager.Instance.Dimensions.X - dimensions.X) / 2;
            else pos.Y = (ScreenManager.Instance.Dimensions.Y - dimensions.Y) / 2;
            
        }
        else pos = position;

        for (int i = 0; i < menuItems.Count; i++)
        {
            dimensions = new Vector2(font.MeasureString(menuItems[i]).X + menuImages[i].Width, font.MeasureString(menuItems[i]).Y + menuImages[i].Height);

            if (axis == 1) pos.Y = (ScreenManager.Instance.Dimensions.Y - dimensions.Y) / 2;
            else pos.X = (ScreenManager.Instance.Dimensions.X - dimensions.X) / 2;

            for (int j = 0; j < animationTypes.Count; j++)
            {
                switch (animationTypes[j])
                {
                    case "Fade": 
                        tmpAnimation.Add(new FadeAnimation());
                        tmpAnimation[tmpAnimation.Count - 1].LoadContent(content, menuImages[i], menuItems[i], pos);
                        tmpAnimation[tmpAnimation.Count - 1].Font = font;
                        break;
                }
            }

            if(tmpAnimation.Count > 0) animation.Add(tmpAnimation);
            tmpAnimation = new List<Animation>();
            
            if(axis == 1) pos.X += dimensions.X;
            else pos.Y += dimensions.Y;
            
        }
    }

    private static Type[] getTypeByName(string className)
    {
        List<Type> returnVal = new List<Type>();

        foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] assemblyTypes = a.GetTypes();
            for (int j = 0; j < assemblyTypes.Length; j++)
            {
                if (assemblyTypes[j].Name == className)
                {
                    returnVal.Add(assemblyTypes[j]);
                }
            }
        }

        return returnVal.ToArray();
    }

}
