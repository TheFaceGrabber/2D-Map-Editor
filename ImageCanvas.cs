using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfApp1
{

    public class LevelImage
    {
        public string Img;
        public Point Location;

        public BitmapImage ImgScr { get; private set; }

        public LevelImage(string image, Point loc)
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

        Level lvl;

        public void Update(Level lvl)
        {
            this.lvl = lvl;
            this.InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (lvl != null)
            {
                for (int y = 0; y < lvl.LevelSize; y++)
                {
                    if (string.IsNullOrEmpty(lvl.LevelGrid[y])) continue;

                    for (int x = 0; x < lvl.LevelGrid[y].Length; x++)
                    {
                        var r = new Rect(x * GRIDSIZE, (y + 1) * GRIDSIZE, GRIDSIZE, GRIDSIZE);
                        if (lvl.LevelGrid[y][x] == ' ')
                        {
                            dc.DrawImage(new BitmapImage(new Uri(@"Images\Grid.png", UriKind.Relative)), r);
                        }
                        else
                        {
                            try
                            {
                                var img = lvl.Blocks.SingleOrDefault(v => v.Char == lvl.LevelGrid[y][x]).Source;
                                dc.DrawImage(img,
                                   r);
                            }
                            catch { }
                        }
                    }
                }

                foreach (var item in lvl.Entities)
                {
                    if (string.IsNullOrEmpty(item.Image)) continue;

                    dc.DrawImage(new BitmapImage(new Uri(item.Image, UriKind.Relative)),
                        new Rect(item.Position.X * GRIDSIZE, item.Position.Y * GRIDSIZE, GRIDSIZE * item.Size, GRIDSIZE * item.Size));
                }
            }
            base.OnRender(dc);
        }
    }
}
