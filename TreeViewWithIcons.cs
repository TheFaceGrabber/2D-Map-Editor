using System;
using System.Collections.Generic;
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

namespace WpfApp1
{
    public class TreeViewWithIcons : TreeViewItem
    {
        #region Global variables
        ImageSource iconSource;
        TextBlock textBlock;
        Image icon;
        #endregion Global variables

        #region Constructors and Destructors
        public TreeViewWithIcons()
        {
            StackPanel stack = new StackPanel();
            stack.Orientation = Orientation.Horizontal;
            Header = stack;
            //Uncomment this code If you want to add an Image after the Node-HeaderText
            //textBlock = new TextBlock();
            //textBlock.VerticalAlignment = VerticalAlignment.Center;
            //stack.Children.Add(textBlock);
            icon = new Image();
            icon.VerticalAlignment = VerticalAlignment.Center;
            icon.Margin = new Thickness(0, 0, 4, 0);
            icon.Source = iconSource;
            stack.Children.Add(icon);
            //Add the HeaderText After Adding the icon
            textBlock = new TextBlock();
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            stack.Children.Add(textBlock);
        }
        #endregion Constructors and Destructors
        #region Properties
        /// <summary>
        /// Gets/Sets the Selected Image for a TreeViewNode
        /// </summary>
        public ImageSource Icon
        {
            set
            {
                iconSource = value;
                icon.Source = iconSource;
            }
            get
            {
                return iconSource;
            }
        }
        #endregion Properties
        #region Event Handlers
        /// <summary>
        /// Event Handler on UnSelected Event
        /// </summary>
        /// <param name="args">Eventargs</param>
        protected override void OnUnselected(RoutedEventArgs args)
        {
            base.OnUnselected(args);
            icon.Source = iconSource;
        }
        /// <summary>
        /// Event Handler on Selected Event 
        /// </summary>
        /// <param name="args">Eventargs</param>
        protected override void OnSelected(RoutedEventArgs args)
        {
            base.OnSelected(args);
            icon.Source = iconSource;
        }

        /// <summary>
        /// Gets/Sets the HeaderText of TreeViewWithIcons
        /// </summary>
        public string HeaderText
        {
            set
            {
                textBlock.Text = value;
            }
            get
            {
                return textBlock.Text;
            }
        }
        #endregion Event Handlers
    }
}
