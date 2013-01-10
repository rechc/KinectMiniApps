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
        private int _dragDirection;
        private bool _waitForTextList;

        private bool _kinectFocused;

        private readonly KinectSensor _kinectSensor;
        private Skeleton[] _skeletons;
        private int _id = -1;

        public MainWindow()
        {
            InitializeComponent();

            _kinectSensor = KinectSensor.KinectSensors.FirstOrDefault(s => s.Status == KinectStatus.Connected);

            if (_kinectSensor == null)
            {
                ExceptionTextBlock.Text = "Kein Kinect-Sensor angeschloßen";
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
                _kinectSensor.ElevationAngle = 17;
            }


            MyLoopList.SetAutoDragOffset(0.55);
            MyLoopList.SetDuration(new Duration(new TimeSpan(3000000))); //300m
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

        private void OnAllReady(object sender, AllFramesReadyEventArgs e)
        {
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

        private void ProcessSkeleton(Skeleton skeleton)
        {
            if (skeleton == null)
            {
                if (_kinectFocused)
                {
                    myLoopList_MouseUp_1(null, null);
                    _kinectFocused = false;
                }
                return;
            }
            Joint handRight = skeleton.Joints[JointType.HandRight];
            if (handRight.TrackingState != JointTrackingState.Tracked)
            {
                if (_kinectFocused)
                {
                    myLoopList_MouseUp_1(null, null);
                    _kinectFocused = false;
                }
                return;
            }
            
            
            if (handRight.Position.Z < 1.5)
            {
                if (!_kinectFocused)
                {
                    myLoopList_MouseDown_1(null, null);
                    _kinectFocused = true;
                }
            }
            else
            {
                if (_kinectFocused)
                {
                    myLoopList_MouseUp_1(null, null);
                    _kinectFocused = false;
                }
            }
            ColorImagePoint cp = _kinectSensor.CoordinateMapper.MapSkeletonPointToColorPoint(handRight.Position, ColorImageFormat.RawBayerResolution640x480Fps30);

            Point currentPoint = new Point(cp.X*5, cp.Y*5);
            //if (currentPoint.X > 0 && currentPoint.Y > 0)
                Move(currentPoint);
            Debug.WriteLine("X: " + currentPoint.X + " Y: " + currentPoint.Y + " Z: " + handRight.Position.Z);
            //Debug.WriteLine("kinect focused: " + _kinectFocused);
            //Debug.WriteLine("_doDrag: " + _doDrag);
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
                if (_kinectFocused)
                {
                    myLoopList_MouseUp_1(null, null);
                    _kinectFocused = false;
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



        private void Move(Point currentPos)
        {
            if (!_doDrag) return;
            if (!_oldMovePoint.HasValue)
                _oldMovePoint = currentPos;
            if (Math.Abs(_oldMovePoint.Value.X - currentPos.X) < 0.000000001 && Math.Abs(_oldMovePoint.Value.Y - currentPos.Y) < 0.000000001)
            {
              //  return;
            }

            int xDistance = (int)(currentPos.X - _oldMovePoint.Value.X);
            int yDistance = (int)(currentPos.Y - _oldMovePoint.Value.Y);

            _dragDirection = Math.Abs(xDistance) >= Math.Abs(yDistance) ? 1 : 2;
            bool mayDragOn = false;
            if (_dragDirection == 1)
            {
                mayDragOn = MyLoopList.HDrag(xDistance);
            }
            if (_dragDirection == 2)
            {
                if (!_waitForTextList)
                    mayDragOn = MyLoopList.VDrag(yDistance);
            }
            if (!mayDragOn)
            {
                _doDrag = false;
            }
            _oldMovePoint = currentPos;
        }

        private void myLoopList_MouseMove_1(object sender, MouseEventArgs e)
        {
            Point point = e.GetPosition(MyLoopList);
            Debug.WriteLine("X: " + point.X + " Y: " + point.Y );
            Move(e.GetPosition(MyLoopList));
        }

        private void myLoopList_MouseUp_1(object sender, MouseButtonEventArgs e)
        {
            _doDrag = false;
            _oldMovePoint = null;
            MyLoopList.AnimBack();
            _dragDirection = 0;
        }

        private void myLoopList_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            _doDrag = true;
        }


        protected override void OnKeyDown(KeyEventArgs e)
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

        private void myLoopList_MouseLeave_1(object sender, MouseEventArgs e)
        {
            myLoopList_MouseUp_1(null, null);
        }

    }
}
