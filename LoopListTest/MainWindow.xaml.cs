using System.Collections.Generic;
using HandDetection;
using LoopList;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;

namespace LoopListTest
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private Point? _oldMovePoint;
        private bool _doDrag;
        private bool _waitForTextList;
        private bool _mouseIsUp;

        private readonly List<int> _savedDirections = new List<int>();
        private bool _dragDirectionIsObvious;

        private KinectSensor _kinectSensor;
        private Skeleton[] _skeletons;
        private DepthImagePixel[] _depthPixels;
        private int _id = -1;
        private HandTracker _handTracker;

        public MainWindow()
        {
            InitializeComponent();
            try
            {
                InitKinect();
                InitList();
            }
            catch (Exception exc)
            {
                ExceptionTextBlock.Text = exc.Message + "\n\r" + exc.InnerException;
            }
        }

        private void InitList()
        {
            MyLoopList.SetAutoDragOffset(0.20);
            MyLoopList.SetDuration(new Duration(new TimeSpan(3000000))); //300m
            MyLoopList.Scrolled += MyLoopListOnScrolled;
            MyTextLoopList.Scrolled += MyTextLoopList_Scrolled;
            MyTextLoopList.SetFontSize(36);
            MyTextLoopList.SetFontFamily("Miriam Fixed");
            MyTextLoopList.SetDuration(new Duration(new TimeSpan(2500000)));
            KinectProjectUiBuilder kpub = new KinectProjectUiBuilder(MyLoopList, MyTextLoopList);
            string[] paths = Directory.GetFiles(Environment.CurrentDirectory + @"\images", "tele*");

            List<FrameworkElement> list = new List<FrameworkElement>
                    {
                        BuildGrid(paths[0]),
                        BuildGrid(paths[1]),
                    };
            kpub.AddRow("Ebene1", list);
            list = new List<FrameworkElement>
                    {
                        BuildGrid(paths[2]),
                        BuildGrid(paths[3]),
                        BuildGrid(paths[4]),
                    };
            kpub.AddRow("Ebene2", list);
            list = new List<FrameworkElement>
                    {
                        BuildGrid(paths[4]),
                        BuildGrid(Environment.CurrentDirectory + @"\images\mokup.jpg"),
                    };
            kpub.AddRow("Ebene3", list);
        }

        private void InitKinect()
        {
            _handTracker = new HandTracker();
            _kinectSensor = KinectSensor.KinectSensors.FirstOrDefault(s => s.Status == KinectStatus.Connected);

            if (_kinectSensor == null)
            {
                ExceptionTextBlock.Text = "Kein Kinect-Sensor erkannt";
            }
            else
            {
                _kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

                _kinectSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);

                _kinectSensor.SkeletonStream.Enable(new TransformSmoothParameters
                {
                    Smoothing = 0.8f,
                    Correction = 0f,
                    Prediction = 0.2f,
                    JitterRadius = 0.1f,
                    MaxDeviationRadius = 0.8f
                });

                _kinectSensor.AllFramesReady += OnAllReady;

                _kinectSensor.Start();
                _kinectSensor.ElevationAngle = 0;
            }
        }

        private FrameworkElement BuildGrid(string path)
        {
            Grid grid = new Grid();
            Image img = new Image
            {
                Stretch = Stretch.Fill,
                Source = LoadImage(path)
            };
            grid.Children.Add(img);
            return grid;
        }

        private void OnAllReady(object sender, AllFramesReadyEventArgs e)
        {
            try
            {
                using (DepthImageFrame dFrame = e.OpenDepthImageFrame())
                {
                    if (_depthPixels == null) 
                        _depthPixels = new DepthImagePixel[dFrame.PixelDataLength];
                    dFrame.CopyDepthImagePixelDataTo(_depthPixels);
                    using (SkeletonFrame sFrame = e.OpenSkeletonFrame())
                    {
                        if (sFrame == null) return;
                        if (_skeletons == null)
                        {
                            _skeletons = new Skeleton[_kinectSensor.SkeletonStream.FrameSkeletonArrayLength];
                        }
                        sFrame.CopySkeletonDataTo(_skeletons);
                        Skeleton skeleton = GetFixedSkeleton();
                        ProcessSkeleton(skeleton);
                    }
                }
            }
            catch (Exception exc)
            {
                ExceptionTextBlock.Text = exc.Message + "\n\r" + exc.InnerException;
            }
        }

        private void ProcessSkeleton(Skeleton skeleton)
        {
            if (skeleton == null)
            {
                return;
            }

            HandStatus handStatus = _handTracker.GetBufferedHandStatus(_depthPixels,
                                                                       skeleton.Joints[JointType.HandRight],
                                                                       _kinectSensor,
                                                                       DepthImageFormat.Resolution640x480Fps30);
            
            switch (handStatus)
            {
                case HandStatus.Closed:
                    myLoopList_MouseUp_1(null, null);
                    break;
                case HandStatus.Opened:
                    myLoopList_MouseDown_1(null, null);
                    break;
                default:
                    return;
            }
            ColorImagePoint cp = _kinectSensor.CoordinateMapper.MapSkeletonPointToColorPoint(skeleton.Joints[JointType.HandRight].Position, ColorImageFormat.RawBayerResolution640x480Fps30);

            Point currentPoint = new Point(cp.X*6, cp.Y*6);
            Drag(currentPoint);
        }

        private Skeleton GetFixedSkeleton()
        {
            Skeleton skeleton = null;
            if (_skeletons != null)
            {
                if (_id == -1)
                {
                    skeleton = _skeletons.FirstOrDefault(s => s.TrackingState == SkeletonTrackingState.Tracked);
                    if (skeleton != null)
                    {
                        _id = skeleton.TrackingId;
                    }
                }
                else
                {
                    skeleton = _skeletons.FirstOrDefault(s => s.TrackingState == SkeletonTrackingState.Tracked && s.TrackingId == _id);
                    if (skeleton == null)
                    {
                        skeleton = _skeletons.FirstOrDefault(s => s.TrackingState == SkeletonTrackingState.Tracked);
                        if (skeleton != null)
                        {
                            _id = skeleton.TrackingId;
                        }
                        else
                        {
                            _id = -1;
                            
                        }
                    }
                }
            }
            return skeleton;
        }

        private void MyTextLoopList_Scrolled(object sender, EventArgs e)
        {
            _waitForTextList = false;
            if (!_mouseIsUp)
                _doDrag = true;
        }

        private void MyLoopListOnScrolled(object sender, EventArgs e)
        {
            if (e != null)
            {
                switch (((LoopListArgs) e).GetDirection())
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

        private static BitmapImage LoadImage(string path)
        {
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
            bi.EndInit();
            return bi;
        }

        private void Drag(Point currentPos)
        {
            try
            {
                if (!_doDrag) goto exit;
                if (!_oldMovePoint.HasValue)
                    _oldMovePoint = currentPos;
                if (Math.Abs(_oldMovePoint.Value.X - currentPos.X) < 0.000000001 &&
                    Math.Abs(_oldMovePoint.Value.Y - currentPos.Y) < 0.000000001) goto exit;


                int xDistance = (int) (currentPos.X - _oldMovePoint.Value.X);
                int yDistance = (int) (currentPos.Y - _oldMovePoint.Value.Y);

                int dragDirection = Math.Abs(xDistance) >= Math.Abs(yDistance) ? 1 : 2;
                if (!_dragDirectionIsObvious)
                {
                    if (_savedDirections.Count < 4)
                    {
                        _savedDirections.Add(dragDirection);
                        goto exit;
                    }
                    int xCount = 0;
                    int yCount = 0;
                    foreach (int dir in _savedDirections)
                    {
                        if (dir == 1)
                            xCount++;
                        else if (dir == 2)
                            yCount++;
                    }
                    int greater = Math.Max(xCount, yCount);
                    int lower = Math.Min(xCount, yCount);
                    if (lower/(double) greater < 0.15)
                    {
                        _dragDirectionIsObvious = true;
                        dragDirection = greater == xCount ? 1 : 2;
                        KinectVibratingRectangle.Visibility = Visibility.Collapsed;
                    }
                    _savedDirections.Clear();
                    if (!_dragDirectionIsObvious)
                    {
                        KinectVibratingRectangle.Visibility = Visibility.Visible;
                        goto exit;
                    }
                }

                bool mayDragOn = false;
                if (dragDirection == 1)
                {
                    mayDragOn = MyLoopList.HDrag(xDistance);
                }
                if (dragDirection == 2)
                {
                    if (!_waitForTextList)
                        mayDragOn = MyLoopList.VDrag(yDistance);
                }
                if (!mayDragOn) _doDrag = false;
                exit:
                _oldMovePoint = currentPos;
            }
            catch (Exception exc)
            {
                ExceptionTextBlock.Text = exc.Message + "\n\r" + exc.InnerException;
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
                MyLoopList.AnimBack();
                
            }
            catch (Exception exc)
            {
                ExceptionTextBlock.Text = exc.Message + "\n\r" + exc.InnerException;
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
                }

                e.Handled = true;
            }
            catch (Exception exc)
            {
                ExceptionTextBlock.Text = exc.Message + "\n\r" + exc.InnerException;
            }
        }

        private void myLoopList_MouseLeave_1(object sender, MouseEventArgs e)
        {
            myLoopList_MouseUp_1(null, null);
        }

    }
}
