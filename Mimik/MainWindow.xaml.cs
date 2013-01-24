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

using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.FaceTracking;
using System.Diagnostics;

namespace Mimik
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensor kinectSensor;
        FaceTracker faceTracker;
        private byte[] colorPixelData;
        private short[] depthPixelData;
        private DepthImagePixel[] depthPixels;
        private Skeleton[] skeletonData;
        private WriteableBitmap colorBitmap;

        private float oldStretch = -2;

        public MainWindow()
        {
            InitializeComponent();

            // For a KinectSensor to be detected, we can plug it in after the application has been started.
            KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;
            // Or it's already plugged in, so we will look for it.
            var kinect = KinectSensor.KinectSensors.FirstOrDefault(k => k.Status == KinectStatus.Connected);
            if (kinect != null)
            {
                OpenKinect(kinect);

                // This is the bitmap we'll display on-screen
                this.colorBitmap = new WriteableBitmap(this.kinectSensor.ColorStream.FrameWidth, this.kinectSensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                this.Image.Source = this.colorBitmap;

                this.depthPixels = new DepthImagePixel[this.kinectSensor.DepthStream.FramePixelDataLength];
            }
        }

        /// <summary>
        /// Handles the StatusChanged event of the KinectSensors control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Microsoft.Kinect.StatusChangedEventArgs"/> instance containing the event data.</param>
        void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            if (e.Status == KinectStatus.Connected)
            {
                OpenKinect(e.Sensor);
            }
        }

        /// <summary>
        /// Opens the kinect.
        /// </summary>
        /// <param name="newSensor">The new sensor.</param>
        private void OpenKinect(KinectSensor newSensor)
        {
            kinectSensor = newSensor;

            // Initialize all the necessary streams:
            // - ColorStream with default format
            // - DepthStream with Near mode
            // - SkeletonStream with tracking in NearReange and Seated mode.

            kinectSensor.ColorStream.Enable();

            // kinectSensor.DepthStream.Range = DepthRange.Near;
            kinectSensor.DepthStream.Enable(DepthImageFormat.Resolution80x60Fps30);

            kinectSensor.SkeletonStream.EnableTrackingInNearRange = true;
            kinectSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
            kinectSensor.SkeletonStream.Enable(new TransformSmoothParameters() { Correction = 0.5f, JitterRadius = 0.05f, MaxDeviationRadius = 0.05f, Prediction = 0.5f, Smoothing = 0.5f });

            // Listen to the AllFramesReady event to receive KinectSensor's data.
            kinectSensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(kinectSensor_AllFramesReady);

            // Initialize data arrays
            colorPixelData = new byte[kinectSensor.ColorStream.FramePixelDataLength];
            depthPixelData = new short[kinectSensor.DepthStream.FramePixelDataLength];
            skeletonData = new Skeleton[6];

            // Starts the Sensor
            kinectSensor.Start();

            // Initialize a new FaceTracker with the KinectSensor
            faceTracker = new FaceTracker(kinectSensor);
        }

        /// <summary>
        /// Handles the AllFramesReady event of the kinectSensor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Microsoft.Kinect.AllFramesReadyEventArgs"/> instance containing the event data.</param>
        void kinectSensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            // Retrieve each single frame and copy the data
            using (ColorImageFrame colorImageFrame = e.OpenColorImageFrame())
            {
                if (colorImageFrame == null)
                    return;
                colorImageFrame.CopyPixelDataTo(colorPixelData);

                this.colorBitmap.WritePixels(
                    new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                    this.colorPixelData,
                    this.colorBitmap.PixelWidth * sizeof(int),
                    0);
            }

            using (DepthImageFrame depthImageFrame = e.OpenDepthImageFrame())
            {
                if (depthImageFrame == null)
                    return;
                depthImageFrame.CopyPixelDataTo(depthPixelData);
                depthImageFrame.CopyDepthImagePixelDataTo(depthPixels);
            }

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame == null)
                    return;
                skeletonFrame.CopySkeletonDataTo(skeletonData);
            }

            // Retrieve the first tracked skeleton if any. Otherwise, do nothing.
            var skeleton = skeletonData.FirstOrDefault(s => s.TrackingState == SkeletonTrackingState.Tracked);
            if (skeleton == null)
                return;

            // Make the faceTracker processing the data.
            EmotionTracker(kinectSensor.ColorStream.Format, colorPixelData,
                           kinectSensor.DepthStream.Format, depthPixelData,
                           skeleton);
        }

        private void EmotionTracker(ColorImageFormat colorImageFormat, byte[] colorImage, DepthImageFormat depthImageFormat, short[] depthImage, Skeleton skeletonOfInterest) 
        {
            FaceTrackFrame faceFrame = faceTracker.Track(kinectSensor.ColorStream.Format, colorPixelData,
                                             kinectSensor.DepthStream.Format, depthPixelData,
                                             skeletonOfInterest);

            // If a face is tracked, then we can use it.
            if (faceFrame.TrackSuccessful)
            {
                var list = faceFrame.GetProjected3DShape();
                if (oldStretch == -2)
                {
                    oldStretch = Math.Abs(list[31].X - list[64].X);
                }
                float newStretch = Math.Abs(list[31].X - list[64].X);
                Debug.WriteLine(newStretch);
                if (Math.Abs(newStretch - oldStretch) > 3)
                {
                    if (laugh)
                    {
                        laugh = false;
                    }
                    else
                    {
                        laugh = true;
                    }
                    Debug.WriteLine(laugh);
                }
                oldStretch = newStretch;
            }
            
        }
        bool laugh = false;
        private void FireSurprised()
        {
            EmotionResult.Text = "Überascht";
        }

        private void FireHappy()
        {
            EmotionResult.Text = "Lachen";
        }
    }
}
