using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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

namespace WpfApp1
{
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

    public partial class MainWindow : Window
    {
        public Level CurrentLevel;
        public string FileDirectory = "";

        ObservableCollection<TreeViewItem> Items { get; set; } = new ObservableCollection<TreeViewItem>();

        TreeViewWithIcons Entities;
        TreeViewWithIcons Blocks;
        TreeViewWithIcons Level;

        ContextMenu EntitiesContextMenu;
        ContextMenu BlocksContextMenu;

        ContextMenu EntityContextMenu;
        ContextMenu BlockContextMenu;

        public Block SelectedBlock = null;

        public MainWindow()
        {
            InitializeComponent();
            Init();
        }

        public void Init()
        {
            ProjectTreeView.SelectedItemChanged += ProjectTreeView_SelectedItemChanged;
            CurrentLevel = new Level();
            EntitiesContextMenu = new ContextMenu();
            var mi = new MenuItem();
            mi.Header = "Add Entity";
            mi.Click += CreateEntity_Click;
            EntitiesContextMenu.Items.Add(mi);

            BlocksContextMenu = new ContextMenu();
            mi = new MenuItem();
            mi.Header = "Add Block";
            mi.Click += CreateBlock_Click;
            BlocksContextMenu.Items.Add(mi);

            Entities = new TreeViewWithIcons()
            {
                HeaderText = "Entities",
                Icon = new BitmapImage(new Uri("Images/Icons/EntitiesIcon.png", UriKind.Relative)),
                ContextMenu = EntitiesContextMenu
            };

            Entities.Expanded += Entities_Expanded;
            Entities.Collapsed += Entities_Collapsed;

            Blocks = new TreeViewWithIcons()
            {
                HeaderText = "Blocks",
                Icon = new BitmapImage(new Uri("Images/Icons/BlocksIcon.png", UriKind.Relative)),
                ContextMenu = BlocksContextMenu
            };

            Blocks.Expanded += Blocks_Expanded;
            Blocks.Collapsed += Blocks_Collapsed;

            Level = new TreeViewWithIcons()
            {
                HeaderText = "Level Layout",
                Icon = new BitmapImage(new Uri("Images/Icons/LevelIcon.png", UriKind.Relative))
            };

            ProjectTreeView.Items.Add(Entities);
            ProjectTreeView.Items.Add(Blocks);
            ProjectTreeView.Items.Add(Level);

            EntityContextMenu = new ContextMenu();
            mi = new MenuItem();
            mi.Header = "Duplicate";
            mi.Click += DuplicateEntity_Click;
            EntityContextMenu.Items.Add(mi);
            mi = new MenuItem();
            mi.Header = "Remove";
            mi.Click += RemoveEntity_Click;
            EntityContextMenu.Items.Add(mi);

            BlockContextMenu = new ContextMenu();
            mi = new MenuItem();
            mi.Header = "Duplicate";
            mi.Click += DuplicateBlock_Click;
            BlockContextMenu.Items.Add(mi);
            mi = new MenuItem();
            mi.Header = "Remove";
            mi.Click += RemoveBlock_Click;
            BlockContextMenu.Items.Add(mi);

            PropGrid.LostFocus += PropGrid_LostFocus;
            PropGrid.KeyDown += PropGrid_KeyDown;

        }

        #region Public Methods

        public void OpenItem(string path)
        {
            if (File.Exists(path))
            {
                Mouse.OverrideCursor = Cursors.Wait;
                var op = File.ReadAllText(path);

                CurrentLevel = JsonConvert.DeserializeObject<Level>(op);
                Entities.IsExpanded = false;
                Blocks.IsExpanded = false;

                Entities_Collapsed(null, null);
                Blocks_Collapsed(null, null);
                FileDirectory = path;
                Mouse.OverrideCursor = null;
                UpdateCanvas();
            }
            else
            {
                MessageBox.Show("Failed to open file at " + path + "!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void UpdateCanvas()
        {
            MainCanvas.Width = CurrentLevel.LevelSize * ImageCanvas.GRIDSIZE;
            MainCanvas.Height = CurrentLevel.LevelSize * ImageCanvas.GRIDSIZE + (2 * ImageCanvas.GRIDSIZE);

            for (int i = 0; i < CurrentLevel.LevelSize; i++)
            {
                for (int y = 0; y < CurrentLevel.LevelSize; y++)
                {
                    MainCanvas.Images.Add(new LevelImage("Images/Grid.png", new Point(i, y)));
                }
            }

            MainCanvas.Update(CurrentLevel);
        }

        public void UpdateBlocks()
        {
            foreach (var item in Blocks.Items)
            {
                if (item is TreeViewWithIcons tree)
                {
                    tree.HeaderText = CurrentLevel.Blocks[Blocks.Items.IndexOf(item)].Name;
                }
            }
        }

        public void UpdateEntities()
        {
            foreach (var item in Entities.Items)
            {
                if (item is TreeViewWithIcons tree)
                {
                    tree.HeaderText = CurrentLevel.Entities[Entities.Items.IndexOf(item)].Name;
                }
            }
        }

        #endregion

        #region App Events

        private void PropGrid_KeyDown(object sender, KeyEventArgs e)
        {
            UpdateBlocks();
            UpdateEntities();
            UpdateCanvas();
        }

        private void PropGrid_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateBlocks();
            UpdateEntities();
            UpdateCanvas();
        }

        private void ProjectTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeViewWithIcons val)
            {
                if (Entities.Items.Contains(val))
                {
                    int i = Entities.Items.IndexOf(ProjectTreeView.SelectedItem);
                    PropGrid.SelectedObject = CurrentLevel.Entities[i];
                }
                else if(Blocks.Items.Contains(val))
                {
                    int i = Blocks.Items.IndexOf(ProjectTreeView.SelectedItem);
                    PropGrid.SelectedObject = CurrentLevel.Blocks[i];
                    SelectedBlock = CurrentLevel.Blocks[i];
                }
                else
                {
                    PropGrid.SelectedObject = null;
                    SelectedBlock = null;
                }
            }
            else
            {
                PropGrid.SelectedObject = null;
            }
        }

        private void Blocks_Collapsed(object sender, RoutedEventArgs e)
        {
            Blocks.Items.Clear();
            Blocks.Items.Add("Loading...");
        }

        private void Entities_Collapsed(object sender, RoutedEventArgs e)
        {
            Entities.Items.Clear();
            Entities.Items.Add("Loading...");
        }

        private void Blocks_Expanded(object sender, RoutedEventArgs e)
        {
            if (Blocks.Items.Count > 0 && Blocks.Items[0] is string && (string)Blocks.Items[0] == "Loading...")
                Blocks.Items.RemoveAt(0);
            else
                Blocks.Items.Clear();

            foreach (var item in CurrentLevel.Blocks)
            {
                Blocks.Items.Add(new TreeViewWithIcons()
                {
                    HeaderText = item.Name,
                    ContextMenu = BlockContextMenu,
                    Icon = new BitmapImage(new Uri("Images/Icons/BlocksIcon.png", UriKind.Relative))
                });
            }
        }

        private void Entities_Expanded(object sender, RoutedEventArgs e)
        {
            if (Entities.Items.Count > 0 && Entities.Items[0] is string && (string)Entities.Items[0] == "Loading...")
                Entities.Items.RemoveAt(0);
            else
                Entities.Items.Clear();

            foreach (var item in CurrentLevel.Entities)
            {
                Entities.Items.Add(new TreeViewWithIcons()
                {
                    HeaderText = item.Name,
                    ContextMenu = EntityContextMenu,
                    Icon = new BitmapImage(new Uri("Images/Icons/EntitiesIcon.png", UriKind.Relative))
                });
            }
        }

        private void RemoveEntity_Click(object sender, RoutedEventArgs e)
        {
            if(Entities.Items.Contains(ProjectTreeView.SelectedItem))
            {
                CurrentLevel.Entities.RemoveAt(Entities.Items.IndexOf(ProjectTreeView.SelectedItem));
            }
            Entities_Expanded(null, null);
            UpdateCanvas();
        }

        private void RemoveBlock_Click(object sender, RoutedEventArgs e)
        {
            if (Blocks.Items.Contains(ProjectTreeView.SelectedItem))
            {
                CurrentLevel.Blocks.RemoveAt(Blocks.Items.IndexOf(ProjectTreeView.SelectedItem));
            }
            Blocks_Expanded(null, null);
            UpdateCanvas();
        }

        private void CreateEntity_Click(object sender, RoutedEventArgs e)
        {
            CurrentLevel.Entities.Add(new Entity() { Name = "New Entity" });
            Entities_Expanded(null, null);
            UpdateCanvas();
        }

        private void CreateBlock_Click(object sender, RoutedEventArgs e)
        {
            CurrentLevel.Blocks.Add(new Block() { Name = "New Block" });
            Blocks_Expanded(null, null);
            UpdateCanvas();
        }

        private void DuplicateBlock_Click(object sender, RoutedEventArgs e)
        {
            if (Blocks.Items.Contains(ProjectTreeView.SelectedItem))
            {
                var obj = CurrentLevel.Blocks[Blocks.Items.IndexOf(ProjectTreeView.SelectedItem)];

                CurrentLevel.Blocks.Add(new Block() { Name = obj.Name, Char = obj.Char, Image = obj.Image, IsCollidable= obj.IsCollidable, IsPathNode = obj.IsPathNode });
                Blocks_Expanded(null, null);
                UpdateCanvas();
            }
        }

        private void DuplicateEntity_Click(object sender, RoutedEventArgs e)
        {
            if (Entities.Items.Contains(ProjectTreeView.SelectedItem))
            {
                var obj = CurrentLevel.Entities[Entities.Items.IndexOf(ProjectTreeView.SelectedItem)];

                CurrentLevel.Entities.Add(new Entity() { Name = obj.Name, Image = obj.Image, IsGizmo=obj.IsGizmo, Position=obj.Position, Size=obj.Size});
                Entities_Expanded(null, null);
                UpdateCanvas();
            }
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SaveItem_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;

            if (FileDirectory == "")
            {
                SaveAsItem_Click(sender, e);
                return;
            }

            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;

            using (StreamWriter sw = new StreamWriter(FileDirectory))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, CurrentLevel);
            }

            Mouse.OverrideCursor = null;
        }

        private void SaveAsItem_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog sld = new Microsoft.Win32.SaveFileDialog();
            sld.DefaultExt = ".jlf";
            sld.Filter = "Level file (*.jlf)|*.jlf";

            var r = sld.ShowDialog();

            if(r == true)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                var loc = sld.FileName;

                JsonSerializer serializer = new JsonSerializer();
                serializer.NullValueHandling = NullValueHandling.Ignore;

                using (StreamWriter sw = new StreamWriter(loc))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, CurrentLevel);
                }

                FileDirectory = loc;
                Mouse.OverrideCursor = null;
            }
            else
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void OpenItem_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog sld = new Microsoft.Win32.OpenFileDialog();
            sld.DefaultExt = ".jlf";
            sld.Filter = "Level file (*.jlf)|*.jlf";

            var r = sld.ShowDialog();

            if (r == true)
            {
                var loc = sld.FileName;
                OpenItem(loc);
            }
            else
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void NewItem_Click(object sender, RoutedEventArgs e)
        {
            FileDirectory = "";
            CurrentLevel = new Level();
            Entities_Expanded(null, null);
            Blocks_Expanded(null, null);
            UpdateCanvas();
        }

        private void MainCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point p = Mouse.GetPosition(MainCanvas);
            p = new Point(Math.Floor(p.X / CurrentLevel.LevelSize), Math.Floor(p.Y / CurrentLevel.LevelSize) - 1);
            if (SelectedBlock != null)
            {
                string replacement = e.RightButton == MouseButtonState.Pressed ? " " : SelectedBlock.Char.ToString();
                string temp = CurrentLevel.LevelGrid[(int)p.Y];
                temp = temp.Remove((int)p.X, 1);
                temp = temp.Insert((int)p.X, replacement);
                CurrentLevel.LevelGrid[(int)p.Y] = temp;
                UpdateCanvas();
            }
            else
            {
                if(e.RightButton == MouseButtonState.Pressed)
                {
                    string temp = CurrentLevel.LevelGrid[(int)p.Y];
                    temp = temp.Remove((int)p.X, 1);
                    temp = temp.Insert((int)p.X, " ");
                    CurrentLevel.LevelGrid[(int)p.Y] = temp;
                    UpdateCanvas();
                }
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                SaveItem_Click(sender, null);
            }
            else if(e.Key == Key.O && Keyboard.Modifiers == ModifierKeys.Control)
            {
                OpenItem_Click(sender, null);
            }
            else if (e.Key == Key.N && Keyboard.Modifiers == ModifierKeys.Control)
            {
                NewItem_Click(sender, null);
            }
            else if (e.Key == Key.N && Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                SaveAsItem_Click(sender, null);
            }
        }

        #endregion
    }
}
