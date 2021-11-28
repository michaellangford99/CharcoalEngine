﻿using System;
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
    /// <summary>
    /// This is for objects that are defining their own drawing process while in development or for a specific design reason
    /// </summary>
    class CustomFXVertexDrawingSystem : DrawingSystem
    {
        RenderTarget2D Output;

        public CustomFXVertexDrawingSystem(Viewport v)
        {
            viewport = v;

            Output = CreateStandardRenderTarget();

            OutputMappings.Add("Output", Output);
        }

        public override void Draw()
        {
            Engine.g.SetRenderTarget(Output);

            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].Draw();
            }

            Engine.g.SetRenderTarget(null);

            base.Draw();
        }
    }
}
