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
    class GBufferRenderer : DrawingSystem
    {
        Effect effect;

        //render targets for the GBuffer
        //world normal
        //luminance
        //specular power
        //specular intnesity
        //ambient color
        //depth
        // i think world space position can be recovered from depth, camera position, scene near and far clip, and screen space position (and at higher precision)

        RenderTarget2D NormalMap;
        //RenderTarget2D TangentMap;
        RenderTarget2D DiffuseMap;
        RenderTarget2D DepthMap;
        RenderTarget2D LuminanceMap;
        RenderTarget2D SpecularMap;

        public GBufferRenderer(Viewport v)
        {
            viewport = v;

            //effect = Engine.Content.Load<Effect>("Effects/GBuffer");
            effect = Engine.Content.Load<Effect>("Effects/NDT_Effect");

            NormalMap = new RenderTarget2D(Engine.g, v.Width, v.Height, false, SurfaceFormat.Vector4, DepthFormat.Depth24);
            DiffuseMap = new RenderTarget2D(Engine.g, v.Width, v.Height, false, SurfaceFormat.Vector4, DepthFormat.Depth24);
            DepthMap = new RenderTarget2D(Engine.g, v.Width, v.Height, false, SurfaceFormat.Vector4, DepthFormat.Depth24);
            LuminanceMap = new RenderTarget2D(Engine.g, v.Width, v.Height, false, SurfaceFormat.Vector4, DepthFormat.Depth24);
            SpecularMap = new RenderTarget2D(Engine.g, v.Width, v.Height, false, SurfaceFormat.Vector4, DepthFormat.Depth24);

            //set outputs
            /*OutputMappings.Add("NormalMap", NormalMap);
            OutputMappings.Add("DiffuseMap", DiffuseMap);
            OutputMappings.Add("DepthMap", DepthMap);
            OutputMappings.Add("LuminanceMap", LuminanceMap);
            OutputMappings.Add("SpecularMap", SpecularMap);*/
        }

        public void ViewportChanged(Viewport v)
        {
            viewport = v;

            /*NormalMap = new RenderTarget2D(Engine.g, v.Width, v.Height, false, SurfaceFormat.Vector4, DepthFormat.Depth24);
            DiffuseMap = new RenderTarget2D(Engine.g, v.Width, v.Height, false, SurfaceFormat.Vector4, DepthFormat.Depth24);
            DepthMap = new RenderTarget2D(Engine.g, v.Width, v.Height, false, SurfaceFormat.Vector4, DepthFormat.Depth24);
            LuminanceMap = new RenderTarget2D(Engine.g, v.Width, v.Height, false, SurfaceFormat.Vector4, DepthFormat.Depth24);
            SpecularMap = new RenderTarget2D(Engine.g, v.Width, v.Height, false, SurfaceFormat.Vector4, DepthFormat.Depth24);*/
            //base.ViewportChanged(v);
        }

        public void Draw()
        {
            Engine.g.BlendState = BlendState.AlphaBlend;
            Engine.g.DepthStencilState = DepthStencilState.Default;
            Engine.g.SamplerStates[0] = SamplerState.LinearWrap;
            Engine.graphics.PreferMultiSampling = true;

            Engine.g.SetRenderTargets(NormalMap, DepthMap, DiffuseMap/*, LuminanceMap, SpecularMap*/);

            //set render targets
            //set effect with necessary camera information

            //LOL only draw meshes. what a savage
            /*foreach (Mesh m in Items)
            {
                m.GbufferDraw(effect);
            }*/

            effect.Parameters["NearPlane"].SetValue(viewport.MinDepth);
            effect.Parameters["FarPlane"].SetValue(viewport.MaxDepth);

            //loop through each object handled by this drawing system
            //apply any material information to the effect
            //normal maps
            //height maps (parallax mapping)
            //specular maps
            //texture maps
            //etc..

            //render
            NormalMap.Name = "NormalMap";
            Engine.g.SetRenderTargets(null);

            //temporary for debugging:
            /*
            SpriteBatch s = new SpriteBatch(Engine.g);
            s.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.DepthRead);
            s.Draw(DiffuseMap, Engine.g.Viewport.Bounds, Color.White);
            s.End();*/
        }
    }
}
