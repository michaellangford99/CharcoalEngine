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
    public class InputMapping
    {
        public DrawingSystem input_system;
        public string input_texture_name;

        public InputMapping(DrawingSystem d, string n)
        {
            input_system = d;
            input_texture_name = n;
        }

        public RenderTarget2D Texture
        {
            get
            {
                if (input_system.NeedsDrawn())
                {
                    //input_system.Draw();
                    throw new InvalidOperationException();
                }
                return input_system.OutputMappings[input_texture_name];
            }
        }
    }

    public class DrawingSystem
    {
        //This is the only guaranteed global list
        public List<Transform> Items = new List<Transform>();

        private bool drawn = false;

        public Dictionary<string, InputMapping> InputMappings = new Dictionary<string, InputMapping>();
        public Dictionary<string, RenderTarget2D> OutputMappings = new Dictionary<string, RenderTarget2D>();
        
        public Viewport viewport;

        public DrawingSystem()
        {

        }

        public virtual void RegisterItem(Transform t)
        {
            Items.Add(t);
        }

        public virtual void ViewportChanged(Viewport v)
        {
            viewport = v;
        }

        public RenderTarget2D CreateStandardRenderTarget()
        {
            return new RenderTarget2D(Engine.g, viewport.Width, viewport.Height, false, SurfaceFormat.Vector4, DepthFormat.Depth24);
        }

        public void ResetForDraw()
        {
            drawn = false;
        }

        public bool NeedsDrawn()
        {
            return !drawn;
        }

        public void SetDrawn(bool is_drawn)
        {
            drawn = is_drawn;
        }

        public void DrawDependencies()
        {
            List<InputMapping> inputs = InputMappings.Values.ToList<InputMapping>();
            foreach (InputMapping i in inputs)
            {
                if (i.input_system.NeedsDrawn())
                {
                    i.input_system.DrawDependencies();
                    i.input_system.Draw();
                }
            }
        }

        public virtual void Draw()
        {
            drawn = true;
        }
    }
}
