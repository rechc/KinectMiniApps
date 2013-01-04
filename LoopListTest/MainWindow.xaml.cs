using LoopList;
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
        private int dragDirection;

        public MainWindow()
        {
            InitializeComponent();
            myLoopList.setAutoDragOffset(0.55);
            myLoopList.setDuration(new Duration(new TimeSpan(2000000))); //200ms
            string[] paths = Directory.GetFiles(Environment.CurrentDirectory + @"\images", "tele*");
            Node anchor = null;
            Node anchorForMokup = null;
            for (int i = 0; i < paths.Count(); i++)
            {
                string path = paths[i];
                Grid grid = new Grid();
                Button button = new Button();
                button.Content = "button " + (i + 2);
                button.Click += printName;
                button.MaxHeight = 50;

                Image img = new Image();
                img.Stretch = Stretch.Fill;
                img.Source = loadImage(path);
                grid.Children.Add(img);
                grid.Children.Add(button);
                if (i != 3) {
                    anchor = myLoopList.addToRight(anchor, grid);
                    if (i == 1)
                    {
                        anchorForMokup = anchor;
                    }
                }
                else
                    anchor = myLoopList.addToAbove(anchor, grid);
            }
            Grid mokupGrid = new Grid();

            Image mokuImg = new Image();
            mokuImg.Stretch = Stretch.Fill;
            mokuImg.Source = loadImage(Environment.CurrentDirectory + @"\images\mokup.jpg");

            mokupGrid.Children.Add(mokuImg);

            myLoopList.addToAbove(anchorForMokup, mokupGrid);
        }

        void printName(object sender, EventArgs e)
        {
            Debug.WriteLine(((Button)sender).Content);
        }

        private BitmapImage loadImage(string path)
        {
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
            bi.EndInit();
            return bi;
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
                if (oldMouseMovePoint.HasValue && oldMouseMovePoint.Value.X == currentPos.X && oldMouseMovePoint.Value.Y == currentPos.Y)
                {
                    return;
                }

                int xDistance = (int)(currentPos.X - oldMouseMovePoint.Value.X);
                int yDistance = (int)(currentPos.Y - oldMouseMovePoint.Value.Y);

                if (Math.Abs(xDistance) >= Math.Abs(yDistance))
                {
                    dragDirection = 1;
                }
                else
                {
                    dragDirection = 2;
                }
                bool mayDragOn = false;
                if (dragDirection == 1)
                {
                    mayDragOn = myLoopList.hDrag(xDistance);
                }
                if (dragDirection == 2)
                {
                    mayDragOn = myLoopList.vDrag(yDistance);
                }
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
            dragDirection = 0;
        }

        private void myLoopList_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            doDrag = true;
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
            if (e.Key == Key.Up)
            {
                myLoopList.animV(true);
            }
            if (e.Key == Key.Down)
            {
                myLoopList.animV(false);
            }
            e.Handled = true;
        }

        private void myLoopList_MouseLeave_1(object sender, MouseEventArgs e)
        {
            myLoopList_MouseUp_1(null, null);
        }

    }
}
