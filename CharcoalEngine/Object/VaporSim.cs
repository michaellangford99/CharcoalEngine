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

namespace CharcoalEngine.Object
{
    class VaporSim2D : Transform
    {
        Effect effect;
        VertexPositionColor[] V;
        //density voxels per unit
        RenderTarget2D[] DensityMap = new RenderTarget2D[2];
        RenderTarget2D[] VelocityMap = new RenderTarget2D[2];

        //index of the maps being used as the reference frame
        int SourceIndex = 0;
        int DestinationIndex = 1;

        int Width;
        int Height;

        public float Brightness { get; set; }
        
        public VaporSim2D(int width, int height)
        {
            Width = width;
            Height = height;

            effect = Engine.Content.Load<Effect>("Effects/VaporSim");
            V = new VertexPositionColor[6];

            Random r = new Random();

            V[0] = new VertexPositionColor(new Vector3(-1, -1, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0));
            V[1] = new VertexPositionColor(new Vector3(-1, 1, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0));
            V[2] = new VertexPositionColor(new Vector3(1, 1, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0));
            V[3] = new VertexPositionColor(new Vector3(-1, -1, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0));
            V[4] = new VertexPositionColor(new Vector3(1, 1, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0));
            V[5] = new VertexPositionColor(new Vector3(1, -1, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0));
            
            DensityMap[0]  = new RenderTarget2D(Engine.g, Width, Height, false, SurfaceFormat.Vector4, DepthFormat.Depth24);
            DensityMap[1]  = new RenderTarget2D(Engine.g, Width, Height, false, SurfaceFormat.Vector4, DepthFormat.Depth24);
            VelocityMap[0] = new RenderTarget2D(Engine.g, Width, Height, false, SurfaceFormat.Vector4, DepthFormat.Depth24);
            VelocityMap[1] = new RenderTarget2D(Engine.g, Width, Height, false, SurfaceFormat.Vector4, DepthFormat.Depth24);
            
            Vector4[] vMapData = new Vector4[Width * Height];

            Vector4[] dMapData = new Vector4[Width * Height];
                        
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    Vector3 Up = Vector3.Backward;//lol
                    Vector3 Ray = new Vector3((float)i - (float)Width / 2, (float)j - (float)Height / 2, 0);
                    Vector3 Out = Vector3.Cross(Ray, Up)*0.5f*(float)Math.Sqrt(Math.Pow((float)i - (float)Width / 2, 2) + Math.Pow((float)j - (float)Height / 2, 2));
                    //Vector3 Rand = new Vector3()
                    vMapData[i + j * Width] =  new Vector4(Out, 1.0f)/(Height*(float)Math.Sqrt(2));

                    dMapData[i + j * Width] = new Vector4(j%(Width/4) >= Width/8 ? 1.0f : 0.0f, 0, 0, 0);
                }
            }

            VelocityMap[SourceIndex].SetData(vMapData, 0, Width * Height);
            DensityMap[SourceIndex].SetData(dMapData, 0, Width * Height);
        }
        
        public override void Draw()
        {
            Engine.g.SetRenderTargets(DensityMap[DestinationIndex], VelocityMap[DestinationIndex]);

            //effect.Parameters["w"].SetValue((float)DensityMap[DestinationIndex].Width);
            //effect.Parameters["h"].SetValue((float)DensityMap[DestinationIndex].Height);
            //effect.Parameters["BackgroundColor"].SetValue(Color.Black.ToVector3());
            effect.Parameters["DensityMap"].SetValue(DensityMap[SourceIndex]);
            effect.Parameters["VelocityMap"].SetValue(VelocityMap[SourceIndex]);
            effect.Parameters["Width"].SetValue(Width);
            effect.Parameters["Height"].SetValue(Width);
            effect.Parameters["Brightness"].SetValue(Brightness);

            effect.CurrentTechnique.Passes[0].Apply();

            Engine.g.DrawUserPrimitives(PrimitiveType.TriangleList, V, 0, 2);

            Engine.g.SetRenderTargets(null);

            SpriteBatch s = new SpriteBatch(Engine.g);
            s.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.DepthRead);
            s.Draw(DensityMap[DestinationIndex], new Rectangle(0, 0, Camera.Viewport.Height, Camera.Viewport.Height), Color.White);
            s.Draw(VelocityMap[DestinationIndex], new Rectangle(Camera.Viewport.Height, 0, Camera.Viewport.Height, Camera.Viewport.Height), Color.White);
            s.End();

            //End - swap dest and src
            int temp = DestinationIndex;
            DestinationIndex = SourceIndex;
            SourceIndex = temp;

            base.Draw();
        }
    }
}
