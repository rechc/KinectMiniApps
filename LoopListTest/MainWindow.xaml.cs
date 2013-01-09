using LoopList;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LoopListTest
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private Point? _oldMouseMovePoint;
        private bool _doDrag;
        private int _dragDirection;
        private bool _waitForTextList = false;

        public MainWindow()
        {
            InitializeComponent();
            MyLoopList.SetAutoDragOffset(0.55);
            MyLoopList.SetDuration(new Duration(new TimeSpan(3000000))); //300m
            MyLoopList.Scrolled += MyLoopListOnScrolled;
            MyTextLoopList.Scrolled += MyTextLoopList_Scrolled;
            MyTextLoopList.SetFontSize(16);
            MyTextLoopList.SetFontFamily("Miriam Fixed");
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
                button.Click += PrintName;
                button.MaxHeight = 50;

                Image img = new Image
                    {
                        Stretch = Stretch.Fill, 
                        Source = LoadImage(path)
                    };
                grid.Children.Add(img);
                grid.Children.Add(button);
                if (i != 3) {
                    anchor = MyLoopList.AddToRight(anchor, grid);
                    if (i == 1)
                    {
                        anchorForMokup = anchor;
                    }
                }
                else
                    anchor = MyLoopList.AddToAbove(anchor, grid);
            }
            Grid mokupGrid = new Grid();

            Image mokuImg = new Image
                {
                    Stretch = Stretch.Fill,
                    Source = LoadImage(Environment.CurrentDirectory + @"\images\mokup.jpg")
                };

            mokupGrid.Children.Add(mokuImg);

            MyLoopList.AddToAbove(anchorForMokup, mokupGrid);
            
            MyTextLoopList.Add("Ebene5");
            MyTextLoopList.Add("Ebene4");
            MyTextLoopList.Add("Ebene3");
            MyTextLoopList.Add("Ebene2");
            MyTextLoopList.Add("Ebene1");
            


        }

        private void MyTextLoopList_Scrolled(object sender, EventArgs e)
        {
            _waitForTextList = false;
        }

        private void MyLoopListOnScrolled(object sender, EventArgs e)
        {
            if (e != null)
                if (((LoopListArgs) e).GetDirection() == Direction.Top)
                {
                    MyTextLoopList.Anim(true);
                    _waitForTextList = true;

                }
                else
                {
                    if (((LoopListArgs) e).GetDirection() == Direction.Down)
                    {
                        MyTextLoopList.Anim(false);
                        _waitForTextList = true;

                    }
                }
                
        }

        static void PrintName(object sender, EventArgs e)
        {
            Debug.WriteLine(((Button)sender).Content);

        }

        private static BitmapImage LoadImage(string path)
        {
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
            bi.EndInit();
            return bi;
        }



        private void myLoopList_MouseMove_1(object sender, MouseEventArgs e)
        {
            if (!_doDrag)
            {
                return;
            }
            if (_waitForTextList) {
                myLoopList_MouseUp_1(null, null);
                return;
            }
            Point currentPos = e.GetPosition(MyLoopList);
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
                mayDragOn = MyLoopList.HDrag(xDistance);
            }
            if (_dragDirection == 2)
            {
                mayDragOn = MyLoopList.VDrag(yDistance);
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
            MyLoopList.AnimBack();
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
                MyLoopList.AnimH(true);
            }
            if (e.Key == Key.Right)
            {
                MyLoopList.AnimH(false);

            }
            if (!_waitForTextList)
            {
                if (e.Key == Key.Up)
                {
                    MyLoopList.AnimV(true);
                }
                if (e.Key == Key.Down)
                {
                    MyLoopList.AnimV(false);
                }
            }
            e.Handled = true;
        }

        private void myLoopList_MouseLeave_1(object sender, MouseEventArgs e)
        {
            myLoopList_MouseUp_1(null, null);
        }

    }
}
