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
        private Point? _oldMouseMovePoint;
        private bool _doDrag;
        private int _dragDirection;

        public MainWindow()
        {
            InitializeComponent();
            myLoopList.SetAutoDragOffset(0.55);
            myLoopList.SetDuration(new Duration(new TimeSpan(2000000))); //200ms
            string[] paths = Directory.GetFiles(Environment.CurrentDirectory + @"\images", "tele*");
            Node anchor = null;
            Node anchorForMokup = null;

            for (int i = 0; i < paths.Count(); i++)
            {
                string path = paths[i];
                Grid grid = new Grid();
                Button button = new Button
                    {
                        Content = "button " + (i + 2)
                    };
                button.Click += printName;
                button.MaxHeight = 50;

                Image img = new Image
                    {
                        Stretch = Stretch.Fill, 
                        Source = loadImage(path)
                    };
                grid.Children.Add(img);
                grid.Children.Add(button);
                if (i != 3) {
                    anchor = myLoopList.AddToRight(anchor, grid);
                    if (i == 1)
                    {
                        anchorForMokup = anchor;
                    }
                }
                else
                    anchor = myLoopList.AddToAbove(anchor, grid);
            }
            Grid mokupGrid = new Grid();

            Image mokuImg = new Image
                {
                    Stretch = Stretch.Fill,
                    Source = loadImage(Environment.CurrentDirectory + @"\images\mokup.jpg")
                };

            mokupGrid.Children.Add(mokuImg);

            myLoopList.AddToAbove(anchorForMokup, mokupGrid);
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
            if (!_doDrag) return;
            Point currentPos = e.GetPosition(myLoopList);
            if (!_oldMouseMovePoint.HasValue)
                _oldMouseMovePoint = currentPos;
            if (Math.Abs(_oldMouseMovePoint.Value.X - currentPos.X) < 0.000000001 && Math.Abs(_oldMouseMovePoint.Value.Y - currentPos.Y) < 0.000000001)
            {
                return;
            }

            int xDistance = (int)(currentPos.X - _oldMouseMovePoint.Value.X);
            int yDistance = (int)(currentPos.Y - _oldMouseMovePoint.Value.Y);

            _dragDirection = Math.Abs(xDistance) >= Math.Abs(yDistance) ? 1 : 2;
            bool mayDragOn = false;
            if (_dragDirection == 1)
            {
                mayDragOn = myLoopList.HDrag(xDistance);
            }
            if (_dragDirection == 2)
            {
                mayDragOn = myLoopList.VDrag(yDistance);
            }
            if (!mayDragOn)
            {
                _doDrag = false;
            }
            _oldMouseMovePoint = currentPos;
        }

        private void myLoopList_MouseUp_1(object sender, MouseButtonEventArgs e)
        {
            _doDrag = false;
            _oldMouseMovePoint = null;
            myLoopList.AnimBack();
            _dragDirection = 0;
        }

        private void myLoopList_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            _doDrag = true;
        }


        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                myLoopList.AnimH(true);
            }
            if (e.Key == Key.Right)
            {
                myLoopList.AnimH(false);

            }
            if (e.Key == Key.Up)
            {
                myLoopList.AnimV(true);
            }
            if (e.Key == Key.Down)
            {
                myLoopList.AnimV(false);
            }
            e.Handled = true;
        }

        private void myLoopList_MouseLeave_1(object sender, MouseEventArgs e)
        {
            myLoopList_MouseUp_1(null, null);
        }

    }
}
