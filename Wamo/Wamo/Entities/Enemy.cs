﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

public class Enemy : Entity
{
    public override void LoadContent(ContentManager content, InputManager inputManager)
    {
        base.LoadContent(content, inputManager);
    }

    public override void UnloadContent()
    {
        base.UnloadContent();
    }

    public override void Update(GameTime gameTime, InputManager inputManager)
    {
        base.Update(gameTime, inputManager);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
    }
}
