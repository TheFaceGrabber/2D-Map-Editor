using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;

namespace WpfApp1
{
    public class LevelImage
    {
        public string Img;
        public System.Windows.Point Location;

        public BitmapImage ImgScr { get; private set; }

        public LevelImage(string image, System.Windows.Point loc)
        {
            Img = image;
            Location = loc;

            ImgScr = new BitmapImage(new Uri(Img, UriKind.Relative));
        }
    }

    public class ImageCanvas : Canvas
    {
        public static readonly int GRIDSIZE = 32;

        public List<LevelImage> Images = new List<LevelImage>();

        Level level;


        BitmapImage grid;
        BitmapImage convertedGrid
        {
            get
            {
                if (grid == null)
                    grid = Convert(Properties.Resources.Grid);

                return grid;
            }
        }

        public BitmapImage Convert(Bitmap src)
        {
            MemoryStream ms = new MemoryStream();
            ((System.Drawing.Bitmap)src).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }

        public void Update(Level lvl)
        {
            level = lvl;
            InvalidateVisual();
        }
        public bool IsValidURI(string uri)
        {
            var temp = System.IO.Directory.GetCurrentDirectory() + @"\" + uri;
            if (File.Exists(temp)) return true;

            return false;
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (level != null)
            {
                for (int y = 0; y < level.LevelSize; y++)
                {
                    if (string.IsNullOrEmpty(level.LevelGrid[y])) continue;

                    for (int x = 0; x < level.LevelGrid[y].Length; x++)
                    {
                        var r = new Rect(x * GRIDSIZE, (y + 1) * GRIDSIZE, GRIDSIZE, GRIDSIZE);
                        if (level.LevelGrid[y][x] == ' ')
                        {
                            dc.DrawImage(convertedGrid, r);
                        }
                        else
                        {
                            var block = level.Blocks.SingleOrDefault(v => v.Char == level.LevelGrid[y][x]);
                            if (block != null && IsValidURI(block.Image) && block.Source != null)
                            {
                                var img = block.Source;
                                dc.DrawImage(img, r);
                            }
                        }
                    }
                }
                
                foreach (var item in level.Entities)
                {
                    if (string.IsNullOrEmpty(item.Image) || !IsValidURI(item.Image)) continue;

                    dc.DrawImage(item.Source,
                        new Rect(item.Position.X * GRIDSIZE, item.Position.Y * GRIDSIZE, GRIDSIZE * item.Size, GRIDSIZE * item.Size));
                }
                
            }
            base.OnRender(dc);
        }
    }
}
