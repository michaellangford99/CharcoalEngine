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
using CharcoalEngine.Utilities;

namespace CharcoalEngine.Scene
{
    public class Scene
    {
        public Transform Root = new Transform();
        
        GraphicsDevice g;
        SpriteBatch spriteBatch;

        GizmoComponent _gizmo;

        OutputDrawingSystem output;
        PointLightRenderer point_light_renderer;
        SSAORenderer ssao_renderer;
        SSReflectionRenderer ssreflection_renderer;

        Effect gbuffereffect;

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

        public Scene()
        {
            Engine.Game.Window.AllowUserResizing = true;
            Engine.Game.IsMouseVisible = true;

            g = Engine.g;
            spriteBatch = new SpriteBatch(g);
            
            init_camera();
            init_gizmo();
            init_gbuffer();
            init_output();
            
            OBJModel obj = new OBJModel(UserUtilities.OpenFile(UserUtilities.OBJ_FILTER), Vector3.Zero, Vector3.Zero, 1.0f, false);
            Root.Children.Add(obj);
            Root.Update();

            Random r = new Random();
            
            for (int i = 0; i < 10; i++)
            {
                addlight(new Color(new Vector3((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble())), new Vector3(-5 + i, 0, 3));
            }
        }

        public void addlight(Color c, Vector3 p)
        {
            PointLight p1 = new PointLight() { Position = p, Color = c.ToVector3() };
            Root.Children.Add(p1);
            point_light_renderer.lights.Add(p1);
        }

        public void init_output()
        {
            output = new OutputDrawingSystem();
            output.Inputs.Add(NormalMap);
            output.Inputs.Add(DiffuseMap);
            output.Inputs.Add(DepthMap);
            //output.Inputs.Add(LuminanceMap);
            //output.Inputs.Add(SpecularMap);

            point_light_renderer = new PointLightRenderer(Camera.Viewport);
            output.Inputs.Add(point_light_renderer.Output);

            ssao_renderer = new SSAORenderer(Camera.Viewport);
            output.Inputs.Add(ssao_renderer.Output);

            ssreflection_renderer = new SSReflectionRenderer(Camera.Viewport);
            output.Inputs.Add(ssreflection_renderer.Output);
        }

        public void init_camera()
        {
            Camera.Initialize_WithDefaults();
        }

        public void init_gizmo()
        {
            _gizmo = new GizmoComponent(g, spriteBatch, Engine.Content.Load<SpriteFont>("Fonts/Font"));
            _gizmo.SetSelectionPool(Root.Children, Root);
            _gizmo.RotateEvent += _gizmo_RotateEvent;
        }

        public void init_gbuffer()
        {
            gbuffereffect = Engine.Content.Load<Effect>("Effects/NDT_Effect");

            NormalMap = new RenderTarget2D(Engine.g, Camera.Viewport.Width, Camera.Viewport.Height, false, SurfaceFormat.Vector4, DepthFormat.Depth24);
            DiffuseMap = new RenderTarget2D(Engine.g, Camera.Viewport.Width, Camera.Viewport.Height, false, SurfaceFormat.Vector4, DepthFormat.Depth24);
            DepthMap = new RenderTarget2D(Engine.g, Camera.Viewport.Width, Camera.Viewport.Height, false, SurfaceFormat.Single, DepthFormat.Depth24);
            LuminanceMap = new RenderTarget2D(Engine.g, Camera.Viewport.Width, Camera.Viewport.Height, false, SurfaceFormat.Vector4, DepthFormat.Depth24);
            SpecularMap = new RenderTarget2D(Engine.g, Camera.Viewport.Width, Camera.Viewport.Height, false, SurfaceFormat.Vector4, DepthFormat.Depth24);
        }

        private void _gizmo_RotateEvent(Transform transformable, TransformationEventArgs e, TransformationEventArgs d)
        {
            _gizmo.RotationHelper(transformable, e, d);
        }
        
        private KeyboardState _previousKeys;
        private MouseState _previousMouse;
        private MouseState _currentMouse;
        private KeyboardState _currentKeys;

        public void Update(GameTime gameTime)
        {
            Engine.gameTime = gameTime;

            _currentMouse = Mouse.GetState();
            _currentKeys = Keyboard.GetState();
            // select entities with your cursor (add the desired keys for add-to / remove-from -selection)
            if (_currentMouse.RightButton == ButtonState.Pressed && _previousMouse.RightButton == ButtonState.Released)
                _gizmo.SelectEntities(new Vector2(_currentMouse.X, _currentMouse.Y),
                                      _currentKeys.IsKeyDown(Keys.LeftControl) || _currentKeys.IsKeyDown(Keys.RightControl),
                                      _currentKeys.IsKeyDown(Keys.LeftAlt) || _currentKeys.IsKeyDown(Keys.RightAlt));
                
            // set the active mode like translate or rotate
            if (IsNewButtonPress(Keys.D1))
                _gizmo.ActiveMode = GizmoMode.Translate;
            if (IsNewButtonPress(Keys.D2))
                _gizmo.ActiveMode = GizmoMode.Rotate;
            if (IsNewButtonPress(Keys.D3))
                _gizmo.ActiveMode = GizmoMode.NonUniformScale;
            if (IsNewButtonPress(Keys.D4))
                _gizmo.ActiveMode = GizmoMode.UniformScale;
            if (IsNewButtonPress(Keys.D5))
                _gizmo.ActiveMode = GizmoMode.CenterTranslate;

            if (IsNewButtonPress(Keys.Enter))
                _gizmo.LevelDown();
            if (IsNewButtonPress(Keys.Back))
                _gizmo.LevelUp();

            // toggle precision mode
            if (_currentKeys.IsKeyDown(Keys.LeftShift) || _currentKeys.IsKeyDown(Keys.RightShift))
                _gizmo.PrecisionModeEnabled = true;
            else
                _gizmo.PrecisionModeEnabled = false;
            
            // toggle snapping
            if (IsNewButtonPress(Keys.I))
                _gizmo.SnapEnabled = !_gizmo.SnapEnabled;
            
            // clear selection
            if (IsNewButtonPress(Keys.Escape))
                _gizmo.Clear();

            if (IsNewButtonPress(Keys.Q)) output.ActiveInput++;
            if (IsNewButtonPress(Keys.W)) output.ActiveInput--;

            _gizmo.Update(gameTime);

            _previousKeys = _currentKeys;
            _previousMouse = _currentMouse;

            Root.Update();
        }

        private bool IsNewButtonPress(Keys key)
        {
            return _currentKeys.IsKeyDown(key) && _previousKeys.IsKeyUp(key);
        }

        public void Draw()
        {
            draw_gbuffer();
            draw_point_lights();
            draw_ssao();
            draw_ssreflection();
            draw_outputs();
            draw_debug();
            draw_gizmo();
        }

        public void draw_gbuffer_mesh(Mesh m)
        {
            gbuffereffect.Parameters["World"].SetValue(m.AbsoluteWorld);
            gbuffereffect.Parameters["View"].SetValue(Camera.View);
            gbuffereffect.Parameters["Projection"].SetValue(Camera.Projection);
            gbuffereffect.Parameters["BasicTexture"].SetValue(m.Material.Texture);
            gbuffereffect.Parameters["TextureEnabled"].SetValue(m.Material._TextureEnabled);
            gbuffereffect.Parameters["NormalMap"].SetValue(m.Material.NormalMap);
            gbuffereffect.Parameters["NormalMapEnabled"].SetValue(m.Material.NormalMapEnabled);
            gbuffereffect.Parameters["DiffuseColor"].SetValue(m.Material.DiffuseColor);
            gbuffereffect.Parameters["Alpha"].SetValue(m.Material.Alpha);
            gbuffereffect.Parameters["AlphaEnabled"].SetValue(m.Material.AlphaEnabled);
            gbuffereffect.Parameters["AlphaMaskEnabled"].SetValue(m.Material.AlphaMaskEnabled);
            gbuffereffect.Parameters["AlphaMask"].SetValue(m.Material.AlphaMask);

            //...
            gbuffereffect.CurrentTechnique.Passes[0].Apply();
            gbuffereffect.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
            Engine.g.DrawUserPrimitives(PrimitiveType.TriangleList, m.V, 0, m.Faces.Count);
        }

        public void draw_gbuffer_recursive(Transform t)
        {
            if (t is Mesh)
                ((Mesh)t).GbufferDraw(gbuffereffect);
            foreach (Transform m in t.Children)
            {
                draw_gbuffer_recursive(m);
            }
        }

        public void draw_gbuffer()
        {
            Engine.g.BlendState = BlendState.AlphaBlend;
            Engine.g.DepthStencilState = DepthStencilState.Default;
            Engine.g.SamplerStates[0] = SamplerState.LinearWrap;
            Engine.graphics.PreferMultiSampling = true;

            Engine.g.SetRenderTargets(NormalMap, DepthMap, DiffuseMap/*, LuminanceMap, SpecularMap*/);

            //set render targets
            //set effect with necessary camera information

            //LOL only draw meshes. what a savage
            draw_gbuffer_recursive(Root);
            
            gbuffereffect.Parameters["NearPlane"].SetValue(Camera.Viewport.MinDepth);
            gbuffereffect.Parameters["FarPlane"].SetValue(Camera.Viewport.MaxDepth);

            //loop through each object handled by this drawing system
            //apply any material information to the effect
            //normal maps
            //height maps (parallax mapping)
            //specular maps
            //texture maps
            //etc..
            Engine.g.SetRenderTargets(null);
        }

        public void draw_point_lights()
        {
            point_light_renderer.Draw(NormalMap, DepthMap, DiffuseMap);
        }

        public void draw_ssao()
        {
            ssao_renderer.Draw(NormalMap, DepthMap, DiffuseMap);
        }

        public void draw_ssreflection()
        {
            ssreflection_renderer.Draw(NormalMap, DepthMap, DiffuseMap);
        }

        public void draw_outputs()
        {
            output.Draw();
        }

        public void draw_debug()
        {
            Root.DrawDebugMode();
        }

        public void draw_gizmo()
        {
            #region setup
            g.BlendState = BlendState.Opaque;
            g.DepthStencilState = DepthStencilState.Default;
            g.SamplerStates[0] = SamplerState.LinearWrap;
            //g.DepthStencilState = DepthStencilState.Default;
            #endregion

            _gizmo.Draw();
        }
    }
}