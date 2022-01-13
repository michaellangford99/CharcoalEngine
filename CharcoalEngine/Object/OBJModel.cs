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
using CharcoalEngine.Object;


namespace CharcoalEngine.Object
{
    public class OBJModel : Transform
    {
        public List<Vertex> Vertices = new List<Vertex>();
        public List<TexCoord> TexCoords = new List<TexCoord>();
        public List<Normal> Normals = new List<Normal>();

        public OBJModel(string filename, Vector3 Position, Vector3 YawPitchRoll, float Scale, bool FlipAxis)
        {
            OBJLoader loader = new OBJLoader();
            loader.Load(filename, Engine.g, Position, YawPitchRoll, Scale, FlipAxis, false, this);
        }
    }
    public class Mesh : Transform
    {
        [Editor(typeof(CollectionEditor), typeof(System.Drawing.Design.UITypeEditor))]
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

        [Editor(typeof(WindowsFormsComponentEditor), typeof(System.Drawing.Design.UITypeEditor))]
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

        public VertexPositionNormalTextureTangent[] V;

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
            V = new VertexPositionNormalTextureTangent[Faces.Count * 3/* * 2 */];

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
                
                Vector3 V0 = Vertices[Faces[i].fv[0].v1 - 1]._Vertex - (AbsolutePosition + WBPosition);
                Vector3 V1 = Vertices[Faces[i].fv[1].v1 - 1]._Vertex - (AbsolutePosition + WBPosition);
                Vector3 V2 = Vertices[Faces[i].fv[2].v1 - 1]._Vertex - (AbsolutePosition + WBPosition);

                Vector3 N0 = Normals[Faces[i].fv[0].n1 - 1]._Normal;
                Vector3 N1 = Normals[Faces[i].fv[1].n1 - 1]._Normal;
                Vector3 N2 = Normals[Faces[i].fv[2].n1 - 1]._Normal;

                Vector2 UV0 = TexCoords[Faces[i].fv[0].t1 - 1]._TexCoord;
                Vector2 UV1 = TexCoords[Faces[i].fv[1].t1 - 1]._TexCoord;
                Vector2 UV2 = TexCoords[Faces[i].fv[2].t1 - 1]._TexCoord;

                // Edges of the triangle : position delta
                Vector3 deltaPos1 = V1 - V0;
                Vector3 deltaPos2 = V2 - V0;

                // UV delta
                Vector2 deltaUV1 = UV1 - UV0;
                Vector2 deltaUV2 = UV2 - UV0;

                float r = 1.0f / (deltaUV1.X * deltaUV2.Y - deltaUV1.Y * deltaUV2.X);
                Vector3 tangent = (deltaPos1 * deltaUV2.Y - deltaPos2 * deltaUV1.Y) * r;
                Vector3 bitangent = (deltaPos2 * deltaUV1.X - deltaPos1 * deltaUV2.X) * r;

                V[i * 3 + 2] = new VertexPositionNormalTextureTangent(V0, N0, UV0, tangent);
                V[i * 3 + 1] = new VertexPositionNormalTextureTangent(V1, N1, UV1, tangent);
                V[i * 3 + 0] = new VertexPositionNormalTextureTangent(V2, N2, UV2, tangent);

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
                        e.Parameters["AlphaMaskEnabled"].SetValue(Material.AlphaMaskEnabled);
                        e.Parameters["AlphaMask"].SetValue(Material.AlphaMask);

                        //...
                        e.CurrentTechnique.Passes[0].Apply();
                        e.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
                        Engine.g.DrawUserPrimitives(PrimitiveType.TriangleList, V, 0, Faces.Count);
                    }
                }
            }
        }
    }
    // vertex structure data.  
    // i generate the binormal on the shader when i use custom vertexs
    // so maybe you could make it and match it up to that function.
    public struct VertexPositionNormalTextureTangent : IVertexType
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TextureCoordinate;
        public Vector3 Tangent;

        public VertexPositionNormalTextureTangent(Vector3 _Position, Vector3 _Normal, Vector2 _TextureCoordinate, Vector3 _Tangent)
        {
            Position = _Position;
            Normal = _Normal;
            TextureCoordinate = _TextureCoordinate;
            Tangent = _Tangent;
        }

        public static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
              new VertexElement(VertexElementByteOffset.PositionStartOffset(), VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
              new VertexElement(VertexElementByteOffset.OffsetVector3(), VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
              new VertexElement(VertexElementByteOffset.OffsetVector2(), VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
              new VertexElement(VertexElementByteOffset.OffsetVector3(), VertexElementFormat.Vector3, VertexElementUsage.Normal, 1)
        );
        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
    }
    public struct VertexElementByteOffset
    {
        public static int currentByteSize = 0;
        [STAThread]
        public static int PositionStartOffset() { currentByteSize = 0; var s = sizeof(float) * 3; currentByteSize += s; return currentByteSize - s; }
        public static int Offset(float n) { var s = sizeof(float); currentByteSize += s; return currentByteSize - s; }
        public static int Offset(Vector2 n) { var s = sizeof(float) * 2; currentByteSize += s; return currentByteSize - s; }
        public static int Offset(Color n) { var s = sizeof(int); currentByteSize += s; return currentByteSize - s; }
        public static int Offset(Vector3 n) { var s = sizeof(float) * 3; currentByteSize += s; return currentByteSize - s; }
        public static int Offset(Vector4 n) { var s = sizeof(float) * 4; currentByteSize += s; return currentByteSize - s; }

        public static int OffsetFloat() { var s = sizeof(float); currentByteSize += s; return currentByteSize - s; }
        public static int OffsetColor() { var s = sizeof(int); currentByteSize += s; return currentByteSize - s; }
        public static int OffsetVector2() { var s = sizeof(float) * 2; currentByteSize += s; return currentByteSize - s; }
        public static int OffsetVector3() { var s = sizeof(float) * 3; currentByteSize += s; return currentByteSize - s; }
        public static int OffsetVector4() { var s = sizeof(float) * 4; currentByteSize += s; return currentByteSize - s; }
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
