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
    class DebugOverlayDrawingSystem : DrawingSystem
    {
        RenderTarget2D Output;

        public DebugOverlayDrawingSystem(Viewport v, List<Transform> Nodes)
        {
            viewport = v;

            Output = new RenderTarget2D(Engine.g, v.Width, v.Height, false, SurfaceFormat.Vector4, DepthFormat.Depth24);
        }

        public void Draw()
        {
            Engine.g.BlendState = BlendState.AlphaBlend;
            Engine.g.DepthStencilState = DepthStencilState.Default;
            Engine.g.SamplerStates[0] = SamplerState.LinearWrap;
            Engine.graphics.PreferMultiSampling = true;

            Engine.g.SetRenderTarget(Output);
            
            SpriteBatch s = new SpriteBatch(Engine.g);
            s.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.DepthRead);
            

            s.End();

            Engine.g.SetRenderTarget(null);
        }
    }
}
