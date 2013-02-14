using LoopList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace LoopListTest
{
    /// <summary>
    /// Interaktionslogik für LoopListTestMain.xaml
    /// </summary>
    public partial class LoopListTestMain
    {
        private Point? _oldMovePoint;
        private bool _doDrag;
        private bool _waitForTextList;
        private bool _mouseIsUp;

        private readonly List<Orientation> _savedDirections = new List<Orientation>();
        private bool _dragDirectionIsObvious;



        public LoopListTestMain()
        {
            InitializeComponent();
            try
            {
                InitList();
            }
            catch (Exception exc)
            {
                ExceptionTextBlock.Text = exc.Message + "\r\n" + exc.InnerException;
            }
        }

        private void InitList()
        {
            MyLoopList.SetAutoDragOffset(0.5);
            MyLoopList.SetDuration(new Duration(new TimeSpan(3000000))); //300m
            MyLoopList.Scrolled += MyLoopListOnScrolled;
            MyTextLoopList.Scrolled += MyTextLoopList_Scrolled;
            MyTextLoopList.SetFontSize(36);
            MyTextLoopList.SetFontFamily("Miriam Fixed");
            MyTextLoopList.SetDuration(new Duration(new TimeSpan(5500000)));

            string[] paths = Directory.GetFiles(Environment.CurrentDirectory + @"\images\Beach");
            Image img = new Image { Source = new BitmapImage(new Uri(paths[0], UriKind.RelativeOrAbsolute)) };
            Node node1 = MyLoopList.AddNewToLeft(null, img);
            img = new Image { Source = new BitmapImage(new Uri(paths[1], UriKind.RelativeOrAbsolute)) };
            Node node2 = MyLoopList.AddNewToLeft(null, img);
            node1.Right = node2;
            node2.Left = node1;
            node2.Below = node1;
            node2.Right = node1;
            Node nodeHallo = MyLoopList.AddNewToAbove(null, new Button { Content = "Hallo", Width = 40, Height = 40 });
            Node nodeToll = MyLoopList.AddNewToLeft(nodeHallo, new Button { Content = "Toll, ne?", Width = 100, Height = 40 });
            node2.Above = nodeToll;
            nodeToll.Below = nodeHallo;
            nodeToll.Above = nodeHallo;
            nodeHallo.Above = nodeToll;
            nodeHallo.Below = nodeToll;
            MyTextLoopList.Add("lol");
            MyTextLoopList.Add("käse");
            MyTextLoopList.Add("Test");
            MyTextLoopList.Add("Ahjo");
            MyTextLoopList.Add("a");
            MyTextLoopList.Add("b");
            MyTextLoopList.Add("c");
        }





        /*Erst wenn die Scrollanimation der TextLoopList beendet ist, darf die LoopList weiterscrollen (vertical).*/
        private void MyTextLoopList_Scrolled(object sender, EventArgs e)
        {
            _waitForTextList = false;
            if (!_mouseIsUp)
                _doDrag = true;
        }

        /*Wenn die LoopList vertical gescrollt wurde, wird die TextLoopList gescrollt.*/
        private void MyLoopListOnScrolled(object sender, EventArgs e)
        {
            if (e != null)
            {
                switch (((LoopListArgs)e).GetDirection())
                {
                    case Direction.Top:
                        _waitForTextList = MyTextLoopList.Anim(true);
                        break;
                    case Direction.Down:
                        _waitForTextList = MyTextLoopList.Anim(false);
                        break;
                }
                ResetDragDirectionObvious();
                if (!_mouseIsUp)
                    _doDrag = true;
            }
        }

        private void Drag(Point currentPos)
        {
            try
            {
                if (!_doDrag)
                    return;
                if (!_oldMovePoint.HasValue)
                    _oldMovePoint = currentPos;
                if (Math.Abs(_oldMovePoint.Value.X - currentPos.X) < 0.000000001 &&
                    Math.Abs(_oldMovePoint.Value.Y - currentPos.Y) < 0.000000001)
                    return; //keine Bewegung?

                int xDistance = (int)(currentPos.X - _oldMovePoint.Value.X);
                int yDistance = (int)(currentPos.Y - _oldMovePoint.Value.Y);

                Orientation dragDirection = Math.Abs(xDistance) >= Math.Abs(yDistance) ? Orientation.Horizontal : Orientation.Vertical;
                if (!_dragDirectionIsObvious)
                {
                    if (_savedDirections.Count < 4)
                    {
                        _savedDirections.Add(dragDirection);
                        return;
                    }
                    int xCount = 0;
                    int yCount = 0;
                    foreach (Orientation dir in _savedDirections)
                    {
                        switch (dir)
                        {
                            case Orientation.Horizontal:
                                xCount++;
                                break;
                            case Orientation.Vertical:
                                yCount++;
                                break;
                        }
                    }
                    int greater = Math.Max(xCount, yCount);
                    int lower = Math.Min(xCount, yCount);
                    if (lower / (double)greater < 0.15) //x- und y-Entwicklung unterscheiden sich deutlich.
                    {
                        _dragDirectionIsObvious = true;
                        dragDirection = greater == xCount ? Orientation.Horizontal : Orientation.Vertical;
                        KinectVibratingRectangle.Visibility = Visibility.Collapsed;
                    }
                    _savedDirections.Clear();
                    if (!_dragDirectionIsObvious)
                    {
                        KinectVibratingRectangle.Visibility = Visibility.Visible;
                        return;
                    }
                }

                bool mayDragOn = false;
                if (dragDirection == Orientation.Horizontal)
                {
                    mayDragOn = MyLoopList.HDrag(xDistance);
                }
                else
                {
                    if (!_waitForTextList) //<-- nervt doch nur ... <-- nein das dient der synchronisation zwischen linkem text und looplist.
                        mayDragOn = MyLoopList.VDrag(yDistance);
                }
                if (!mayDragOn) _doDrag = false;
            }
            catch (Exception exc)
            {
                ExceptionTextBlock.Text = exc.Message + "\r\n" + exc.InnerException;
            }
            finally
            {
                _oldMovePoint = currentPos;
            }
        }

        private void myLoopList_MouseMove_1(object sender, MouseEventArgs e)
        {
            Drag(e.GetPosition(MyLoopList));
        }

        private void myLoopList_MouseUp_1(object sender, MouseButtonEventArgs e)
        {
            try
            {
                KinectFocusedRectangle.Visibility = Visibility.Collapsed;
                _mouseIsUp = true;
                ResetDragDirectionObvious();

                _doDrag = false;
                _oldMovePoint = null;
               // MyLoopList.AnimBack(); //zurueckspringen des Bildes

            }
            catch (Exception exc)
            {
                ExceptionTextBlock.Text = exc.Message + "\r\n" + exc.InnerException;
            }
        }

        private void ResetDragDirectionObvious()
        {
            _dragDirectionIsObvious = false;
            KinectVibratingRectangle.Visibility = Visibility.Collapsed;
            _savedDirections.Clear();
        }

        private void myLoopList_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            _mouseIsUp = false;
            _doDrag = true;
            KinectFocusedRectangle.Visibility = Visibility.Visible;
        }

        public void DelegateKeyEvent(KeyEventArgs e)
        {
            OnKeyDown(e);
        }
        /*Tastensteuerung der LoopList*/
        protected override void OnKeyDown(KeyEventArgs e)
        {
            try
            {
                switch (e.Key)
                {
                    case Key.Left:
                        MyLoopList.AnimH(true);
                        break;
                    case Key.Right:
                        MyLoopList.AnimH(false);
                        break;
                    case Key.Up:
                        if (!_waitForTextList)
                            MyLoopList.AnimV(true);
                        break;
                    case Key.Down:
                        if (!_waitForTextList)
                            MyLoopList.AnimV(false);
                        break;
                    case Key.NumPad4:
                        MyLoopList.HDragPercent(-0.25);
                        break;
                    case Key.NumPad6:
                        MyLoopList.HDragPercent(0.25);
                        break;
                    case Key.NumPad8:
                        if (!_waitForTextList)
                            MyLoopList.VDragPercent(-0.25);
                        break;
                    case Key.NumPad2:
                        if (!_waitForTextList)
                            MyLoopList.VDragPercent(0.25);
                        break;
                    default:
                        //Environment.Exit(0);
                        break;
                }
                e.Handled = true;
            }
            catch (Exception exc)
            {
                ExceptionTextBlock.Text = exc.Message + "\r\n" + exc.InnerException;
            }
        }

        /*MouseLeave wird wie MouseUp behandelt*/
        private void myLoopList_MouseLeave_1(object sender, MouseEventArgs e)
        {
            myLoopList_MouseUp_1(null, null);
        }
    }
}
