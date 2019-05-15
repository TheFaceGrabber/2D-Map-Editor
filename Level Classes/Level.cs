using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;
using Xceed.Wpf.Toolkit.PropertyGrid.Converters;
using Xceed.Wpf.Toolkit.PropertyGrid.Commands;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

[System.Serializable]
    public class Entity
    {
        [Category("Information")]
        [DisplayName("Entity Name")]
        public string Name { get; set; }

        [Category("Information")]
        [DisplayName("Editor Only")]
        public bool IsGizmo { get; set; }

        [Category("Information")]
        [DisplayName("World Position")]
        [Description("Position in world space for this entity to start its life")]
        public Point Position { get; set; }

        [Category("Information")]
        [DisplayName("World Scale")]
        [Description("How big (or small) the entity will be relative to the block size")]
        public float Size { get; set; } = 1;

        [Category("Information")]
        [DisplayName("Texture Location")]
        [Description("Location of the texture to be used when displaying this entity - relative to the game directory")]
        [Editor(typeof(ImageEditor), typeof(ImageEditor))]
        public string Image
        {
            get { return path; }
            set
            {
                path = value;
                Source = new BitmapImage(new Uri(value, UriKind.Relative));
            }
        }

        [JsonIgnore]
        public BitmapImage Source;

        string path = "";

    }

    [System.Serializable]
    public class Block
    {
        string _image = "";

        [Category("Information")]
        [DisplayName("Block Name")]
        [PropertyOrder(0)]
        public string Name { get; set; }

        [Category("Information")]
        [DisplayName("Block Char")]
        [Description("The character used to represent the block on the world grid")]
        public char Char { get; set; } = '-';

        [Category("Information")]
        [DisplayName("Texture Location")]
        [Description("Location of the texture to be used when displaying this block - relative to the game directory")]
        [Editor(typeof(ImageEditor), typeof(ImageEditor))]
        public string Image
        {
            get { return path; }
            set
            {
                path = value;
                Source = new BitmapImage(new Uri(value, UriKind.Relative));
            }
        }

        [JsonIgnore]
        public BitmapImage Source;

        string path = "";

        [Category("Information")]
        [DisplayName("Enable Collision")]
        [Description("Will entities and other objects be able to collide with this block?")]
        public bool IsCollidable { get; set; }

        [Category("Information")]
        [DisplayName("Path Node")]
        [Description("Will AI be able to use this block to walk upon?")]
        public bool IsPathNode { get; set; }
    }

    [System.Serializable]
    public class Level
    {
        public List<Entity> Entities = new List<Entity>();
        public List<Block> Blocks = new List<Block>();

        public string[] LevelGrid;

        public int LevelSize { get; set; } = 32;

        public Level()
        {
            if (LevelGrid == null)
            {
                LevelGrid = new string[LevelSize];
                Debug.WriteLine("Init grid");
                for (int i = 0; i < LevelSize; i++)
                {
                    string str = "";
                    for (int x = 0; x < LevelSize; x++)
                    {
                        str += " ";
                    }
                    LevelGrid[i] = str;
                }
            }
        }
    }