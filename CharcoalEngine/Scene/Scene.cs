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

        public List<DrawingSystem> DrawingSystems = new List<DrawingSystem>();



        GraphicsDevice g;
        SpriteBatch spriteBatch;

        GizmoComponent _gizmo;

        OutputDrawingSystem output;

        public Scene()
        {
            //Engine.Game.Window.AllowUserResizing = true;
            Engine.Game.IsMouseVisible = true;

            g = Engine.g;
            spriteBatch = new SpriteBatch(g);

            Engine.Game.Window.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);

            Camera.Initialize_WithDefaults();

            _gizmo = new GizmoComponent(g, spriteBatch, Engine.Content.Load<SpriteFont>("Fonts/Font"));
            _gizmo.SetSelectionPool(Root.Children, Root);

            //_gizmo.TranslateEvent += _gizmo_TranslateEvent;
            _gizmo.RotateEvent += _gizmo_RotateEvent;
            //_gizmo.ScaleEvent += _gizmo_ScaleEvent;


            Root.Update();
            /*DrawingSystems.Add(new RayMarching());


            Root.Children.Add(new Sphere());     
            ((RayMarching)DrawingSystems[0]).RegisterItem(Root.Children[0]);
            Root.Children.Add(new Sphere());
            ((RayMarching)DrawingSystems[0]).RegisterItem(Root.Children[1]);*/

            // DrawingSystems.Add(new CustomFXVertexDrawingSystem());
            // Root.Children.Add(new RayTracing());
            //Root.Children.Add(new VaporSim2D(1000, 1000/*g.Viewport.Height, g.Viewport.Height*/));
            //Root.Children.Add(new VaporTracing());
            // DrawingSystems[0].RegisterItem(Root.Children[0]);
            
            GBufferRenderer gb = new GBufferRenderer(Camera.Viewport);
            DrawingSystems.Add(gb);

            /*CustomFXVertexDrawingSystem custom = new CustomFXVertexDrawingSystem(Camera.Viewport);
            DrawingSystems.Add(custom);

            Root.Children.Add(new VaporTracing());
            custom.RegisterItem(Root.Children[0]);*/
            
            GBufferReliantDrawingSystem gbds = new GBufferReliantDrawingSystem(Camera.Viewport);
            gbds.InputMappings.Add("Normal", new InputMapping(gb, "NormalMap"));
            gbds.InputMappings.Add("Diffuse", new InputMapping(gb, "DiffuseMap"));
            gbds.InputMappings.Add("Depth", new InputMapping(gb, "DepthMap"));
            DrawingSystems.Add(gbds);

            output = new OutputDrawingSystem();
            DrawingSystems.Add(output);
            output.InputMappings.Add("NormalMap", new InputMapping(gb, "NormalMap"));
            output.InputMappings.Add("DiffuseMap", new InputMapping(gb, "DiffuseMap"));
            output.InputMappings.Add("DepthMap", new InputMapping(gb, "DepthMap"));
            output.InputMappings.Add("Vapor", new InputMapping(gbds, "Output"));

            OBJModel obj = new OBJModel(UserUtilities.OpenFile(UserUtilities.OBJ_FILTER), gb, Vector3.Zero, Vector3.Zero, 1.0f, false);
            Root.Children.Add(obj);
            Root.Update();
        }

        private void _gizmo_RotateEvent(Transform transformable, TransformationEventArgs e, TransformationEventArgs d)
        {
            _gizmo.RotationHelper(transformable, e, d);
        }

        /// <summary>
        /// not used yet
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            //Camera.Viewport.Bounds = g.PresentationParameters.Bounds;
            //notify all draw systems of window size change
            /*for (int i = 0; i < DrawingSystems.Count; i++)
            {
                DrawingSystems[i].ViewportChanged(Camera.Viewport);
            }*/
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
            #region setup
            g.BlendState = BlendState.AlphaBlend;
            g.DepthStencilState = DepthStencilState.Default;
            g.SamplerStates[0] = SamplerState.LinearWrap;
            Engine.graphics.PreferMultiSampling = true;
            //g.DepthStencilState = DepthStencilState.Default;
            //RasterizerState r = new RasterizerState();
            //r.MultiSampleAntiAlias = true;
            //g.RasterizerState = r;

            #endregion

            //Set the active scene:
            Engine.ActiveScene = this;

            g.Clear(Color.Black);

            /*for (int i = 0; i < Root.Children.Count; i++)
            {
                Root.Children[i].Draw();
            }*/

            //go through and draw each system and at each system, draw its dependants. if those have dependants, draw them.
            //if the system already has output, then dont redraw it, but route its outputs to the necessary inputs
            //each system aught to have a hash id
            //then a system can have a list of those that it depends on (their ids)
            //dont do indexes because if they get rearranged it'll get all screwed up
            //or give them instance numbers.
            
            //each system has a list of its dependants
            //and for each dependant the names of the texture that it needs
            //every system has a list of its output textures along with their names
            //texture set input, textureset output, mapping from inputs to outpus entitling the hash and name of input texture

            ///
            ///input_mapping {
            ///string drawing_system_input_hashcode;
            ///string texture_input_name;
            ///}
            ///
            /// list<input_mapping> inputs
            /// 
            /// texture_set {
            /// string name;
            /// RenderTarget2D texture;
            /// 
            /// internal get set operations for getting setting by name
            /// 
            /// of course, perform get/set as few times as possible and retain target in local variable
            /// 
            /// }
            ///

            //idk

            for (int i = 0; i < DrawingSystems.Count; i++)
            {
                //reset flag keeping track of wether a dependant system already forced a draw
                DrawingSystems[i].ResetForDraw();
            }

            for (int i = 0; i < DrawingSystems.Count; i++)
            {
                if (DrawingSystems[i].NeedsDrawn())
                {
                    DrawingSystems[i].DrawDependencies();
                    DrawingSystems[i].Draw();
                }
            }

            #region setup
            g.BlendState = BlendState.Opaque;
            g.DepthStencilState = DepthStencilState.Default;
            g.SamplerStates[0] = SamplerState.LinearWrap;
            //g.DepthStencilState = DepthStencilState.Default;
            #endregion

            _gizmo.Draw();
            //Root.DrawDebugMode();
        }

        List<Transform> transforms = new List<Transform>();
    }
}