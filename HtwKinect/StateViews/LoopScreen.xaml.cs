using AccessoryLib;
using LoopList;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RectNavigation;
using Database;
using System.Diagnostics;
using Database.DAO;
using MiniGame;
using System.Windows.Media;

namespace HtwKinect.StateViews
{
    /// <summary>
    /// Interaktionslogik für LoopScreen.xaml
    /// </summary>
    public partial class LoopScreen : UserControl, ISwitchableUserControl
    {
        private Point? _oldMovePoint;
        private bool _doDrag;
        private bool _waitForTextList;
        private bool _unclicked;
        private KinectProjectUiBuilder _kinectProjectUiBuilder;
        private String _gender;
        private int lastControlSkeleton = -1;
        public String Gender
        {
            get { return _gender = _gd.Gender; }
            set { _gender = value; }
        }
        private TravelOffer _currentOffer;
        private bool mouseOn = false;
        private bool _isGameActive = false;

        private readonly List<Orientation> _savedDirections = new List<Orientation>();
        private bool _dragDirectionIsObvious;
        private GenderDetector.GenderDetectorControl _gd;

        public void SwipeLeft(object sender, EventArgs e)
        {
            if (_unclicked)
            {
                Click(new Point(0, 0));
            }
            SwipeArgs sa = (SwipeArgs)e;
            Point newPoint = new Point(-MyLoopList.GetDraggableHLength() * sa.Progress, 0);
            Drag(newPoint, 0);
        }

        public void SwipeRight(object sender, EventArgs e)
        {
            if (_unclicked)
            {
                Click(new Point(0, 0));
            }
            SwipeArgs sa = (SwipeArgs)e;
            Point newPoint = new Point(MyLoopList.GetDraggableHLength() * sa.Progress, 0);
            Drag(newPoint, 0);
        }

        public void SwipeUp(object sender, EventArgs e)
        {
            if (_unclicked)
            {
                Click(new Point(0, 0));
            }
            SwipeArgs sa = (SwipeArgs)e;
            Point newPoint = new Point(0, MyLoopList.GetDraggableVLength() * sa.Progress);
            Drag(newPoint, 0);
        }

        public void SwipeDown(object sender, EventArgs e)
        {
            if (_unclicked)
            {
                Click(new Point(0, 0));
            }
            SwipeArgs sa = (SwipeArgs)e;
            Point newPoint = new Point(0, -MyLoopList.GetDraggableVLength() * sa.Progress);
            Drag(newPoint, 0);
        }

        public void NoSwipe(object sender, EventArgs e)
        {
            UnClick();
        }

        private void InitList()
        {
            MyLoopList.SetAutoDragOffset(0.50);
            MyLoopList.SetDuration(new Duration(new TimeSpan(3000000))); //300m
            MyLoopList.Scrolled += MyLoopListOnScrolled;
            MyTextLoopList.Scrolled += MyTextLoopList_Scrolled;
            MyTextLoopList.SetFontSize(36);
            MyTextLoopList.SetFontColor(Colors.White);
            //MyTextLoopList.SetWordWrap(TextWrapping.Wrap);
            //MyTextLoopList.SetFontFamily("Miriam Fixed");
            MyTextLoopList.SetDuration(new Duration(new TimeSpan(5500000)));
            LoadPictures(new LocalPictureUiLoader());
        }

        private void InitGenderDetection()
        {
            var helper = KinectHelper.Instance;
            _gd = new GenderDetector.GenderDetectorControl();
            _gd.Start(KinectHelper.Instance.Sensor);
            helper.ReadyEvent += (s, _) => GenderTestEvent();
        }


          private void GenderTestEvent() 
          {
              if (KinectHelper.Instance.GetFixedSkeleton() != null)
              {
                  if (lastControlSkeleton != KinectHelper.Instance.GetFixedSkeleton().TrackingId)
                  {
                      lastControlSkeleton = KinectHelper.Instance.GetFixedSkeleton().TrackingId;
                      if (_gd != null)
                      {
                          _gd.GenderCheck(KinectHelper.Instance.GetFixedSkeleton(), KinectHelper.Instance.ColorPixels);
                          Gender = _gd.Gender;
                      }
                  }
              }
          }

        private void LoadPictures(IUiLoader uiLoader)
        {
            _kinectProjectUiBuilder = new KinectProjectUiBuilder(MyLoopList, MyTextLoopList);
            uiLoader.LoadElementsIntoList(_kinectProjectUiBuilder, _currentOffer);
            //string[] paths = Directory.GetFiles(Environment.CurrentDirectory + @"\images\Top");
            //Image img = new Image {Source = new BitmapImage(new Uri(paths[0], UriKind.RelativeOrAbsolute)) };
            //Node node1 = MyLoopList.AddNewToLeft(null, img);
            //img = new Image { Source = new BitmapImage(new Uri(paths[1], UriKind.RelativeOrAbsolute)) };
            //Node node2 = MyLoopList.AddNewToLeft(null, img);
            //node1.Right = node2;
            //node2.Left = node1;
            //node2.Below = node1;
            //node2.Right = node1;
            //MyTextLoopList.Add("lol");
        }

        /*Callback fur ein fertiges Frame vom Kinect-Sensor*/
        private void HelperReady()
        {
            var helper = KinectHelper.Instance;
            Skeleton skeleton = helper.GetFixedSkeleton();
            if (skeleton != null)
                RectNavigationControl.GestureRecognition(skeleton);
            GreenScreen.RenderImageData(helper.DepthImagePixels, helper.ColorPixels);
            SetAccessoriesNew(helper);
            KinectHelper.Instance.SetTransform(GreenScreen);
            KinectHelper.Instance.SetTransform(Accessories);
            KinectHelper.Instance.SetTransform(RectNavigationControl);
        }

        private void SetAccessoriesNew(KinectHelper helper)
        {
            Skeleton activeSkeleton = helper.GetFixedSkeleton();
            if (activeSkeleton != null) 
            {
                if (helper.Skeletons.Length > 1 && Accessories.AccessoryRect != null)
                {
                    Rect accessoryRect = Accessories.AccessoryRect;
                    var left = helper.Sensor.CoordinateMapper.MapSkeletonPointToColorPoint(activeSkeleton.Joints[JointType.HandLeft].Position, helper.Sensor.ColorStream.Format);
                    if (left.X >= accessoryRect.Left && left.X <= accessoryRect.Right &&
                        left.Y >= accessoryRect.Top + accessoryRect.Height && left.Y <= accessoryRect.Bottom + accessoryRect.Height)
                    {
                        activeSkeleton = helper.SetNewFixedSkeleton();
                    }
                }
            }
            Accessories.SetActiveSkeleton(activeSkeleton);
        }
       
        /*Erst wenn die Scrollanimation der TextLoopList beendet ist, darf die LoopList weiterscrollen (vertical).*/
        private void MyTextLoopList_Scrolled(object sender, EventArgs e)
        {
            _waitForTextList = false;
            if (!_unclicked)
                _doDrag = true;
            string[] texts = MyTextLoopList.GetNeighbourTexts();
            RectNavigationControl.SetTopText(texts[0]);
            RectNavigationControl.SetBottomText(texts[1]);
        }

        /*Wenn die LoopList vertical gescrollt wurde, wird die TextLoopList gescrollt.*/
        private void MyLoopListOnScrolled(object sender, EventArgs e)
        {
            if (e != null)
            {
                LoopListArgs lla = (LoopListArgs)e;

                if (lla.GetId() != -1) // Wenn Minigame
                {
                    GreenScreen.Opacity = 1;
                    Accessories.Opacity = 1;
                    _isGameActive = false;
                    _currentOffer = new TravelOfferDao().SelectById(lla.GetId());
                }
                else 
                {
                    _isGameActive = true;
                    GreenScreen.Opacity = 0.2;
                    Accessories.Opacity = 0.2;
                }

                SetNewHat();

                switch (lla.GetDirection())
                {
                    case Direction.Top:
                        _waitForTextList = MyTextLoopList.Anim(true);
                        break;
                    case Direction.Down:
                        _waitForTextList = MyTextLoopList.Anim(false);
                        break;
                }
                ResetDragDirectionObvious();
                if (!_unclicked)
                    _doDrag = true;
            }
        }

        private void Drag(Point currentPos, int testDirectionTimes)
        {
            try
            {
                if (!_doDrag)
                    return;

                if (Math.Abs(_oldMovePoint.Value.X - currentPos.X) < 0.000000001 &&
                    Math.Abs(_oldMovePoint.Value.Y - currentPos.Y) < 0.000000001)
                    return; //keine Bewegung?

                double xDistance = currentPos.X - _oldMovePoint.Value.X;
                double yDistance = currentPos.Y - _oldMovePoint.Value.Y;

                Orientation dragDirection = Math.Abs(xDistance) >= Math.Abs(yDistance) ? Orientation.Horizontal : Orientation.Vertical;
                if (!_dragDirectionIsObvious && testDirectionTimes > 0)
                {
                    if (_savedDirections.Count < testDirectionTimes)
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
                    if (lower/(double) greater < 0.15) //x- und y-Entwicklung unterscheiden sich deutlich.
                    {
                        _dragDirectionIsObvious = true;
                        dragDirection = greater == xCount ? Orientation.Horizontal : Orientation.Vertical;
                    }
                    _savedDirections.Clear();
                    if (!_dragDirectionIsObvious)
                    {
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
            if (mouseOn)
                Drag(e.GetPosition(MyLoopList), 20);
        }

        private void myLoopList_MouseUp_1(object sender, MouseButtonEventArgs e)
        {
            if (mouseOn)
                UnClick();
        }

        public void UnClick()
        {
            try
            {
                _oldMovePoint = null;
                _unclicked = true;
                ResetDragDirectionObvious();

                _doDrag = false;
                MyLoopList.AnimBack(); //zurueckspringen des Bildes

            }
            catch (Exception exc)
            {
                ExceptionTextBlock.Text = exc.Message + "\r\n" + exc.InnerException;
            }
        }

        private void ResetDragDirectionObvious()
        {
            _dragDirectionIsObvious = false;
            _savedDirections.Clear();
        }

        private void myLoopList_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            if (mouseOn)
                Click(e.GetPosition(MyLoopList));
        }

        private void Click(Point point)
        {
            _oldMovePoint = point;
            _unclicked = false;
            _doDrag = true;
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
                }
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

        public TravelOffer StopDisplay()
        {
            return _currentOffer;
        }

        public void StartDisplay(TravelOffer lastTravel)
        {
            _currentOffer = lastTravel;
            try
            {
                InitializeComponent();
                InitList();
                var helper = KinectHelper.Instance;
                helper.ReadyEvent += (s, _) => HelperReady();
                GreenScreen.Start(helper.Sensor, true);
                SetNewHat();
                Accessories.Start(helper.Sensor);
                RectNavigationControl.Start(helper.Sensor);
                RectNavigationControl.SwipeLeftEvent += SwipeLeft;
                RectNavigationControl.SwipeRightEvent += SwipeRight;
                RectNavigationControl.SwipeUpEvent += SwipeUp;
                RectNavigationControl.SwipeDownEvent += SwipeDown;
                RectNavigationControl.NoSwipe += NoSwipe;
                string[] texts = MyTextLoopList.GetNeighbourTexts();
                RectNavigationControl.SetTopText(texts[0]);
                RectNavigationControl.SetBottomText(texts[1]);
                InitGenderDetection();
            }
            catch (Exception exc)
            {
                ExceptionTextBlock.Text = exc.Message + "\r\n" + exc.InnerException + "\r\n" + exc.StackTrace;
            }
        }

        public bool IsGame()
        {
            return (_isGameActive && KinectHelper.Instance.GetFixedSkeleton() != null);
        }

        private void SetNewHat()
        {
            Accessories.AccessoryItems.Clear();
            AccessoryItem hat = new AccessoryItem(AccessoryPositon.Hat, _currentOffer.Category.CategoryId, _gender == "Female" ? true : false);
            Accessories.AccessoryItems.Add(hat);
        }
    }
}
