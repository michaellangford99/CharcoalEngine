using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.IO;
using CharcoalEngine.Scene;
using CharcoalEngine;
using CharcoalEngine.Utilities;
using Jitter;
using Jitter.Collision;
using Jitter.Collision.Shapes;
using Jitter.DataStructures;
using Jitter.Dynamics;
using Jitter.LinearMath;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Windows;
using System.Design;
using CharcoalEngine.Utilities.MapGeneration;
using CharcoalEngine.Object;

namespace CharcoalEngine.Scene
{
    class OutputDrawingSystem : DrawingSystem
    {
        public OutputDrawingSystem()
        {
            
        }

        public override void Draw()
        {
            Engine.g.BlendState = BlendState.AlphaBlend;
            Engine.g.DepthStencilState = DepthStencilState.Default;
            Engine.g.SamplerStates[0] = SamplerState.LinearWrap;
            Engine.graphics.PreferMultiSampling = true;

            int targets = 2;

            Engine.g.SetRenderTarget(null);

            SpriteBatch s = new SpriteBatch(Engine.g);
            s.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.DepthRead);

            List<InputMapping> inputs = InputMappings.Values.ToList<InputMapping>();

            for (int i = 0; i < inputs.Count; i++)
            {
                Rectangle r = Engine.g.Viewport.Bounds;
                r.Width /= targets;
                r.Height /= targets;
                r.X += r.Width * (i % targets);
                r.Y += r.Height * (i / targets);
                s.Draw(inputs[i].Texture, r, Color.White);
            }
            
            s.End();
            s.Dispose();

            base.Draw();
        }
    }
}
