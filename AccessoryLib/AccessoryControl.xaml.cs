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

            // Zeichnet das Control automatisch neu.
            set
            {
                _skeletons = value;
                InvalidateVisual();
            }
        }

        // Liste von Gegenstaenden, die gezeichnet werden sollen.
        public List<AccessoryItem> AccessoryItems { get; private set; }

        public void Start()
        {
            if (!Sensor.SkeletonStream.IsEnabled)
                Sensor.SkeletonStream.Enable();
            Sensor.SkeletonFrameReady += OnSkeletonFrameReady;
        }

        // Event Handler liest alle Skeletons aus dem Skeleton Frame aus.
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

        // Control neu zeichnen.
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (Skeletons == null)
                return;

            // Nicht ueber den Rand des Controls hinaus zeichnen.
            drawingContext.PushClip(new RectangleGeometry(new Rect(0, 0, Width, Height)));

            // Items fuer alle Personen zeichnen.
            foreach (Skeleton person in Skeletons)
            {
                if (person.TrackingState == SkeletonTrackingState.Tracked)
                {
                    RenderAccessories(drawingContext, person);
                }
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
            ColorImagePoint cloc = _sensor.CoordinateMapper.MapSkeletonPointToColorPoint(
                headPos, _sensor.ColorStream.Format);

            // 120 px bei 1m Entfernung.
            const double g = 0.15; // Objektgroesse: 15 cm.
            const double px = 120; // Objektgroesse: 120 px bei 1 m Abstand.
            double r = headPos.Z;  // Entfernung in m.
            double imgHeight = 2 * Math.Atan(g / (2 * r)) * (px / g);
            double imgWidth = imgHeight;

            double offsetX = 0, offsetY = 0;
            switch (item.Position)
            {
                case AccessoryPositon.Hat:
                    offsetX = 0.11 * imgWidth;
                    offsetY = -imgHeight;
                    break;

                case AccessoryPositon.Beard:
                    offsetY = imgHeight/4;
                    break;
            }

            double headX = cloc.X + offsetX;
            double headY = cloc.Y + offsetY;

            //Console.WriteLine("Z: {0}, imgW: {1} , imgH {2}", headPos.Z, imgWidth, imgHeight);

            drawingContext.DrawImage(item.Image, new Rect(headX - imgWidth/2, headY, imgWidth, imgHeight));
        }
    }
}
