using System.Collections.Generic;
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
        private bool _kinectFocused;

        private readonly List<int> _savedDirections = new List<int>();
        private bool _dragDirectionIsObvious;

        private readonly KinectSensor _kinectSensor;
        private Skeleton[] _skeletons;
        private int _id = -1;

        public MainWindow()
        {
            InitializeComponent();
            try
            {
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


                MyLoopList.SetAutoDragOffset(0.20);
                MyLoopList.SetDuration(new Duration(new TimeSpan(30000000))); //300m
                MyLoopList.Scrolled += MyLoopListOnScrolled;
                MyTextLoopList.Scrolled += MyTextLoopList_Scrolled;
                MyTextLoopList.SetFontSize(36);
                MyTextLoopList.SetFontFamily("Miriam Fixed");
                MyTextLoopList.SetDuration(new Duration(new TimeSpan(2500000)));
                string[] paths = Directory.GetFiles(Environment.CurrentDirectory + @"\images", "tele*");
                Node anchor = null;
                Node anchorForMokup = null;

                for (int i = 0; i < paths.Count(); i++)
                {
                    string path = paths[i];
                    Grid grid = new Grid();
                    Image img = new Image
                        {
                            Stretch = Stretch.Fill,
                            Source = LoadImage(path)
                        };
                    grid.Children.Add(img);
                    if (i != 3)
                    {
                        anchor = MyLoopList.AddToBelow(anchor, grid);
                        if (i == 1)
                        {
                            anchorForMokup = anchor;
                        }
                    }
                    else
                        anchor = MyLoopList.AddToLeft(anchor, grid);
                }
                Grid mokupGrid = new Grid();

                Image mokuImg = new Image
                    {
                        Stretch = Stretch.Fill,
                        Source = LoadImage(Environment.CurrentDirectory + @"\images\mokup.jpg")
                    };

                mokupGrid.Children.Add(mokuImg);

                MyLoopList.AddToRight(anchorForMokup, mokupGrid);

                MyTextLoopList.Add("Ebene2");
                MyTextLoopList.Add("Ebene1");
            }
            catch (Exception exc)
            {
                ExceptionTextBlock.Text = exc.Message + "\n\r" + exc.InnerException;
            }
        }

        private void OnAllReady(object sender, AllFramesReadyEventArgs e)
        {
            try
            {
                using (SkeletonFrame sFrame = e.OpenSkeletonFrame())
                {
                    if (sFrame == null) return;
                    if (_skeletons == null)
                    {
                        _skeletons = new Skeleton[_kinectSensor.SkeletonStream.FrameSkeletonArrayLength];
                    }
                    sFrame.CopySkeletonDataTo(_skeletons);
                    //CorrectRoomCoords();
                    Skeleton skeleton = GetFixedSkeleton();
                    ProcessSkeleton(skeleton);
                }
            }
            catch (Exception exc)
            {
                ExceptionTextBlock.Text = exc.Message + "\n\r" + exc.InnerException;
            }
        }

        private void CorrectRoomCoords()
        {
            if (!_kinectSensor.IsRunning) return;
            int elevationAngle = _kinectSensor.ElevationAngle;
            if (elevationAngle == 0) return;
            elevationAngle *= -1;
            foreach (Skeleton s in _skeletons)
            {
                if (s.TrackingState != SkeletonTrackingState.Tracked) continue;
                foreach (JointType jt in Enum.GetValues(typeof(JointType)))
                {
                    var joint = s.Joints[jt];
                    var position = joint.Position;
                    position.Y = (float)(position.Y * Math.Cos(elevationAngle * Math.PI / 180.0) - position.Z * Math.Sin(elevationAngle * Math.PI / 180.0));
                    position.Z = (float)(position.Y * Math.Sin(elevationAngle * Math.PI / 180.0) + position.Z * Math.Cos(elevationAngle * Math.PI / 180.0));
                    joint.Position = position;
                    s.Joints[jt] = joint;
                }
            }
        }

        private void ProcessSkeleton(Skeleton skeleton)
        {
            if (skeleton == null)
            {
                _kinectFocused = false;
                return;
            }
            Joint handRight = skeleton.Joints[JointType.HandRight];
            Joint shoulderCenter = skeleton.Joints[JointType.ShoulderCenter];
            if (handRight.TrackingState != JointTrackingState.Tracked || shoulderCenter.TrackingState != JointTrackingState.Tracked)
            {
                _kinectFocused = false;
                return;
            }
            
            if (Math.Abs(shoulderCenter.Position.Z - handRight.Position.Z) > 0.4)
            {
                if (!_kinectFocused)
                {
                    myLoopList_MouseDown_1(null, null);
                    _kinectFocused = true;
                    KinectFocusedRectangle.Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (_kinectFocused)
                {
                    
                    myLoopList_MouseUp_1(null, null);
                    _kinectFocused = false;
                    KinectFocusedRectangle.Visibility = Visibility.Collapsed;
                }
            }
            ColorImagePoint cp = _kinectSensor.CoordinateMapper.MapSkeletonPointToColorPoint(handRight.Position, ColorImageFormat.RawBayerResolution640x480Fps30);

            Point currentPoint = new Point(cp.X*8, cp.Y*8);
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
                    if (_savedDirections.Count < 20)
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
