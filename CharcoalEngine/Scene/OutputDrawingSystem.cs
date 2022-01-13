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
    class OutputDrawingSystem
    {
        public List<RenderTarget2D> Inputs = new List<RenderTarget2D>();

        public int ActiveInput {
            get
            {
                return _active_input;
            }
            set
            {
                if ((value >= 0) && (value < Inputs.Count))
                    _active_input = value;
            }
        }

        int _active_input = 0;

        public OutputDrawingSystem()
        {
            
        }

        public void Draw()
        {
            if (Inputs.Count == 0) return;

            Engine.g.BlendState = BlendState.AlphaBlend;
            Engine.g.DepthStencilState = DepthStencilState.Default;
            Engine.g.SamplerStates[0] = SamplerState.LinearWrap;
            Engine.graphics.PreferMultiSampling = true;
            
            Engine.g.SetRenderTarget(null);

            SpriteBatch s = new SpriteBatch(Engine.g);
            s.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.DepthRead);

            s.Draw(Inputs[ActiveInput], Engine.g.Viewport.Bounds, Color.White);
            
            s.End();
            s.Dispose();
        }
    }
}
