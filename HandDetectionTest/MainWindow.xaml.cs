using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HandDetection;
using Microsoft.Kinect;

namespace HandDetectionTest
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public SkeletonPoint RightHand { get; set; }

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor sensor;

        private bool handtracked;

        /// <summary>
        /// Bitmap that will hold color information
        /// </summary>
        private WriteableBitmap colorBitmap;

        /// <summary>
        /// Intermediate storage for the color data received from the camera
        /// </summary>
        private byte[] colorPixels;
       // private DepthImagePixel[] depthPixels;

        private HandDetection.HandDetect handDection;

        private Bitmap bmap;

        private Skeleton[] skeletons = new Skeleton[0];

        //init gui
        public MainWindow()
        {
            InitializeComponent();
        }

        //init sensors
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            handDection = new HandDetect();

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

                this.sensor.SkeletonStream.Enable();
                this.sensor.SkeletonFrameReady += SensorDetectHandReady;

                this.sensor.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
                this.sensor.DepthFrameReady += SensorDepthFrameReady;
              //  this.depthPixels = new DepthImagePixel[this.sensor.DepthStream.FramePixelDataLength];

                // Turn on the color stream to receive color frames
                this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);


                // Allocate space to put the pixels we'll receive
                this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];

                // This is the bitmap we'll display on-screen
                this.colorBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                // Set the image we display to point to the bitmap where we'll put the image data
                this.ColorImage.Source = this.colorBitmap;

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.ColorFrameReady += this.SensorColorFrameReady;


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
                Console.WriteLine("No Kinect ready");
            }
        }

        //stop sensors
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.Stop();
            }
        }


        /// <summary>
        /// Event handler for Kinect sensor's ColorFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    // Copy the pixel data from the image to a temporary array
                    colorFrame.CopyPixelDataTo(this.colorPixels);

                    // Write the pixel data into our bitmap
                    this.colorBitmap.WritePixels(
                        new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                        this.colorPixels,
                        this.colorBitmap.PixelWidth * sizeof(int),
                        0);
                    bmap = ImageToBitmap(colorFrame);
                    ColorImage.Source = DrawEllipses(colorFrame);
                }
            }
        }



        private void SensorDetectHandReady(object sender, SkeletonFrameReadyEventArgs e)
        {
             

             using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
             {
                 if (skeletonFrame != null)
                 {
                     skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                     skeletonFrame.CopySkeletonDataTo(skeletons);
                 }

                 if (skeletons.Count(t => t.TrackingState == SkeletonTrackingState.Tracked) >= 1)
                 {
                     var person = skeletons.First(p => p.TrackingState == SkeletonTrackingState.Tracked);
                     RightHand = person.Joints[JointType.HandRight].Position;

                     handtracked = true;
                 }
                 else 
                 {
                     handtracked = false;
                 }

             }
        }

        // cut hand image  get bool
        private void SensorDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            try
            {
                using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
                {
                    if (depthFrame != null) 
                    {
                        int intRightX = (int) (RightHand.X * depthFrame.Width);
                        int intRightY = -1* (int) (RightHand.Y * depthFrame.Height);

                        DepthImagePoint handPos = new DepthImagePoint();
                        try
                        {
                            handPos = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(RightHand,
                                                                                           DepthImageFormat.
                                                                                               Resolution320x240Fps30);

                        //depthFrame.CopyDepthImagePixelDataTo(this.depthPixels);

                        //depthBitmap = new WriteableBitmap(sensor.DepthStream.FrameWidth, sensor.DepthStream.FrameHeight,
                                                                            //96.0, 96.0, PixelFormats.Bgr32, null);
                        this.DepthImage.Source = DepthToBitmapSource(depthFrame);
                        if (handtracked)
                        {
                            Console.WriteLine("HandX: {0} , HandY: {1}; calcX {2}, calcY{3} ; handPosX: {4} , handPosY {5}"
                                       , RightHand.X, RightHand.Y, intRightX, intRightY, handPos.X, handPos.Y);

                            if ((handPos.X-25 + 60) > 320 || handPos.X-25 <= 0) return;
                            if ((handPos.Y-25 + 60) > 240 || handPos.Y-25 <= 0) return;

                            ImageSource imgRightHandSource =
                                new CroppedBitmap((BitmapSource)DepthImage.Source.CloneCurrentValue(), new Int32Rect(
                                                                                                            handPos.X - 20,
                                                                                                            handPos.Y - 20,
                                                                                                            40, 40
                                                                                ));

                            RightHandImage.Source = imgRightHandSource; //paints

                            DepthImagePixel[] depthPixels = new DepthImagePixel[sensor.DepthStream.FramePixelDataLength];
                            depthFrame.CopyDepthImagePixelDataTo(depthPixels);


                            bool handClosed = handDection.IsMakingAFist(depthPixels, handPos);
                            this.HandDescriptionTBox.Text = handClosed ? "Hand is closed" : "Hand is opened";
                        }
                        } catch { }
                    }
                }
            }
            catch
            {
            }
        }

   
        BitmapSource DepthToBitmapSource(DepthImageFrame imageFrame)
        {
            short[] pixelData = new short[imageFrame.PixelDataLength];
            imageFrame.CopyPixelDataTo(pixelData);
            BitmapSource bmap = BitmapSource.Create(imageFrame.Width, imageFrame.Height, 96, 96, PixelFormats.Gray16, null, pixelData, imageFrame.Width * imageFrame.BytesPerPixel);
            return bmap;
        }


        // get hand pos
        System.Drawing.Point? GetJoint2DPoint(JointType j, Skeleton S)
        {
            if (S.Joints[j].TrackingState != JointTrackingState.Tracked) return null;
            SkeletonPoint Sloc = S.Joints[j].Position;

            try
            {
                ColorImagePoint Cloc = sensor.CoordinateMapper.MapSkeletonPointToColorPoint(Sloc, ColorImageFormat.RgbResolution640x480Fps30);
                return new System.Drawing.Point(Cloc.X, Cloc.Y);
            }
            catch { };
            return new System.Drawing.Point(0, 0);
        }

        // get bitmap of colorstream
         Bitmap ImageToBitmap(ColorImageFrame Image)
        {
            if (colorPixels == null)
            {
                colorPixels = new byte[Image.PixelDataLength];
            }
            Image.CopyPixelDataTo(colorPixels);
            Bitmap bmap = new Bitmap(
                   Image.Width,
                   Image.Height,
                   System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            BitmapData bmapdata = bmap.LockBits(new System.Drawing.Rectangle(0, 0, Image.Width, Image.Height), ImageLockMode.WriteOnly, bmap.PixelFormat);
            IntPtr ptr = bmapdata.Scan0;
            Marshal.Copy(colorPixels, 0, ptr, Image.PixelDataLength);
            bmap.UnlockBits(bmapdata);
            return bmap;
        }

        // get hand cyrcle
         private BitmapSource DrawEllipses(ColorImageFrame CFrame)
         {
             Bitmap bmap = ImageToBitmap(CFrame);

             try
             {
                 foreach (Skeleton s in skeletons)
                 {
                     if (s.TrackingState == SkeletonTrackingState.Tracked)
                     {
                         Graphics g = Graphics.FromImage(bmap);

                         System.Drawing.Point? p3 = GetJoint2DPoint(JointType.HandRight, s);
                         System.Drawing.Pen pen3 = new System.Drawing.Pen(System.Drawing.Color.Purple);
                         pen3.Width = 10;
                         if (p3 != null)
                             g.DrawEllipse(pen3, p3.Value.X - 25, p3.Value.Y - 25, 50, 50);
                     }
                 }

             }
             catch (Exception)
             {

                 throw;
             }
             return BitmapToBitmapSource(bmap);
         }

         private BitmapSource BitmapToBitmapSource(Bitmap bitmap)
         {
             using (MemoryStream stream = new MemoryStream())
             {
                 bitmap.Save(stream, ImageFormat.Bmp);

                 stream.Position = 0;
                 BitmapImage result = new BitmapImage();
                 result.BeginInit();

                 result.CacheOption = BitmapCacheOption.OnLoad;
                 result.StreamSource = stream;
                 result.EndInit();
                 result.Freeze();
                 return result;
             }
         }
    }
}
