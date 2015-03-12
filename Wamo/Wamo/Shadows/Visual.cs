using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


    public class Visual
    {
        /// <summary>
        /// The texture, or color of the object
        /// </summary>
        public Texture2D Texture { get; set; }

        /// <summary>
        /// The glow, or light emitted by the object
        /// can be null
        /// </summary>
        public Texture2D Glow { get; set; }

        /// <summary>
        /// The position, scale and rotation of the object
        /// </summary>
        public Pose2D Pose { get; set; }

        public Visual(Texture2D texture, Pose2D pose)
            : this(texture, pose, null) { }

        public Visual(Texture2D texture, Vector2 position, float rotation)
            : this(texture, new Pose2D(position, rotation)) { }

        public Visual(Texture2D texture, Vector2 position, float rotation, Texture2D glow)
            : this(texture, new Pose2D(position, rotation), glow) { }

        public Visual(Texture2D texture, Pose2D pose, Texture2D glow)
        {
            this.Texture = texture;
            this.Pose = pose;
            this.Glow = glow;
        }
    }

