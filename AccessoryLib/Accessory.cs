    using System;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
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

        private double renderWidth = 640;
        private double renderHeight = 480;
        private string imagePath;
        private AccessoryPositon accessoryPositon;
        private System.Windows.Controls.Image globalSystemWindowsControlsImage;


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

        public Accessory(KinectSensor _kinectSensor, string imgPath, AccessoryPositon accessoryPositon, System.Windows.Controls.Image globalSystemWindowsControlsImage)
        {
            // TODO: Complete member initialization
            this.sensor = _kinectSensor;
            this.imagePath = imgPath;
            this.renderHeight = globalSystemWindowsControlsImage.Height;
            this.renderWidth = globalSystemWindowsControlsImage.Width;
            this.accessoryPositon = accessoryPositon;
            this.globalSystemWindowsControlsImage = globalSystemWindowsControlsImage;

            this.sensor.SkeletonStream.Enable();
            this.sensor.SkeletonFrameReady += this.AccessoryFrameReady;

            this.drawingGroup = new DrawingGroup();
            this.ImageSource = new DrawingImage(this.drawingGroup);

            globalSystemWindowsControlsImage.Source = ImageSource;
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
                                                                                                    ColorImageFormat.RgbResolution640x480Fps30);

                        ImageSource image =
                            new BitmapImage(
                                new Uri(imagePath, UriKind.Absolute));

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
                        int imgHeight = (int) (150 - (50 * Sloc.Z));
                        int imgWidth = (int) (150 - (50 * Sloc.Z));
                        //var img = CreateResizedImage(image, imgWidth, imgHeight);
                        
                        Console.WriteLine("Z: {0}, imgW: {1} , imgH {2}", Sloc.Z, imgWidth, imgHeight);
                        
                        dc.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, renderWidth, renderHeight));
                        dc.DrawImage(image, new Rect(headX - 35, headY, imgWidth, imgHeight));
                        //Console.WriteLine("X: {0}, y {1}", headX, headY);
                        //this.drawingGroup.ClipGeometry =
                        //    new RectangleGeometry(new Rect(0.0, 0.0, this.renderWidth, this.renderHeight));
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new ImageSource with the specified width/height
        /// </summary>
        /// <param name="source">Source image to resize</param>
        /// <param name="width">Width of resized image</param>
        /// <param name="height">Height of resized image</param>
        /// <returns>Resized image</returns>
        ImageSource CreateResizedImage(ImageSource source, int width, int height)
        {
            // Target Rect for the resize operation
            Rect rect = new Rect(0, 0, width, height);

            // Create a DrawingVisual/Context to render with
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawImage(source, rect);
            }

            // Use RenderTargetBitmap to resize the original image
            RenderTargetBitmap resizedImage = new RenderTargetBitmap(
                (int)rect.Width, (int)rect.Height,  // Resized dimensions
                96, 96,                             // Default DPI values
                PixelFormats.Default);              // Default pixel format
            resizedImage.Render(drawingVisual);

            // Return the resized image
            return resizedImage;
        }
    }
}
