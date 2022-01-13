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

    class SSAORenderer : DrawingSystem
    {
        public RenderTarget2D Output;
        Effect effect;
        VertexPositionColor[] V;

        public SSAORenderer(Viewport v)
        {
            viewport = v;

            Output = CreateStandardRenderTarget();

            effect = Engine.Content.Load<Effect>("Effects/SSAOEffect");
            V = new VertexPositionColor[6];

            Random r = new Random();

            V[0] = new VertexPositionColor(new Vector3(-1, -1, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0));
            V[1] = new VertexPositionColor(new Vector3(-1, 1, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0));
            V[2] = new VertexPositionColor(new Vector3(1, 1, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0));
            V[3] = new VertexPositionColor(new Vector3(-1, -1, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0));
            V[4] = new VertexPositionColor(new Vector3(1, 1, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0));
            V[5] = new VertexPositionColor(new Vector3(1, -1, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0));
        }

        public void Draw(RenderTarget2D Normal, RenderTarget2D Depth, RenderTarget2D Diffuse)
        {
            Engine.g.SetRenderTarget(Output);

            effect.Parameters["w"].SetValue((float)Camera.Viewport.Width);
            effect.Parameters["h"].SetValue((float)Camera.Viewport.Height);
            effect.Parameters["ViewProjection"].SetValue(Camera.View * Camera.Projection);
            effect.Parameters["InverseViewProjection"].SetValue(Matrix.Invert(Camera.View * Camera.Projection));
            effect.Parameters["InverseView"].SetValue(Matrix.Invert(Camera.View));
            effect.Parameters["InverseProjection"].SetValue(Matrix.Invert(Camera.Projection));
            effect.Parameters["NearClip"].SetValue(Camera.Viewport.MinDepth);
            effect.Parameters["FarClip"].SetValue(Camera.Viewport.MaxDepth);
            effect.Parameters["CameraPosition"].SetValue(Camera.Position);

            effect.Parameters["NormalMap"].SetValue(Normal);
            effect.Parameters["DepthMap"].SetValue(Depth);
            //effect.Parameters["DiffuseMap"].SetValue(Diffuse);

            effect.CurrentTechnique.Passes[0].Apply();

            Engine.g.DrawUserPrimitives(PrimitiveType.TriangleList, V, 0, 2);

            Engine.g.SetRenderTarget(null);
        }
    }
}
