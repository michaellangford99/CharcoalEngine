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
    public class MTLLoader
    {
        public static List<Material> Load(string Path, string LocalFolder, GraphicsDevice g)
        {
            List<Material> Materials = new List<Material>();

            StreamReader reader;
            try
            {
                reader = new System.IO.StreamReader(Path);
            }
            catch
            {
                MessageBox.Show("This model has no MTL File, & is unusable");
                return null;
            }
            string line = "";
            while (true)
            {
                if (reader.EndOfStream == true)
                    break;
                line = reader.ReadLine();
                while (true)
                {
                    if (line.StartsWith(" "))
                        line = line.Remove(0);
                    else
                        break;
                }
                if (line.StartsWith("newmtl "))
                {
                    Material newmtl = new Material();
                    newmtl.Load(line.Remove(0, 7));
                    Materials.Add(newmtl);
                    Materials[Materials.Count - 1].TextureEnabled = false;
                }
                #region load_texture
                if (line.StartsWith("map_Kd "))
                {
                    Materials[Materials.Count - 1].TextureEnabled = true;

                    string texturename = LocalFolder + line.Remove(0, 7);
                    Materials[Materials.Count - 1].Texture = TextureImporter.LoadTextureFromFile(texturename);
                    Materials[Materials.Count - 1].TextureFileName = texturename;
                    if (Materials[Materials.Count - 1].Texture == null)
                        Materials[Materials.Count - 1].TextureEnabled = false;
                }
                if (line.StartsWith("map_bump "))
                {
                    Materials[Materials.Count - 1].NormalMapEnabled = true;

                    line = line.Remove(0, 9);

                    if (line.Length > 0)
                    {
                        while (line[0] == ' ')
                            line = line.Remove(0, 1);

                        string texturename = LocalFolder + line;

                        Materials[Materials.Count - 1].NormalMap = TextureImporter.LoadTextureFromFile(texturename);
                        //Materials[Materials.Count - 1].TextureFileName = texturename;
                    }
                    if (Materials[Materials.Count - 1].NormalMap == null)
                        Materials[Materials.Count - 1].NormalMapEnabled = false;
                }
                #endregion
                if (line.StartsWith("Kd "))//diffuse color
                {
                    // Kd 0.0470588 0.447059 0.133333
                    string dc = line.Remove(0, 3);

                    while (dc[0] == ' ')
                        dc = dc.Remove(0, 1);

                    string x = "", y = "", z = "";

                    int c = 0;

                    for (; c < dc.Length; c++)
                    {
                        if (dc[c] != ' ')
                            x += dc[c];
                        else
                        {
                            c++;
                            break;
                        }
                    }
                    for (; c < dc.Length; c++)
                    {
                        if (dc[c] != ' ')
                            y += dc[c];
                        else
                        {
                            c++;
                            break;
                        }
                    }
                    for (; c < dc.Length; c++)
                    {
                        if (dc[c] != ' ')
                            z += dc[c];
                        else
                        {
                            c++;
                            break;
                        }
                    }

                    //Console.WriteLine("Diffuse Color: " + x + " " + y + " " + z);
                    Materials[Materials.Count - 1].DiffuseColor = new Vector3(float.Parse(x), float.Parse(y), float.Parse(z));
                }
                if (line.StartsWith("d "))//alpha
                {
                    // Ka 0.0470588
                    string a = line.Remove(0, 2);

                    while (a[0] == ' ')
                        a = a.Remove(0, 1);

                    string alpha = "";

                    for (int c = 0; c < a.Length; c++)
                    {
                        if (a[c] != ' ')
                            alpha += a[c];
                        else
                        {
                            c++;
                            break;
                        }
                    }
                    Console.WriteLine("Alpha: " + alpha);
                    Materials[Materials.Count - 1].Alpha = float.Parse(alpha);
                    if (Materials[Materials.Count - 1].Alpha < 1.0f) Materials[Materials.Count - 1].AlphaEnabled = true;
                }

            }
            reader.Close();
            return Materials;
        }
    }
}
