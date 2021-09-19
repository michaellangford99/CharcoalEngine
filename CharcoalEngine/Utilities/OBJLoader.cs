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
using Jitter.LinearMath;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Windows;
using System.Design;
using CharcoalEngine.Object;

namespace CharcoalEngine.Utilities
{
    public class OBJLoader
    {
        //Whether you want a nice verbose log dump on every load call:
        const bool LOG = true;

        //sharing is caring
        public List<Vertex> Vertices = new List<Vertex>();
        public List<TexCoord> TexCoords = new List<TexCoord>();
        public List<Normal> Normals = new List<Normal>();

        //only one?
        public List<Material> Materials;

        //always got his nose in a file...
        private StreamReader reader;
        private string foldername;
        private string location;
        private int Line_Number;

        /// <summary>
        /// this assumes the node passed as the local root is empty
        /// </summary>
        /// <param name="_location"></param>
        /// <param name="GraphicsDevice"></param>
        /// <param name="__Position"></param>
        /// <param name="__YawPitchRoll"></param>
        /// <param name="__Scale"></param>
        /// <param name="__FlipAxis"></param>
        /// <param name="__IsStatic"></param>
        /// <param name="Root">Node to attach the meshes to</param>
        public void Load(string _location, GraphicsDevice GraphicsDevice, Vector3 __Position, Vector3 __YawPitchRoll, float __Scale, bool __FlipAxis, bool __IsStatic, OBJModel Root)
        {
            //Application.EnableVisualStyles();
            //DialogResult FlipResult = MessageBox.Show("Flip Y & Z Axis?", "Flip Axis?", MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, 0);
            Root.Vertices = Vertices;
            Root.TexCoords = TexCoords;
            Root.Normals = Normals;


            location = _location;
            reader = new System.IO.StreamReader(location);
            foldername = location;
            int num_removed = 0;
            for (int i = foldername.Length - 1; i > 0; i--)
            {
                if (foldername[i] != '\\')
                {
                    foldername = foldername.Remove(i, 1);
                    num_removed++;
                }
                else
                {
                    break;
                }
            }
            if (LOG)
            {
                Console.WriteLine("foldername : " + foldername);
                Console.WriteLine("file name : " + location);
            }
            Root.Name = location.Remove(0, location.Length - num_removed);

            string line;
            while (true)
            {
                if (reader.EndOfStream == true)
                    break;
                line = reader.ReadLine();
                if (line.StartsWith("#"))
                    if (LOG) Console.WriteLine("Comment: " + line.Remove(0, 1));
                if (line.StartsWith("v "))
                    ReadVertex(line);
                if (line.StartsWith("vt "))
                    ReadTexCoord(line);
                if (line.StartsWith("vn "))
                    ReadNormal(line);
                if (line.StartsWith("f "))
                {
                    if (Root.Children.Count == 0)
                    {
                        MessageBox.Show("Error, no groups");
                        return;
                    }
                    if (Root.Children[Root.Children.Count - 1].Children.Count == 0)
                    {
                        //no material has been specified yet, borrow the material from the last mesh
                        if (Root.Children.Count > 1)
                        {
                            Mesh mesh = new Mesh();
                            mesh.Load("error");
                            Root.Children[Root.Children.Count - 1].Children.Add(mesh);

                            mesh.Vertices = Vertices;
                            mesh.TexCoords = TexCoords;
                            mesh.Normals = Normals;


                            mesh.Material = ((Mesh)Root.Children[Root.Children.Count - 2].Children[Root.Children[Root.Children.Count - 1].Children.Count - 1]).Material.Clone();
                            mesh.Name = ((Mesh)Root.Children[Root.Children.Count - 2].Children[Root.Children[Root.Children.Count - 1].Children.Count - 1]).Name;
                        }

                    }
                    ReadFace(line, Root);
                }
                if (line.StartsWith("g "))
                {
                    if (LOG) Console.WriteLine("Group!");
                    Transform T = new Transform();
                    T.Name = line.Remove(0, 2);

                    Root.Children.Add(T);

                    /*Material mat = new Material();
                    mat.Load("null");

                    ((Mesh)Root.Children[Root.Children.Count - 1]).Material = mat;
                    ((Mesh)Root.Children[Root.Children.Count - 1]).Material.TextureEnabled = false;

                    if (Root.Children.Count > 1)
                    {
                        ((Mesh)Root.Children[Root.Children.Count - 1]).Material = ((Mesh)Root.Children[Root.Children.Count - 2]).Material.Clone();
                    }*/
                }
                if (line.StartsWith("usemtl "))
                {
                    //if a new group has just been added, don't add another...
                    //try
                    //{
                    //    if (((Mesh)Root.Children[Root.Children.Count - 1]).Faces.Count != 0)
                    //    {
                    if (Root.Children.Count == 0)
                    {
                        Transform T = new Transform();
                        T.Name = line.Remove(0, 7);

                        Root.Children.Add(T);
                    }
                    if (LOG) Console.WriteLine("Group!");
                    Mesh mesh = new Mesh();
                    mesh.Load(line.Remove(0, 7));
                    mesh.Vertices = Vertices;
                    mesh.TexCoords = TexCoords;
                    mesh.Normals = Normals;
                    Root.Children[Root.Children.Count - 1].Children.Add(mesh);
                    //Root.Children[Root.Children.Count - 1].Name = line.Remove(0, 7);
                    Material mat = new Material();
                    mat.Load("null");

                    ((Mesh)Root.Children[Root.Children.Count - 1].Children[Root.Children[Root.Children.Count - 1].Children.Count - 1]).Material = mat;
                    ((Mesh)Root.Children[Root.Children.Count - 1].Children[Root.Children[Root.Children.Count - 1].Children.Count - 1]).Material.TextureEnabled = false;
                    /*    }
                    }
                    catch
                    {
                        try
                        {
                            if (((Mesh)Root.Children[Root.Children.Count - 1]).Faces.Count == 0)
                                Root.Children.RemoveAt(Root.Children.Count - 1);
                            if (LOG) Console.WriteLine("Group!");
                            Mesh m = new Mesh();
                            m.Load(line.Remove(0, 2));
                            Root.Children.Add(m);

                            Material mat = new Material();
                            mat.Load("null");

                            ((Mesh)Root.Children[Root.Children.Count - 1]).Material = mat;
                            ((Mesh)Root.Children[Root.Children.Count - 1]).Material.TextureEnabled = false;
                        }
                        catch
                        {
                            Mesh m = new Mesh();
                            m.Load(line.Remove(0, 2));
                            Root.Children.Add(m);

                            Material mat = new Material();
                            mat.Load("null");

                            ((Mesh)Root.Children[Root.Children.Count - 1]).Material = mat;
                            ((Mesh)Root.Children[Root.Children.Count - 1]).Material.TextureEnabled = false;
                        }
                    }*/

                    if (LOG) Console.WriteLine("usemtl.....");
                    string materialname = line.Remove(0, 7);
                    foreach (Material m in Materials)
                    {
                        if (materialname == m.name)
                        {
                            if (LOG) Console.WriteLine("material found");
                            /*if (Root.Children.Count == 0)
                            {
                                Mesh mesh = new Mesh();
                                mesh.Load(materialname);
                                Root.Children.Add(mesh);
                                Material material = new Material();
                                material.Load("null");
                                ((Mesh)Root.Children[Root.Children.Count - 1]).Material = material;
                                ((Mesh)Root.Children[Root.Children.Count - 1]).Material.TextureEnabled = false;
                            }*/
                            ((Mesh)Root.Children[Root.Children.Count - 1].Children[Root.Children[Root.Children.Count - 1].Children.Count - 1]).Material = m.Clone();
                            break;
                        }
                    }
                }
                if (line.StartsWith("mtllib "))
                {
                    if (LOG) Console.WriteLine("mtllib!");
                    line = line.Remove(0, 7);
                    List<Material> m = MTLLoader.Load(foldername + line, foldername, GraphicsDevice);
                    if (m == null)
                    {
                        Console.WriteLine("MTL load failed");
                        return;
                    }
                    Materials = m;
                }

                Line_Number++;

            }
            reader.Close();

            for (int group = 0; group < Root.Children.Count; group++)
            {
                Root.Children[group].Parent = Root;

                for (int sub = 0; sub < Root.Children[group].Children.Count; sub++)
                {
                    Mesh mesh = ((Mesh)Root.Children[group].Children[sub]);
                    mesh.Parent = Root.Children[group];
                    mesh.Vertices = Vertices;
                    mesh.TexCoords = TexCoords;
                    mesh.Normals = Normals;
                    mesh.UpdateMesh();
                    mesh.UpdateBoundingBox();
                    mesh.UpdateMatrix();
                }

            }

            //now go through each group and center the position so that it is easy to select sections
            //Root.boundingBox = new BoundingBox(Vector3.Zero, Vector3.Zero);
            for (int group = 0; group < Root.Children.Count; group++)
            {
                //Root.Children[group].boundingBox = new BoundingBox(Vector3.Zero, Vector3.Zero);
                BoundingBox b = Root.GetBBox(Root.Children[group].Children[0].boundingBox, ((Mesh)(Root.Children[group].Children[0])).WBPosition);

                for (int sub = 0; sub < Root.Children[group].Children.Count; sub++)
                {
                    //Root.Children[group].Children[sub].boundingBox = new BoundingBox(Vector3.Zero, Vector3.Zero);
                    b = BoundingBox.CreateMerged(b, Root.GetBBox(Root.Children[group].Children[sub].boundingBox, ((Mesh)(Root.Children[group].Children[sub])).WBPosition));

                }

                for (int sub = 0; sub < Root.Children[group].Children.Count; sub++)
                {
                    Root.Children[group].Children[sub].Position = ((Mesh)(Root.Children[group].Children[sub])).WBPosition - (b.Max + b.Min) / 2;
                }
                //Root.Children[group].__boundingbox__.Min -= Root.Children[group].Position;
                //Root.Children[group].__boundingbox__.Max -= Root.Children[group].Position;
                Root.Children[group].Position = (b.Max + b.Min) / 2;

                //Root.Children[group].UpdateBoundingBox();
                //for (int sub = 0; sub < Root.Children[group].Children.Count; sub++)
                //{
                //    Root.Children[group].Children[sub].UpdateBoundingBox();
                //}

            }

            for (int group = 0; group < Root.Children.Count; group++)
            {
                Root.Children[group].Parent = Root;

                for (int sub = 0; sub < Root.Children[group].Children.Count; sub++)
                {
                    ((Mesh)Root.Children[group].Children[sub]).Parent = Root.Children[group];
                    //assign faces and vertices?
                    //>>

                    //((Mesh)Root.Children[group].Children[sub]).Obj_File = this;
                    //((Mesh)Root.Children[group].Children[sub]).UpdateMesh();
                    ((Mesh)Root.Children[group].Children[sub]).UpdateBoundingBox();
                    //((Mesh)Root.Children[group].Children[sub]).UpdateMatrix();
                }

            }
            Root.Update();
            Root.UpdateBoundingBox();
            /*for (int group = 0; group < Root.Children.Count; group++)
            {
                Root.Children[group].Parent = Root;

                for (int sub = 0; sub < Root.Children[group].Children.Count; sub++)
                {
                    ((Mesh)Root.Children[group].Children[sub]).Parent = Root.Children[group];
                    ((Mesh)Root.Children[group].Children[sub]).Obj_File = this;
                    ((Mesh)Root.Children[group].Children[sub]).UpdateMesh();
                    ((Mesh)Root.Children[group].Children[sub]).UpdateBoundingBox();
                    ((Mesh)Root.Children[group].Children[sub]).UpdateMatrix();
                }

            }*/


            //if (FlipResult == DialogResult.Yes)
            //{
            //    YawPitchRoll.Y = -MathHelper.PiOver2;
            //}

            Root.Position = __Position;
            //YawPitchRoll = __YawPitchRoll;
            //Scale = new Vector3(__Scale);
        }

        private void ReadVertex(string l)
        {
            Vertex v = new Vertex();
            l = l.Remove(0, 2);

            while (l[0] == ' ')
                l = l.Remove(0, 1);

            string x = "";
            string y = "";
            string z = "";

            int i = 0;

            for (i = 0; i < l.Length; i++)
            {
                if (l[i] == ' ')
                {
                    i++;
                    break;
                }
                x += l[i];
            }

            for (; i < l.Length; i++)
            {
                if (l[i] == ' ')
                {
                    i++;
                    break;
                }
                y += l[i];
            }

            for (; i < l.Length; i++)
            {
                if (l[i] == ' ')
                {
                    i++;
                    break;
                }
                z += l[i];
            }
            /*Console.WriteLine("------------");
            Console.Write("Vertex ");
            Console.Write(x + " ");
            Console.Write(y + " ");
            Console.Write(z);
            Console.WriteLine(" ");*/
            v.LineNumber = Line_Number;
            v._Vertex = new Vector3(float.Parse(x), float.Parse(y), float.Parse(z));
            Vertices.Add(v);
        }
        private void ReadNormal(string l)
        {
            Normal n = new Normal();
            l = l.Remove(0, 3);

            while (l[0] == ' ')
                l = l.Remove(0, 1);

            string x = "";
            string y = "";
            string z = "";

            int i = 0;

            for (i = 0; i < l.Length; i++)
            {
                if (l[i] == ' ')
                {
                    i++;
                    break;
                }
                x += l[i];
            }

            for (; i < l.Length; i++)
            {
                if (l[i] == ' ')
                {
                    i++;
                    break;
                }
                y += l[i];
            }

            for (; i < l.Length; i++)
            {
                if (l[i] == ' ')
                {
                    i++;
                    break;
                }
                z += l[i];
            }
            /*Console.WriteLine("------------");
            Console.Write("Normal ");
            Console.Write(x + " ");
            Console.Write(y + " ");
            Console.Write(z);
            Console.WriteLine(" ");*/
            n.LineNumber = Line_Number;
            n._Normal = new Vector3(float.Parse(x), float.Parse(y), float.Parse(z));
            Normals.Add(n);
        }
        private void ReadTexCoord(string l)
        {
            TexCoord v = new TexCoord();
            l = l.Remove(0, 3);

            while (l[0] == ' ')
                l = l.Remove(0, 1);

            string x = "";
            string y = "";

            int i = 0;

            for (i = 0; i < l.Length; i++)
            {
                if (l[i] == ' ')
                {
                    i++;
                    break;
                }
                x += l[i];
            }

            for (; i < l.Length; i++)
            {
                if (l[i] == ' ')
                {
                    i++;
                    break;
                }
                y += l[i];
            }

            /*Console.WriteLine("------------");
            Console.Write("TexCoord: ");
            Console.Write(x + " ");
            Console.Write(y);
            Console.WriteLine(" ");*/
            v.LineNumber = Line_Number;

            float tx, ty;
            tx = 0;
            ty = 0;

            try
            {
                tx = float.Parse(x);
                ty = float.Parse(y);
            }
            catch
            {

            }

            v._TexCoord = new Vector2(tx, 1 - ty);
            TexCoords.Add(v);
        }
        private void ReadFace(string l, Transform Root)
        {
            l = l.Remove(0, 2);
            while (l.StartsWith(" "))
            {
                l = l.Remove(0, 1);
            }
            string x = "";
            string y = "";
            string z = "";

            FaceVertex[] fv = new FaceVertex[3];
            fv[0] = new FaceVertex();
            fv[1] = new FaceVertex();
            fv[2] = new FaceVertex();
            #region fv1
            x = y = z = "";

            int i = 0;

            for (i = 0; i < l.Length; i++)
            {
                if (l[i] == '/')
                {
                    i++;
                    break;
                }
                if (l[i] == ' ')
                {
                    break;
                }
                x += l[i];
            }

            for (; i < l.Length; i++)
            {
                if (l[i] == '/')
                {
                    i++;
                    break;
                }
                if (l[i] == ' ')
                {
                    break;
                }
                y += l[i];
            }

            for (; i < l.Length; i++)
            {
                if (l[i] == ' ')
                {
                    i++;
                    break;
                }
                z += l[i];
            }
            //
            if (x == "") { x = "0"; }
            fv[0].v1 = int.Parse(x);
            if (y == "")
            {
                TexCoord n = new TexCoord();
                n._TexCoord = Vector2.Zero;
                if (TexCoords.Count == 0)
                    TexCoords.Add(n);
                y = "1";
            }
            fv[0].t1 = int.Parse(y);
            if (z == "")
            {
                Normal n = new Normal();
                n._Normal = Vector3.Zero;
                if (Normals.Count == 0)
                    Normals.Add(n);
                z = "1";
            }
            fv[0].n1 = int.Parse(z);
            #endregion
            #region fv2
            x = y = z = "";
            for (; i < l.Length; i++)
            {
                if (l[i] == '/')
                {
                    i++;
                    break;
                }
                if (l[i] == ' ')
                {
                    break;
                }
                x += l[i];
            }

            for (; i < l.Length; i++)
            {
                if (l[i] == '/')
                {
                    i++;
                    break;
                }
                if (l[i] == ' ')
                {
                    break;
                }
                y += l[i];
            }

            for (; i < l.Length; i++)
            {
                if (l[i] == ' ')
                {
                    i++;
                    break;
                }
                z += l[i];
            }
            //
            if (x == "") { x = "0"; }
            fv[1].v1 = int.Parse(x);
            if (y == "")
            {
                TexCoord n = new TexCoord();
                n._TexCoord = Vector2.Zero;
                if (TexCoords.Count == 0)
                    TexCoords.Add(n);
                y = "1";
            }
            fv[1].t1 = int.Parse(y);
            if (z == "")
            {
                Normal n = new Normal();
                n._Normal = Vector3.Zero;
                if (Normals.Count == 0)
                    Normals.Add(n);
                z = "1";
            }
            fv[1].n1 = int.Parse(z);
            #endregion
            #region fv3
            x = y = z = "";
            for (; i < l.Length; i++)
            {
                if (l[i] == '/')
                {
                    i++;
                    break;
                }
                if (l[i] == ' ')
                {
                    break;
                }
                x += l[i];
            }

            for (; i < l.Length; i++)
            {
                if (l[i] == '/')
                {
                    i++;
                    break;
                }
                if (l[i] == ' ')
                {
                    break;
                }
                y += l[i];
            }

            for (; i < l.Length; i++)
            {
                if (l[i] == ' ')
                {
                    i++;
                    break;
                }
                z += l[i];
            }
            //
            if (x == "") { x = "0"; }
            fv[2].v1 = int.Parse(x);
            if (y == "")
            {
                TexCoord n = new TexCoord();
                n._TexCoord = Vector2.Zero;
                if (TexCoords.Count == 0)
                    TexCoords.Add(n);
                y = "1";
            }
            fv[2].t1 = int.Parse(y);
            if (z == "")
            {
                Normal n = new Normal();
                n._Normal = Vector3.Zero;
                if (Normals.Count == 0)
                    Normals.Add(n);
                z = "1";
            }
            fv[2].n1 = int.Parse(z);
            #endregion
            #region fv4?
            x = y = z = "";
            for (; i < l.Length; i++)
            {
                if (l[i] == '/')
                {
                    i++;
                    break;
                }
                if (l[i] == ' ')
                {
                    break;
                }
                x += l[i];
            }

            for (; i < l.Length; i++)
            {
                if (l[i] == '/')
                {
                    i++;
                    break;
                }
                if (l[i] == ' ')
                {
                    break;
                }
                y += l[i];
            }

            for (; i < l.Length; i++)
            {
                if (l[i] == ' ')
                {
                    i++;
                    break;
                }
                z += l[i];
            }
            //
            if (x != "")
            {
                int v1 = int.Parse(x);
                if (y == "")
                {
                    TexCoord n = new TexCoord();
                    n._TexCoord = Vector2.Zero;
                    if (TexCoords.Count == 0)
                        TexCoords.Add(n);
                    y = "1";
                }
                int t1 = int.Parse(y);
                if (z == "")
                {
                    Normal n = new Normal();
                    n._Normal = Vector3.Zero;
                    if (Normals.Count == 0)
                        Normals.Add(n);
                    z = "1";
                }
                int n1 = int.Parse(z);



                FaceVertex[] fv2 = new FaceVertex[3];
                fv2[0] = fv[0];
                fv2[1] = fv[2];
                fv2[2] = new FaceVertex();
                fv2[2].n1 = n1;
                fv2[2].v1 = v1;
                fv2[2].t1 = t1;

                Face f2 = new Face();
                f2.fv = fv2;
                ((Mesh)(Root.Children[Root.Children.Count - 1]).Children[Root.Children[Root.Children.Count - 1].Children.Count - 1]).Faces.Add(f2);

            }
            else
            {
                //Console.WriteLine("did not add 4th face!!!");
            }
            #endregion


            /*Console.WriteLine("------------");
            Console.Write("Face: ");
            Console.Write(fv[0].v1 + "/" + fv[0].t1 + "/" + fv[0].n1 + " ");
            Console.Write(fv[1].v1 + "/" + fv[1].t1 + "/" + fv[1].n1 + " ");
            Console.Write(fv[2].v1 + "/" + fv[2].t1 + "/" + fv[2].n1 + " ");
            Console.WriteLine(" ");*/
            Face f = new Face();
            f.fv = fv;
            ((Mesh)Root.Children[Root.Children.Count - 1].Children[Root.Children[Root.Children.Count - 1].Children.Count - 1]).Faces.Add(f);
        }
    }
}
