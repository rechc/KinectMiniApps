//------------------------------------------------------------------------------
// <copyright file="SwipeRectangleMain.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using Microsoft.Kinect;
    using System.Diagnostics;
    using System.Linq;
    using System;
    using System.Windows.Media.Animation;
    using System.Windows.Shapes;
    using System.Windows.Controls;
using System.Timers;

    /// <summary>
    /// Interaction logic for SwipeRectangleMain.xaml
    /// </summary>
    public partial class SwipeRectangleMain : Window
    {
        /// <summary>
        /// Width of output drawing
        /// </summary>
        private const float RenderWidth = 640.0f;

        /// <summary>
        /// Height of our output drawing
        /// </summary>
        private const float RenderHeight = 480.0f;

        /// <summary>
        /// Thickness of drawn joint lines
        /// </summary>
        private const double JointThickness = 3;

        /// <summary>
        /// Thickness of body center ellipse
        /// </summary>
        private const double BodyCenterThickness = 10;

        /// <summary>
        /// Thickness of clip edge rectangles
        /// </summary>
        private const double ClipBoundsThickness = 10;

        /// <summary>
        /// Brush used to draw skeleton center point
        /// </summary>
        private readonly Brush centerPointBrush = Brushes.Blue;

        /// <summary>
        /// Brush used for drawing joints that are currently tracked
        /// </summary>
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));

        /// <summary>
        /// Brush used for drawing joints that are currently inferred
        /// </summary>        
        private readonly Brush inferredJointBrush = Brushes.Yellow;

        /// <summary>
        /// Pen used for drawing bones that are currently tracked
        /// </summary>
        private readonly Pen trackedBonePen = new Pen(Brushes.Green, 6);

        /// <summary>
        /// Pen used for drawing bones that are currently inferred
        /// </summary>        
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor sensor;

        /// <summary>
        /// Drawing group for skeleton rendering output
        /// </summary>
        private DrawingGroup drawingGroup;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSource;

        private SolidColorBrush blue = new SolidColorBrush(Colors.Blue);
        private SolidColorBrush red = new SolidColorBrush(Colors.Red);
        private Vector handDirection = new Vector(0, 0);
        
        private bool isInRect;

        private bool isInInnerRect = false;
        private bool wasInLastFrameInInnerRect = false;
        private bool isInOuterRect = false;
        private bool wasInLastFrameInOuterRect = false;

        Point rectMiddlePoint = new Point(0, 0);
        int colorId = 0;
        int colorBlockId = 0;
        Brush[,] colors = { { Brushes.LightGreen, Brushes.Green, Brushes.DarkGreen }, { Brushes.LightBlue, Brushes.Blue, Brushes.DarkBlue }, { Brushes.LightSalmon, Brushes.Salmon, Brushes.DarkSalmon } };
        Point handRightPoint = new Point(0, 0);
        Rect innerRect;
        Rect outerRect;

        /// <summary>
        /// Initializes a new instance of the SwipeRectangleMain class.
        /// </summary>
        public SwipeRectangleMain()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Draws indicators to show which edges are clipping skeleton data
        /// </summary>
        /// <param name="skeleton">skeleton to draw clipping information for</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private static void RenderClippedEdges(Skeleton skeleton, DrawingContext drawingContext)
        {
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, RenderHeight - ClipBoundsThickness, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, ClipBoundsThickness, RenderHeight));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(RenderWidth - ClipBoundsThickness, 0, ClipBoundsThickness, RenderHeight));
            }
        }

        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // Create the drawing group we'll use for drawing
            this.drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);

            // Display the drawing using our image control
            Image.Source = this.imageSource;

            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                // Turn on the skeleton stream to receive skeleton frames
                this.sensor.SkeletonStream.Enable();

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;

                // Start the sensor!
                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }

            if (null == this.sensor)
            {
                this.statusBarText.Text = Properties.Resources.NoKinectReady;
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.Stop();
            }
        }

        /// <summary>
        /// Event handler for Kinect sensor's SkeletonFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
            }

            using (DrawingContext dc = this.drawingGroup.Open())
            {
                // Draw a transparent background to set the render size
                dc.DrawRectangle(Brushes.White, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));

                if (skeletons.Length != 0)
                {
                    foreach (Skeleton skel in skeletons)
                    {
                        RenderClippedEdges(skel, dc);

                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            this.DrawBonesAndJoints(skel, dc);
                        }
                        else if (skel.TrackingState == SkeletonTrackingState.PositionOnly)
                        {
                            dc.DrawEllipse(
                            this.centerPointBrush,
                            null,
                            this.SkeletonPointToScreen(skel.Position),
                            BodyCenterThickness,
                            BodyCenterThickness);
                        }
                    }
                }

                GestureRecognition(skeletons);

                // prevent drawing outside of our render area
                this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
            }
        }

        private bool wasInInnerRect = false;

        // Rectangle Fade out variables
        private int rectFadeOutTimer = 2000; // Miliseconds   -> Time when Fade-out animation starts
        private long enterInnerRectTimestamp = 0;
        private bool _isAnimating = false;

        private void GestureRecognition(Skeleton[] skeletons)
        {
            if (skeletons.Count(t => t.TrackingState == SkeletonTrackingState.Tracked) >= 1)
            {
                Skeleton skel = skeletons.First(s => s.TrackingState == SkeletonTrackingState.Tracked);
                Joint handRight = skel.Joints[JointType.HandRight];
                Joint hipRight = skel.Joints[JointType.HipRight];
                Joint spine = skel.Joints[JointType.Spine];
                
                innerRect = GetInnerRect(skel);
                outerRect = GetOuterRect(innerRect);
                TransformRectangles();
                TransformHand();
                handRightPoint = SkeletonPointToScreen(handRight.Position);

                isInRect = innerRect.Contains(handRightPoint);

                isInInnerRect = innerRect.Contains(handRightPoint);
                isInOuterRect = outerRect.Contains(handRightPoint);

                // Inneres Rechteck wurde betreten oder verlassen
                if(isInInnerRect != wasInLastFrameInInnerRect)
                {
                    // Inneres Rechteck wurde betreten
                    if (isInInnerRect)
                    {
                        // Fade-out: Save timestamp when enter the inner rect
                        enterInnerRectTimestamp = getTimeStamp();
                        wasInInnerRect = true;
                    }
                    // Inneres Rechteck wurde verlassen
                    else
                    {
                        // Fade-out: Stop animation
                        //if ()
                        if (handRightPoint.X > innerRect.TopRight.X) //leave right
                        {
                            BackColor.Fill = NextColor();
                        }
                        else if (handRightPoint.X < innerRect.TopLeft.X) //leave left
                        {
                            BackColor.Fill = PrevColor();
                        }
                        else if (handRightPoint.Y > innerRect.BottomLeft.Y) //leave bottom
                        {
                            BackColor.Fill = NextColorBlock();
                        }
                        else if (handRightPoint.Y < innerRect.TopLeft.Y) //leave top
                        {
                            BackColor.Fill = PrevColorBlock();
                        }
                    }
                    wasInLastFrameInInnerRect = isInInnerRect;
                }

                // Aeusseres Rechteck wurde betreten oder verlassen
                if (isInOuterRect != wasInLastFrameInOuterRect)
                {
                    // Aeusseres Rechteck wurde betreten
                    if (isInOuterRect)
                    {
                    }
                    // Aeusseres Rechteck wurde verlassen
                    else
                    {
                        if (wasInInnerRect)
                        {
                            if (handRightPoint.X > outerRect.TopRight.X) //leave right
                            {
                                SwipeRight();
                            }
                            else if (handRightPoint.X < outerRect.TopLeft.X) //leave left
                            {
                                SwipeLeft();
                            }
                            else if (handRightPoint.Y > outerRect.BottomLeft.Y) //leave bottom
                            {
                                SwipeDown();
                            }
                            else if (handRightPoint.Y < outerRect.TopLeft.Y) //leave top
                            {
                                SwipeUp();
                            }
                            wasInInnerRect = false;
                            FrontColor.Fill = CurrentColor();
                            TransformFrontColor(0, 0);
                        }
                    }
                }
                wasInLastFrameInOuterRect = isInOuterRect;

                if (isInOuterRect && !isInInnerRect && wasInInnerRect)
                {
                    double swipeLeft = GetPercentageSwipeLeft(handRightPoint);
                    double swipeRight = GetPercentageSwipeRight(handRightPoint);
                    double swipeTop = GetPercentageSwipeTop(handRightPoint);
                    double swipeBottom = GetPercentageSwipeBottom(handRightPoint);
                    if(swipeLeft >= 0)
                    {
                        TransformFrontColor(-swipeLeft, 0);
                    }
                    if (swipeRight >= 0)
                    {
                        TransformFrontColor(swipeRight, 0);
                    }
                    if (swipeTop >= 0)
                    {
                        TransformFrontColor(0, -swipeTop);
                    }
                    if (swipeBottom >= 0)
                    {
                        TransformFrontColor(0, swipeBottom);
                    }
                }

                //Fade-out: If true, start fade animation
                if (getTimeStamp() - enterInnerRectTimestamp > rectFadeOutTimer && isInInnerRect)
                {
                    wasInInnerRect = false;
                }
            }
        }

        private void Animate(String name, DependencyProperty property, int from, int to)
        {
            if (!_isAnimating)
            {
                _isAnimating = true;
                DoubleAnimation doubleAnimation = new DoubleAnimation
                {
                    Duration = new Duration(TimeSpan.FromSeconds(2)),
                    To = to,
                    From = from
                };
                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(doubleAnimation);
                Storyboard.SetTargetName(doubleAnimation, name);
                Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(property));
                storyboard.Begin(this);

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

            // Rechteck verschieben
            int offsetX = 20;
            int offsetY = 0;

            // inneres Rechteck verkleinern
            double height = Math.Abs(spine.Y - y);

            double width = height;

            return new Rect(x  + offsetX, y + offsetY, width, height);
        }

        private Rect GetOuterRect(Rect innerRect)
        {
            double border = 30;
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
            AnimateHandPoint(handRightPoint, innerRect.TopLeft);
        }

        private void TransformHand()
        {
            TranslateTransform transform = new TranslateTransform();
            Hand.RenderTransform = transform;
            transform.X = handRightPoint.X - (Hand.Width / 2);
            transform.Y = handRightPoint.Y - (Hand.Height / 2);
        }

        private void TransformRectangles()
        {
            TranslateTransform transformInnerRect = (TranslateTransform)InnerRect.RenderTransform;
            transformInnerRect.X = innerRect.X;
            transformInnerRect.Y = innerRect.Y;
            InnerRect.Width = innerRect.Width;
            InnerRect.Height = innerRect.Height;

            TranslateTransform transformOuterRect = (TranslateTransform)OuterRect.RenderTransform;
            transformOuterRect.X = outerRect.X;
            transformOuterRect.Y = outerRect.Y;
            OuterRect.Width = outerRect.Width;
            OuterRect.Height = outerRect.Height;
        }

        private void TransformFrontColor(double left, double top)
        {
            TranslateTransform transform = new TranslateTransform();
            FrontColor.RenderTransform = transform;
            transform.X = (left * FrontColor.Width) / 2;
            transform.Y = (top * FrontColor.Height) / 2;
            
        }

        private void SwipeRight()
        {
            TransformFrontColor(1, 0);
        }

        private void SwipeLeft()
        {
            TransformFrontColor(-1, 0);
        }

        private double GetPercentageSwipeLeft(Point hand)
        {

            double rectDistance = innerRect.X - outerRect.X;
            double handDistance = innerRect.X - hand.X;
            return handDistance / rectDistance;
        }

        private double GetPercentageSwipeRight(Point hand)
        {

            double rectDistance = (outerRect.X + outerRect.Width) - (innerRect.X + innerRect.Width);
            double handDistance = hand.X - (innerRect.X + innerRect.Width);
            return handDistance / rectDistance;
        }

        private double GetPercentageSwipeTop(Point hand)
        {

            double rectDistance = innerRect.Y - outerRect.Y;
            double handDistance = innerRect.Y - hand.Y;
            return handDistance / rectDistance;
        }

        private double GetPercentageSwipeBottom(Point hand)
        {

            double rectDistance = (outerRect.Y + outerRect.Height) - (innerRect.Y + innerRect.Height);
            double handDistance = hand.Y - (innerRect.Y + innerRect.Height);
            return handDistance / rectDistance;
        }

        private void SwipeUp()
        {
            TransformFrontColor(0, -1);
        }

        private void SwipeDown()
        {
            TransformFrontColor(0, 1);
        }

        private Brush CurrentColor()
        {
            return colors[colorBlockId, colorId]; 
        }

        private Brush NextColor()
        {
            colorId++;
            if (colorId >= colors.GetLength(1))
            {
                colorId = 0;
            }
            return colors[colorBlockId, colorId];
        }

        private Brush PrevColor()
        {
            colorId--;
            if (colorId < 0)
            {
                colorId = colors.GetLength(1) - 1;
            }
            return colors[colorBlockId, colorId];
        }

        private Brush NextColorBlock()
        {
            colorBlockId++;
            if (colorBlockId >= colors.GetLength(0))
            {
                colorBlockId = 0;
            }
            return colors[colorBlockId, colorId];
        }

        private Brush PrevColorBlock()
        {
            colorBlockId--;
            if (colorBlockId < 0)
            {
                colorBlockId = colors.GetLength(0) - 1;
            }
            return colors[colorBlockId, colorId];
        }


        /// <summary>
        /// Draws a skeleton's bones and joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private void DrawBonesAndJoints(Skeleton skeleton, DrawingContext drawingContext)
        {
            // Render Torso
            this.DrawBone(skeleton, drawingContext, JointType.Head, JointType.ShoulderCenter);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderRight);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.Spine);
            this.DrawBone(skeleton, drawingContext, JointType.Spine, JointType.HipCenter);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipLeft);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipRight);

            // Left Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderLeft, JointType.ElbowLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowLeft, JointType.WristLeft);
            this.DrawBone(skeleton, drawingContext, JointType.WristLeft, JointType.HandLeft);

            // Right Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderRight, JointType.ElbowRight);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowRight, JointType.WristRight);
            this.DrawBone(skeleton, drawingContext, JointType.WristRight, JointType.HandRight);

            // Left Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipLeft, JointType.KneeLeft);
            this.DrawBone(skeleton, drawingContext, JointType.KneeLeft, JointType.AnkleLeft);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleLeft, JointType.FootLeft);

            // Right Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipRight, JointType.KneeRight);
            this.DrawBone(skeleton, drawingContext, JointType.KneeRight, JointType.AnkleRight);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleRight, JointType.FootRight);
 
            // Render Joints
            foreach (Joint joint in skeleton.Joints)
            {
                Brush drawBrush = null;

                if (joint.TrackingState == JointTrackingState.Tracked)
                {
                    drawBrush = this.trackedJointBrush;                    
                }
                else if (joint.TrackingState == JointTrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush;                    
                }

                if (drawBrush != null)
                {
                    drawingContext.DrawEllipse(drawBrush, null, this.SkeletonPointToScreen(joint.Position), JointThickness, JointThickness);
                }
            }
        }

        /// <summary>
        /// Maps a SkeletonPoint to lie within our render space and converts to Point
        /// </summary>
        /// <param name="skelpoint">point to map</param>
        /// <returns>mapped point</returns>
        private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }

        /// <summary>
        /// Draws a bone line between two joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw bones from</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// <param name="jointType0">joint to start drawing from</param>
        /// <param name="jointType1">joint to end drawing at</param>
        private void DrawBone(Skeleton skeleton, DrawingContext drawingContext, JointType jointType0, JointType jointType1)
        {
            Joint joint0 = skeleton.Joints[jointType0];
            Joint joint1 = skeleton.Joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == JointTrackingState.NotTracked ||
                joint1.TrackingState == JointTrackingState.NotTracked)
            {
                return;
            }

            // Don't draw if both points are inferred
            if (joint0.TrackingState == JointTrackingState.Inferred &&
                joint1.TrackingState == JointTrackingState.Inferred)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this.inferredBonePen;
            if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked)
            {
                drawPen = this.trackedBonePen;
            }

            drawingContext.DrawLine(drawPen, this.SkeletonPointToScreen(joint0.Position), this.SkeletonPointToScreen(joint1.Position));
        }

        /// <summary>
        /// Handles the checking or unchecking of the seated mode combo box
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void CheckBoxSeatedModeChanged(object sender, RoutedEventArgs e)
        {
            if (null != this.sensor)
            {
                if (this.checkBoxSeatedMode.IsChecked.GetValueOrDefault())
                {
                    this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                }
                else
                {
                    this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
                }
            }
        }
    }
}