using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RectNavigation
{

    public class SwipeArgs : EventArgs
    {
        public double Progress { get; set; }
    }
    /// <summary>
    /// Interaktionslogik für UserControl1.xaml
    /// </summary>
    public partial class RectNavigationControl : UserControl
    {
        public RectNavigationControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor _sensor;

        /// <summary>
        /// Drawing group for skeleton rendering output
        /// </summary>


        private bool _isInInnerRect;
        private bool _wasInLastFrameInInnerRect;
        private bool _isInOuterRect;
        private bool _wasInLastFrameInOuterRect;

        private double enlargeTopWidth;
        private double enlargeTopHeight;

        private double enlargeBottomWidth;
        private double enlargeBottomHeight;


        private double translateTop;

        private double translateBottom;

        private bool _veryFirstTime = true;

        private Point _handRightPoint = new Point(0, 0);
        private Rect _innerRect;
        private Rect _outerRect;

        private bool _wasInInnerRect;

        // Rectangle Fade out variables
        private const int RectFadeOutTimer = 3000; // Miliseconds   -> Time when Fade-out animation starts
        private long _enterInnerRectTimestamp;

        private int DownSwipeBlockTimer = 1000;
        private long _upSwipeTimestamp;

        public event EventHandler SwipeLeftEvent;
        public event EventHandler SwipeRightEvent;
        public event EventHandler SwipeUpEvent;
        public event EventHandler SwipeDownEvent;
        public event EventHandler NoSwipe;


        private void FireNoSwipe()
        {
            if (NoSwipe != null)
            {
                NoSwipe(this, EventArgs.Empty);
                Debug.WriteLine("no swipe");
            }
        }

        private void FireSwipeLeft(double progress)
        {
            if (SwipeLeftEvent != null)
            {
                SwipeArgs e = new SwipeArgs { Progress = progress };
                SwipeLeftEvent(this, e);
            }
        }

        private void FireSwipeRight(double progress)
        {
            if (SwipeLeftEvent != null)
            {
                SwipeArgs e = new SwipeArgs { Progress = progress };
                SwipeRightEvent(this, e);
            }
        }

        private void FireSwipeUp(double progress)
        {
            if (SwipeLeftEvent != null)
            {
                _upSwipeTimestamp = getTimeStamp();
                enlargeTopWidth = (OuterRect.ActualWidth/5) * progress;
                enlargeTopHeight = (OuterRect.ActualHeight/5) * progress;
                translateTop = (OuterRect.ActualHeight / 30) * progress;
                SwipeArgs e = new SwipeArgs { Progress = progress };
                SwipeUpEvent(this, e);
            }
        }

        private void FireSwipeDown(double progress)
        {
            if (SwipeLeftEvent != null)
            {
                if (getTimeStamp() - _upSwipeTimestamp < DownSwipeBlockTimer)
                    return;
                enlargeBottomWidth = (OuterRect.ActualWidth/5) * progress;
                enlargeBottomHeight = (OuterRect.ActualHeight/5) * progress;
                translateBottom = (OuterRect.ActualHeight / 20) * progress;
                SwipeArgs e = new SwipeArgs { Progress = progress };
                SwipeDownEvent(this, e);
            }
        }


        public void SetTopText(string text)
        {
            TopText.Text = text;
        }

        public void SetBottomText(string text)
        {
            BottomText.Text = text;
        }

        public void Start(KinectSensor sensor)
        {
            _sensor = sensor;
            DrawingCanvas.Width = sensor.ColorStream.FrameWidth;
            DrawingCanvas.Height = sensor.ColorStream.FrameHeight;
        }

        public void GestureRecognition(Skeleton skel)
        {
            Joint handRight = skel.Joints[JointType.HandRight];

            if (handRight.TrackingState != JointTrackingState.Tracked) { //falls keine hand erkannt, breche ab
                return;
            }



            _innerRect = GetInnerRect(skel);
            _outerRect = GetOuterRect(_innerRect);
            TransformRectangles();
            TransformHand();
            _handRightPoint = SkeletonPointToScreen(handRight.Position);

            _isInInnerRect = _innerRect.Contains(_handRightPoint);
            _isInOuterRect = _outerRect.Contains(_handRightPoint);


            if (!_veryFirstTime)
            {
                if (!_isInOuterRect)
                {
                    Animate.Opacity(this, Opacity, 0, 0.2);
                }
            }
            // Inneres Rechteck wurde betreten oder verlassen
            if (_isInInnerRect != _wasInLastFrameInInnerRect)
            {
                // Inneres Rechteck wurde betreten
                if (_isInInnerRect)
                {
                    // Fade-out: Save timestamp when enter the inner rect
                    Animate.Opacity(this, Opacity, 1, 0.2);
                    _enterInnerRectTimestamp = getTimeStamp();
                    _wasInInnerRect = true;
                    FireNoSwipe();
                    enlargeTopHeight = 0;
                    enlargeTopWidth = 0;
                    enlargeBottomHeight = 0;
                    enlargeBottomWidth = 0;
                    translateBottom = 0;
                    translateTop = 0;
                }

                _wasInLastFrameInInnerRect = _isInInnerRect;
            }

            // Aeusseres Rechteck wurde betreten oder verlassen
            if (_isInOuterRect != _wasInLastFrameInOuterRect)
            {
                // Aeusseres Rechteck wurde betreten
                if (_isInOuterRect)
                {
                    _veryFirstTime = false;
                    enlargeTopHeight = 0;
                    enlargeTopWidth = 0;
                    enlargeBottomHeight = 0;
                    enlargeBottomWidth = 0;
                    translateBottom = 0;
                    translateTop = 0;
                }
                // Aeusseres Rechteck wurde verlassen
                else
                {
                    if (_wasInInnerRect)
                    {
                        if (_handRightPoint.X > _outerRect.TopRight.X) //leave right
                        {
                            FireSwipeRight(1);
                        }
                        else if (_handRightPoint.X < _outerRect.TopLeft.X) //leave left
                        {
                            FireSwipeLeft(1);
                        }
                        else if (_handRightPoint.Y > _outerRect.BottomLeft.Y) //leave bottom
                        {
                            FireSwipeDown(1);
                        }
                        else if (_handRightPoint.Y < _outerRect.TopLeft.Y) //leave top
                        {
                            FireSwipeUp(1);
                        }
                        FireNoSwipe();
                        _wasInInnerRect = false;
                    }
                }
                _wasInLastFrameInOuterRect = _isInOuterRect;
            }

            if (_isInOuterRect && !_isInInnerRect && _wasInInnerRect)
            {
                double swipeLeft = GetPercentageSwipeLeft(_handRightPoint);
                double swipeRight = GetPercentageSwipeRight(_handRightPoint);
                double swipeUp = GetPercentageSwipeTop(_handRightPoint);
                double swipeDown = GetPercentageSwipeBottom(_handRightPoint);
                if (swipeLeft >= 0)
                {
                    FireSwipeLeft(swipeLeft);
                }
                if (swipeRight >= 0)
                {
                    FireSwipeRight(swipeRight);
                }
                if (swipeUp >= 0)
                {
                    FireSwipeUp(swipeUp);
                }
                if (swipeDown >= 0)
                {
                    FireSwipeDown(swipeDown);
                }
            }

            //Fade-out: If true, start fade animation
            if (getTimeStamp() - _enterInnerRectTimestamp > RectFadeOutTimer && _isInInnerRect)
            {
                _wasInInnerRect = false;
                Animate.Opacity(this, Opacity, 0, 0.2);
            }
        }


        private long getTimeStamp()
        {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

        private Rect GetInnerRect(Skeleton skeleton)
        {
            Point spine = SkeletonPointToScreen(skeleton.Joints[JointType.Spine].Position);
            Point hipRight = SkeletonPointToScreen(skeleton.Joints[JointType.HipRight].Position);
            Point shoulderCenter = SkeletonPointToScreen(skeleton.Joints[JointType.ShoulderCenter].Position);
            double x = hipRight.X;
            double y = shoulderCenter.Y;

            // inneres Rechteck verkleinern
            double width = Math.Abs((spine.Y - y) * 0.9);
            double height = width * 0.8;


            // Rechteck verschieben
            const int offsetX = 15; // -5
            int offsetY = (int)(width / 2);
            return new Rect(x + offsetX, y + offsetY, width, height);
        }

        private Rect GetOuterRect(Rect innerRect)
        {
            const double border = 40;
            double x = innerRect.X - border;
            double y = innerRect.Y - border;
            double width = innerRect.Width + border * 2;
            double height = innerRect.Height + border * 2;
            return new Rect(x, y, width, height);
        }

        private void AnimateHandPoint(Point from, Point to)
        {
            Duration duration = new Duration(TimeSpan.FromSeconds(1));
            DoubleAnimation animationX = new DoubleAnimation(from.X, to.X + 75, duration);
            DoubleAnimation animationY = new DoubleAnimation(from.Y, to.Y, duration);
            animationX.Completed += AnimationCompleted;
            TranslateTransform trans = new TranslateTransform();
            Hand.RenderTransform = trans;
            trans.BeginAnimation(TranslateTransform.XProperty, animationX);
            trans.BeginAnimation(TranslateTransform.YProperty, animationY);
        }

        private void AnimationCompleted(object sender, EventArgs e)
        {
            AnimateHandPoint(_handRightPoint, _innerRect.TopLeft);
        }

        private void TransformHand()
        {
            TranslateTransform transform = (TranslateTransform)Hand.RenderTransform;
            Hand.RenderTransform = transform;
            transform.X = _handRightPoint.X - (Hand.Width / 2);
            transform.Y = _handRightPoint.Y - (Hand.Height / 2);
        }

        private void TransformRectangles()
        {
            TranslateTransform transformInnerRect = (TranslateTransform)InnerRect.RenderTransform;
            transformInnerRect.X = _innerRect.X;
            transformInnerRect.Y = _innerRect.Y;
            InnerRect.Width = _innerRect.Width;
            InnerRect.Height = _innerRect.Height;

            TranslateTransform transformOuterRect = (TranslateTransform)OuterRect.RenderTransform;
            transformOuterRect.X = _outerRect.X;
            transformOuterRect.Y = _outerRect.Y;
            OuterRect.Width = _outerRect.Width;
            OuterRect.Height = _outerRect.Height;

            TranslateTransform transformTopTextBlockViewBox = (TranslateTransform)TopTextViewBox.RenderTransform;
            transformTopTextBlockViewBox.X = ((TranslateTransform)OuterRect.RenderTransform).X - enlargeTopWidth/2;
            transformTopTextBlockViewBox.Y = ((TranslateTransform)OuterRect.RenderTransform).Y + translateTop;

            TranslateTransform transformBottomTextBlockViewBox = (TranslateTransform)BottomTextViewBox.RenderTransform;
            transformBottomTextBlockViewBox.X = ((TranslateTransform)OuterRect.RenderTransform).X - enlargeBottomWidth/2;
            transformBottomTextBlockViewBox.Y = ((TranslateTransform)OuterRect.RenderTransform).Y + OuterRect.ActualHeight - BottomTextViewBox.ActualHeight - translateBottom;

            TranslateTransform transformArrowRightViewBox = (TranslateTransform)ArrowRightViewBox.RenderTransform;
            transformArrowRightViewBox.X = ((TranslateTransform)OuterRect.RenderTransform).X + OuterRect.ActualWidth - ArrowLeftViewBox.ActualWidth;
            transformArrowRightViewBox.Y = ((TranslateTransform)OuterRect.RenderTransform).Y + OuterRect.ActualHeight / 2 - ArrowRightViewBox.ActualHeight / 2;
            
            TranslateTransform transformArrowLeftViewBox = (TranslateTransform)ArrowLeftViewBox.RenderTransform;
            transformArrowLeftViewBox.X = ((TranslateTransform)OuterRect.RenderTransform).X;
            transformArrowLeftViewBox.Y = ((TranslateTransform)OuterRect.RenderTransform).Y + OuterRect.ActualHeight / 2 - ArrowLeftViewBox.ActualHeight / 2;

            
            TopTextViewBox.Width = OuterRect.ActualWidth + enlargeTopWidth;
            TopTextViewBox.Height = transformInnerRect.Y - transformOuterRect.Y + enlargeTopHeight;

            BottomTextViewBox.Width = OuterRect.ActualWidth + enlargeBottomWidth;
            BottomTextViewBox.Height = transformInnerRect.Y - transformOuterRect.Y + enlargeBottomHeight;

            ArrowRightViewBox.Width = transformInnerRect.X - transformOuterRect.X;
            ArrowLeftViewBox.Width = transformInnerRect.X - transformOuterRect.X;
        }


        private double GetPercentageSwipeLeft(Point hand)
        {

            double rectDistance = _innerRect.X - _outerRect.X;
            double handDistance = _innerRect.X - hand.X;
            return handDistance / rectDistance;
        }

        private double GetPercentageSwipeRight(Point hand)
        {

            double rectDistance = (_outerRect.X + _outerRect.Width) - (_innerRect.X + _innerRect.Width);
            double handDistance = hand.X - (_innerRect.X + _innerRect.Width);
            return handDistance / rectDistance;
        }

        private double GetPercentageSwipeTop(Point hand)
        {

            double rectDistance = _innerRect.Y - _outerRect.Y;
            double handDistance = _innerRect.Y - hand.Y;
            return handDistance / rectDistance;
        }

        private double GetPercentageSwipeBottom(Point hand)
        {

            double rectDistance = (_outerRect.Y + _outerRect.Height) - (_innerRect.Y + _innerRect.Height);
            double handDistance = hand.Y - (_innerRect.Y + _innerRect.Height);
            return handDistance / rectDistance;
        }

        /// <summary>
        /// Maps a SkeletonPoint to lie within our render space and converts to Point
        /// </summary>
        /// <param name="skelpoint">point to map</param>
        /// <returns>mapped point</returns>
        private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            ColorImagePoint colorPoint = _sensor.CoordinateMapper.MapSkeletonPointToColorPoint(skelpoint, _sensor.ColorStream.Format);
            Point p = new Point(colorPoint.X, colorPoint.Y);
            if (Math.Abs(p.X) > 1000000 || Math.Abs(p.Y) > 1000000)
            {
                p.X = 0;
                p.Y = 0;
            }
            return p;
            
        }
    }
}
