using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

namespace LoopListTest
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Point? oldMouseMovePoint;
        private bool doDrag;

        public MainWindow()
        {
            InitializeComponent();
            myLoopList.setAutoDragOffset(0.55);
            string[] paths = Directory.GetFiles(Environment.CurrentDirectory + @"\images");
            for (int i = 0; i < paths.Count(); i++)
            {
                string path = paths[i];
                Grid grid = new Grid();
                grid.RowDefinitions.Add(new RowDefinition());
                grid.RowDefinitions.Add(new RowDefinition());
                Button button = new Button();
                button.Content = "button " + (i + 2);
                button.Click += printName;

                Image img = new Image();
                img.Stretch = Stretch.Fill;
                img.Source = loadData(path);
                grid.Children.Add(img);
                grid.Children.Add(button);
                button.SetValue(Grid.RowProperty, 1);
                myLoopList.add(grid);
            }
        }

        void printName(object sender, EventArgs e)
        {
            Debug.WriteLine(((Button)sender).Content);
        }

        private BitmapImage loadData(string path)
        {
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
            bi.EndInit();
            return bi;
        }


        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                myLoopList.animH(true);
            }
            if (e.Key == Key.Right)
            {
                myLoopList.animH(false);
                
            }
        }

        private void myLoopList_MouseMove_1(object sender, MouseEventArgs e)
        {
            if (doDrag)
            {
                
                Point currentPos = e.GetPosition(myLoopList);
                if (!oldMouseMovePoint.HasValue)
                {
                    oldMouseMovePoint = currentPos;
                }
                if (oldMouseMovePoint.HasValue && oldMouseMovePoint.Value.X == currentPos.X)
                {
                    return;
                }
                
                int xDistance = (int)(currentPos.X - oldMouseMovePoint.Value.X);
                bool mayDragOn = myLoopList.drag(xDistance);
                if (!mayDragOn)
                {
                    doDrag = false;
                }
                oldMouseMovePoint = currentPos;
            }
        }

        private void myLoopList_MouseUp_1(object sender, MouseButtonEventArgs e)
        {
            doDrag = false;
            oldMouseMovePoint = null;
            myLoopList.animBack();
        }

        private void myLoopList_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            doDrag = true;
        }

    }
}
