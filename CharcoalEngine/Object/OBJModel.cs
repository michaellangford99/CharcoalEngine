using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
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
using System.Drawing.Design;
using System.Windows;
using System.Design;
using CharcoalEngine.Object;


namespace CharcoalEngine.Object
{
    public class OBJModel : Transform
    {
        public List<Vertex> Vertices = new List<Vertex>();
        public List<TexCoord> TexCoords = new List<TexCoord>();
        public List<Normal> Normals = new List<Normal>();

        public OBJModel(string filename, DrawingSystem meshdrawer, Vector3 Position, Vector3 YawPitchRoll, float Scale, bool FlipAxis)
        {
            OBJLoader loader = new OBJLoader();
            loader.Load(filename, Engine.g, Position, YawPitchRoll, Scale, FlipAxis, false, this);
            foreach (Transform group in Children)
            {
                foreach (Transform mesh in group.Children)
                {
                    meshdrawer.RegisterItem(mesh);
                }
            }
        }
    }
    public class Mesh : Transform
    {
        [Editor(typeof(CollectionEditor), typeof(UITypeEditor))]
        public List<Material> Material_Edit
        {
            get
            {
                List<Material> T = new List<Material>();
                T.Add(Material);
                return T;
            }
            set
            {
                Material = value[0];
            }
        }

        [Editor(typeof(WindowsFormsComponentEditor), typeof(UITypeEditor))]
        [Browsable(true)]
        public Material Material
        {
            get
            {
                return __material__;
            }
            set
            {
                __material__ = value;
            }
        }
        Material __material__;

        public List<Vertex> Vertices = new List<Vertex>();
        public List<TexCoord> TexCoords = new List<TexCoord>();
        public List<Normal> Normals = new List<Normal>();
        public List<Face> Faces = new List<Face>();

        public VertexPositionNormalTexture[] V;

        public void Load(string n)
        {
            Name = n;
        }
        public Vector3 WBPosition;
        /*
        public void Update_Matrix()
        {
           MeshWorld = Matrix.CreateTranslation(-Center) * Matrix.CreateScale(Scale) * Matrix.CreateFromYawPitchRoll(YawPitchRoll.X, YawPitchRoll.Y, YawPitchRoll.Z) * Matrix.CreateTranslation(Center) * Matrix.CreateTranslation(Position);
        }
        */
        public void UpdateMesh()
        {
            LocalBoundingBox = new BoundingBox();
            //boundingSphere = 
            V = new VertexPositionNormalTexture[Faces.Count * 3/* * 2 */];

            List<Vector3> Points = new List<Vector3>();

            for (int i = 0; i < Faces.Count; i++)
            {

                Points.Add(Vertices[((Face)Faces[i]).fv[0].v1 - 1]._Vertex);
                Points.Add(Vertices[((Face)Faces[i]).fv[1].v1 - 1]._Vertex);
                Points.Add(Vertices[((Face)Faces[i]).fv[2].v1 - 1]._Vertex);
            }
            LocalBoundingBox = BoundingBox.CreateFromPoints(Points);
            __localboundingbox__ = new BoundingBox(boundingBox.Min - (boundingBox.Max + boundingBox.Min) / 2, boundingBox.Max - (boundingBox.Max + boundingBox.Min) / 2);
            WBPosition = (boundingBox.Max + boundingBox.Min) / 2;
            //Console.WriteLine(boundingBox);
            UpdateBoundingBox();

            for (int i = 0; i < Faces.Count; i++)
            {
                ///
                /// Add tangent calculation here based off of gradient of texture coordinates
                ///

                V[i * 3 + 2] = new VertexPositionNormalTexture(Vertices[Faces[i].fv[0].v1 - 1]._Vertex - (AbsolutePosition + WBPosition), Normals[Faces[i].fv[0].n1 - 1]._Normal, TexCoords[Faces[i].fv[0].t1 - 1]._TexCoord);
                V[i * 3 + 1] = new VertexPositionNormalTexture(Vertices[Faces[i].fv[1].v1 - 1]._Vertex - (AbsolutePosition + WBPosition), Normals[Faces[i].fv[1].n1 - 1]._Normal, TexCoords[Faces[i].fv[1].t1 - 1]._TexCoord);
                V[i * 3 + 0] = new VertexPositionNormalTexture(Vertices[Faces[i].fv[2].v1 - 1]._Vertex - (AbsolutePosition + WBPosition), Normals[Faces[i].fv[2].n1 - 1]._Normal, TexCoords[Faces[i].fv[2].t1 - 1]._TexCoord);

            }
            //Parent.Position = Position;
            //Position = Vector3.Zero;


        }
        public void GbufferDraw(Effect e)
        {
            if (Faces.Count != 0)
            {
                if (Material.Visible)
                {
                    //if (Camera.Viewport.frAbsoluteBoundingBox)
                    //fill basic parameters
                    if (e != null)
                    {
                        if ((string)e.Tag == "NDT" && Material.AlphaEnabled == true)
                        {
                            return;
                        }

                        if ((string)e.Tag == "ALPHANDT" && Material.AlphaEnabled == false)
                        {
                            return;
                        }

                        e.Parameters["World"].SetValue(AbsoluteWorld);
                        e.Parameters["View"].SetValue(Camera.View);
                        e.Parameters["Projection"].SetValue(Camera.Projection);
                        e.Parameters["BasicTexture"].SetValue(Material.Texture);
                        e.Parameters["TextureEnabled"].SetValue(Material._TextureEnabled);
                        e.Parameters["NormalMap"].SetValue(Material.NormalMap);
                        e.Parameters["NormalMapEnabled"].SetValue(Material.NormalMapEnabled);
                        e.Parameters["DiffuseColor"].SetValue(Material.DiffuseColor);
                        e.Parameters["Alpha"].SetValue(Material.Alpha);
                        e.Parameters["AlphaEnabled"].SetValue(Material.AlphaEnabled);

                        //...
                        e.CurrentTechnique.Passes[0].Apply();
                        e.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
                        Engine.g.DrawUserPrimitives(PrimitiveType.TriangleList, V, 0, Faces.Count);
                    }
                }
            }
        }
    }
    public struct Vertex
    {
        public Vector3 _Vertex;
        public int LineNumber;
    }
    public struct Normal
    {
        public Vector3 _Normal;
        public int LineNumber;
    }
    public struct TexCoord
    {
        public Vector2 _TexCoord;
        public int LineNumber;
    }
    public struct FaceVertex
    {
        public int v1;
        public int t1;
        public int n1;
    }

    public struct Face
    {
        public FaceVertex[] fv;
    }
}
