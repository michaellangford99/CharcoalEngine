using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using CharcoalEngine.Object;
using CharcoalEngine.Editing;
using Jitter;
using Jitter.Collision;
using Jitter.Collision.Shapes;
using Jitter.DataStructures;
using Jitter.Dynamics;
using Jitter.LinearMath;

namespace CharcoalEngine.Scene
{
    public class DrawingSystem
    {
        public Viewport viewport;
        
        //public Dictionary<string, object> EffectParameters { get; set; }

        public DrawingSystem()
        {

        }


        
        public RenderTarget2D CreateStandardRenderTarget()
        {
            return new RenderTarget2D(Engine.g, viewport.Width, viewport.Height, false, SurfaceFormat.Vector4, DepthFormat.Depth24);
        }
        
    }
}
