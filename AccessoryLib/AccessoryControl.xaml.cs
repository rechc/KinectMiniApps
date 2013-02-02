using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AccessoryLib
{
    public partial class AccessoryControl : UserControl
    {
        private KinectSensor _sensor;
        private Skeleton[] _skeletons;

        // Liste von Gegenstaenden, die gezeichnet werden sollen.
        public List<AccessoryItem> AccessoryItems { get; private set; }

        public AccessoryControl()
        {
            InitializeComponent();
            AccessoryItems = new List<AccessoryItem>();
        }

        public void SetSkeletons(Skeleton[] skeletons)
        {
            _skeletons = skeletons;
            InvalidateVisual();
        }

        public void Start(KinectSensor sensor)
        {
            _sensor = sensor;
        }

        // Control neu zeichnen.
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (_skeletons == null)
                return;

            // Nicht ueber den Rand des Controls hinaus zeichnen.
            //drawingContext.PushClip(new RectangleGeometry(new Rect(0, 0, ActualWidth, ActualHeight)));

            // Items fuer alle Personen zeichnen.
            foreach (Skeleton person in _skeletons)
            {
                if (person.TrackingState == SkeletonTrackingState.Tracked)
                    RenderAccessories(drawingContext, person);
            }
        }

        // Zeichnet alle Items fuer eine einzelne Person.
        private void RenderAccessories(DrawingContext drawingContext, Skeleton person)
        {
            foreach (var item in AccessoryItems)
            {
                RenderAccessoryItem(drawingContext, person, item);
            }
        }

        // Zeichnet ein Item.
        private void RenderAccessoryItem(DrawingContext drawingContext, Skeleton person, AccessoryItem item)
        {
            SkeletonPoint headPos = person.Joints[JointType.Head].Position;
            DepthImagePoint depthImagePoint = _sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(headPos,
                                                                                                    _sensor.DepthStream
                                                                                                           .Format);
            double g = item.Width; // Objektgroesse in m.
            double r = headPos.Z;  // Entfernung in m.
            double imgWidth = 2 * Math.Atan(g / (2 * r)) * ActualWidth;
            double aspectRatio = item.Image.Width / item.Image.Height;
            double imgHeight = imgWidth / aspectRatio;

            double offsetX = 0, offsetY = 0;
            switch (item.Position)
            {
                case AccessoryPositon.Hat:
                    offsetY = -imgHeight;
                    break;
                case AccessoryPositon.Beard:
                    offsetY = imgHeight/4;
                    break;
            }

            double headX = depthImagePoint.X * (ActualWidth / _sensor.DepthStream.FrameWidth) + offsetX;
            double headY = depthImagePoint.Y * (ActualHeight / _sensor.DepthStream.FrameHeight) + offsetY;

            //Console.WriteLine("Z: {0}, imgW: {1}, imgH: {2}, X: {3}, Y: {4}", headPos.Z, imgWidth, imgHeight, cloc.X, cloc.Y);
            drawingContext.DrawImage(item.Image, new Rect(headX - imgWidth / 2, headY, imgWidth, imgHeight));
        }
    }
}
