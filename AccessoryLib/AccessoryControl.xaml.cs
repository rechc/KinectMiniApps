using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AccessoryLib
{
    /// <summary>
    /// Interaction logic for AccessoryControl.xaml
    /// </summary>
    public partial class AccessoryControl : UserControl
    {
        private KinectSensor _sensor;
        private Skeleton[] _skeletons;

        public AccessoryControl()
        {
            InitializeComponent();
            AccessoryItems = new List<AccessoryItem>();
        }

        public KinectSensor Sensor
        {
            get { return _sensor; }
            set { _sensor = value; }
        }

        public Skeleton[] Skeletons
        {
            get { return _skeletons; }
            
            set
            {
                _skeletons = value;
                InvalidateVisual();
            }
        }

        public List<AccessoryItem> AccessoryItems { get; private set; }

        public void Start()
        {
            if (!Sensor.SkeletonStream.IsEnabled)
                Sensor.SkeletonStream.Enable();
            Sensor.SkeletonFrameReady += OnSkeletonFrameReady;
        }

        private void OnSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    var skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                    Skeletons = skeletons;
                }
            }

        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (Skeletons == null)
                return;

            drawingContext.PushClip(new RectangleGeometry(new Rect(0, 0, Width, Height)));

            //Skeleton person = Skeletons.First(p => p.TrackingState == SkeletonTrackingState.Tracked);
            foreach (Skeleton person in Skeletons)
            {
                if (person.TrackingState == SkeletonTrackingState.Tracked)
                {
                    RenderAccessories(drawingContext, person);
                }
            }
        }

        private void RenderAccessories(DrawingContext drawingContext, Skeleton person)
        {
            foreach (var item in AccessoryItems)
            {
                RenderAccessoryItem(drawingContext, person, item);
            }
        }

        private void RenderAccessoryItem(DrawingContext drawingContext, Skeleton person, AccessoryItem item)
        {
            SkeletonPoint sloc = person.Joints[JointType.Head].Position;
            ColorImagePoint cloc = _sensor.CoordinateMapper.MapSkeletonPointToColorPoint(
                sloc, _sensor.ColorStream.Format);

            int positionCorrection = 0;
            switch (item.Position)
            {
                case AccessoryPositon.Hat:
                    positionCorrection = -100;
                    break;

                case AccessoryPositon.Beard:
                    positionCorrection = 10;
                    break;
            }

            double headX = cloc.X;
            double headY = cloc.Y + positionCorrection;
            double imgHeight = 80;// 150 - (50 * sloc.Z);
            double imgWidth = 80;// 150 - (50 * sloc.Z);

            Console.WriteLine("Z: {0}, imgW: {1} , imgH {2}", sloc.Z, imgWidth, imgHeight);

            drawingContext.DrawImage(item.Image, new Rect(headX - 35, headY, imgWidth, imgHeight));
        }
    }
}
