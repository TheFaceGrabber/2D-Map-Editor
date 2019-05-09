using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;
using Xceed.Wpf.Toolkit.PropertyGrid.Converters;
using Xceed.Wpf.Toolkit.PropertyGrid.Commands;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace WpfApp1
{
    public class ImageEditor : ITypeEditor
    {
        private void button_Click(object sender, RoutedEventArgs e)
        {
            PropertyItem item = ((Button)sender).Tag as PropertyItem;
            if (null == item)
            {
                return;
            }

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            var temp = System.IO.Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory() + @"\" + (string)item.Value);
            dlg.InitialDirectory = string.IsNullOrEmpty((string)item.Value) ? System.IO.Directory.GetCurrentDirectory() : temp;
            dlg.Filter = "All Images|*.BMP;*.DIB;*.RLE;*.JPG;*.JPEG;*.JPE;*.JFIF;*.GIF;*.TIF;*.TIFF;*.PNG|" +
                            "BMP Files: (*.BMP;*.DIB;*.RLE)|*.BMP;*.DIB;*.RLE|" +
                            "JPEG Files: (*.JPG;*.JPEG;*.JPE;*.JFIF)|*.JPG;*.JPEG;*.JPE;*.JFIF|" +
                            "GIF Files: (*.GIF)|*.GIF|" +
                            "TIFF Files: (*.TIF;*.TIFF)|*.TIF;*.TIFF|" +
                            "PNG Files: (*.PNG)|*.PNG|" +
                            "All Files|*.*";
            var r= dlg.ShowDialog();
            if (r == false) return;
            var loc = dlg.FileName;
            if(loc.StartsWith(System.IO.Directory.GetCurrentDirectory()))
            {
                item.Value = loc.Remove(0, System.IO.Directory.GetCurrentDirectory().Length + 1);
            }
            else
            {
                MessageBox.Show("All images must be within working directory!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        FrameworkElement ITypeEditor.ResolveEditor(PropertyItem propertyItem)
        {
            Grid panel = new Grid();
            panel.ColumnDefinitions.Add(new ColumnDefinition());
            panel.ColumnDefinitions.Add(new ColumnDefinition()
            {
                Width = GridLength.Auto
            });

            TextBox textBox = new TextBox();
            textBox.HorizontalAlignment = HorizontalAlignment.Stretch;
            Binding binding = new Binding("Value"); //bind to the Value property of the PropertyItem
            binding.Source = propertyItem;
            binding.Mode = BindingMode.TwoWay;
            BindingOperations.SetBinding(textBox, TextBox.TextProperty, binding);

            Button button = new Button();
            button.Content = "...";
            button.Tag = propertyItem;
            button.Click += button_Click;
            Grid.SetColumn(button, 1);

            panel.Children.Add(textBox);
            panel.Children.Add(button);

            return panel;
        }
    }

    public class AppImage
    {
        [Editor(typeof(ImageEditor), typeof(ImageEditor))]
        public string ImagePath
        {
            get { return path; }
            set
            {
                path = value;
                Source = new BitmapImage(new Uri(value, UriKind.Relative));
            }
        }

        public BitmapImage Source
        {
            get;
            set;
        }

        string path = "";
    }
}
