    using System;
    using System.IO;
    using System.Windows;
    using Microsoft.Kinect;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Linq;

namespace AccessoryLib
{
    public enum AccessoryPositon
    {
        Hat,
        Sunclasses,
        Beard
    }

    public class Accessory
    {
        private DrawingGroup drawingGroup;
        private KinectSensor sensor;
        public DrawingImage ImageSource { get; private set; }
        private Brush Color { get; set; }

        private double renderWidth;
        private double renderHeight;
        private string imagePath;
        private AccessoryPositon accessoryPositon;


        public Accessory(KinectSensor sensor, string imagePath, AccessoryPositon accessoryPositon ,double renderWidth, double renderHeight)
        {
            this.sensor = sensor;
            this.renderHeight = renderHeight;
            this.renderWidth = renderWidth;
            this.imagePath = imagePath;
            this.accessoryPositon = accessoryPositon;
            
            // Add an event handler to be called whenever there is new color frame data
            this.sensor.SkeletonStream.Enable();
            this.sensor.SkeletonFrameReady += this.AccessoryFrameReady;

            this.drawingGroup = new DrawingGroup();
            this.ImageSource = new DrawingImage(this.drawingGroup);
            //this.DrawImage.Source = this.ImageSource;
        }


        private void AccessoryFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }

                using (DrawingContext dc = this.drawingGroup.Open())
                {

                    if (skeletons.Count(t => t.TrackingState == SkeletonTrackingState.Tracked) >= 1)
                    {
                        var person = skeletons.First(p => p.TrackingState == SkeletonTrackingState.Tracked);
                        SkeletonPoint Sloc = person.Joints[JointType.Head].Position;
                        ColorImagePoint Cloc = sensor.CoordinateMapper.MapSkeletonPointToColorPoint(Sloc,
                                                                                                    ColorImageFormat.
                                                                                                        RgbResolution640x480Fps30);

                        ImageSource image =
                            new BitmapImage(
                                new Uri(imagePath,
                                        UriKind.Absolute));

                        int positionCorection = 0;
                        switch (accessoryPositon)
                        {
                                case AccessoryPositon.Hat:
                                    positionCorection = -100;
                                    break;
                                case AccessoryPositon.Beard:
                                positionCorection = + 10;
                                    break;
                        }

                        double headX = Cloc.X;
                        double headY = Cloc.Y + positionCorection;
                        dc.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, this.renderWidth, this.renderHeight));
                        dc.DrawImage(image, new Rect(headX - 35, headY, 80, 80));
                        //Console.WriteLine("X: {0}, y {1}", headX, headY);
                        this.drawingGroup.ClipGeometry =
                            new RectangleGeometry(new Rect(0.0, 0.0, this.renderWidth, this.renderHeight));
                    }
                }
            }
        }
    }
}
