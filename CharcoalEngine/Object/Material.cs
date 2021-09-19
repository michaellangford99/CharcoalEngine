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

namespace CharcoalEngine.Object
{
    public class Material
    {
        public string name;

        public string TextureFileName;

        [BrowsableAttribute(true)]
        [EditorAttribute(typeof(System.Windows.Forms.Design.FileNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string _Texture
        {
            get { return TextureFileName; }
            set
            {
                TextureEnabled = true;
                Texture = TextureImporter.LoadTextureFromFile(value);
                if (Texture == null)
                    TextureEnabled = false;
                TextureFileName = value;
            }
        }

        public Texture2D Texture;

        public bool _TextureEnabled
        {
            get { return TextureEnabled; }
            set { TextureEnabled = value; }
        }
        public bool TextureEnabled;


        public string NormalMapFileName;

        [BrowsableAttribute(true)]
        [EditorAttribute(typeof(System.Windows.Forms.Design.FileNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string _NormalMap
        {
            get { return NormalMapFileName; }
            set
            {
                NormalMapEnabled = true;
                NormalMap = TextureImporter.LoadTextureFromFile(value);
                if (NormalMap == null)
                    NormalMapEnabled = false;
                NormalMapFileName = value;
            }
        }

        public Texture2D NormalMap;

        public bool _NormalMapEnabled
        {
            get { return NormalMapEnabled; }
            set { NormalMapEnabled = value; }
        }
        public bool NormalMapEnabled;

        public bool _Visible
        {
            get { return Visible; }
            set { Visible = value; }
        }
        public bool Visible = true;

        public bool AlphaEnabled { get; set; }

        [Editor(typeof(ColorEditor), typeof(UITypeEditor))]
        public System.Drawing.Color _DiffuseColor
        {
            get
            {
                return System.Drawing.Color.FromArgb((int)(DiffuseColor.X * 255), (int)(DiffuseColor.Z * 255), (int)(DiffuseColor.Z * 255));
            }
            set { DiffuseColor = new Vector3((float)value.R / 255.0f, (float)value.G / 255.0f, (float)value.B / 255.0f); }
        }
        public Vector3 DiffuseColor = Vector3.One;
        public Vector3 Ambient;

        public float _Alpha
        {
            get { return Alpha; }
            set { Alpha = value; }
        }
        public float Alpha = 1;

        //public Texture2D SpecularMap;
        //public bool SpecularMapEnabled = false;
        //public Texture2D BumpMap { get; set; }
        //public bool BumpMapEnabled { get; set; } = false;

        public Material()
        {

        }
        public void Load(string n)
        {
            name = n;
        }
        public Material Clone()
        {
            Material mat = new Material();
            mat.Load(name);
            mat.TextureFileName = TextureFileName;
            mat.Texture = Texture;
            mat.TextureEnabled = TextureEnabled;
            mat.Visible = Visible;
            mat.DiffuseColor = DiffuseColor;
            mat.Ambient = Ambient;
            mat.Alpha = Alpha;
            mat.AlphaEnabled = AlphaEnabled;
            mat.NormalMap = NormalMap;
            mat.NormalMapEnabled = NormalMapEnabled;

            return mat;
        }
    }
}
