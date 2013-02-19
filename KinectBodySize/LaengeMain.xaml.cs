using System;
using System.Collections.Generic;
using System.Linq;
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
using Microsoft.Kinect;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;
using System.Windows.Interop;
using System.Diagnostics;

namespace KinectTutorial
{
    /// <summary>
    /// Interaktionslogik für LaengeMain.xaml
    /// </summary>
    public partial class LaengeMain : Window
    {
        private KinectSensor _kinectSensor { get; set; }
        private Skeleton[] _skeletons;
        private byte[] _colorPixels;
        private short[] _depthPixelData;
        private WriteableBitmap _outputImageColor;
        private int _id = -1;

        public LaengeMain()
        {
            InitializeComponent();
            _kinectSensor = KinectSensor.KinectSensors.FirstOrDefault(s => s.Status == KinectStatus.Connected);

            if (_kinectSensor == null)
            {
                TextBlockCoords.Text = "Kein Kinect-Sensor angeschloßen";
            }
            else
            {
                _kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

                _kinectSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);

                _kinectSensor.SkeletonStream.Enable(new TransformSmoothParameters()
                {
                    Smoothing = 0.8f,
                    Correction = 0f,
                    Prediction = 0.2f,
                    JitterRadius = 0.1f,
                    MaxDeviationRadius = 0.8f
                });

                _kinectSensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(OnAllReady);

                _kinectSensor.Start();
            }
        }

        private void OnAllReady(Object sender, AllFramesReadyEventArgs e)
        {
            using (ColorImageFrame CFrame = e.OpenColorImageFrame())
            {
                using (DepthImageFrame DFrame = e.OpenDepthImageFrame())
                {
                    if (DFrame != null)
                    {
                        if (_depthPixelData == null)
                        {
                            _depthPixelData = new short[DFrame.PixelDataLength];
                        }
                    }
                    if (CFrame != null)
                    {
                        if (_colorPixels == null)
                        {
                            _colorPixels = new byte[CFrame.PixelDataLength];
                        }
                        CFrame.CopyPixelDataTo(_colorPixels);
                        RefreshColorImage(CFrame);
                    }
                    using (SkeletonFrame SFrame = e.OpenSkeletonFrame())
                    {
                        if (SFrame != null)
                        {
                            if (_skeletons == null)
                            {
                                _skeletons = new Skeleton[_kinectSensor.SkeletonStream.FrameSkeletonArrayLength];
                            }
                            SFrame.CopySkeletonDataTo(_skeletons);
                        }
                        SkeletonPoint? highestBodyPoint = null;
                        SkeletonPoint? lowestBodyPoint = null;
                        if (DFrame != null)
                        {
                            DFrame.CopyPixelDataTo(_depthPixelData);
                            DepthImage.Source = DepthToBitmapSource(DFrame);
                            highestBodyPoint = GetExtremeBodyPoint(DFrame, true);
                            lowestBodyPoint = GetExtremeBodyPoint(DFrame, false);
                            if (highestBodyPoint.HasValue && lowestBodyPoint.HasValue)
                            {
                                TextBlockCoords2.Text = "Man sieht dich " + (int)((highestBodyPoint.Value.Y - lowestBodyPoint.Value.Y)*100) + " cm hoch";
                            }
                        }
                        RefreshCirclesImage(CFrame, highestBodyPoint, lowestBodyPoint);
                    }
                }
            }
        }

        private SkeletonPoint? GetExtremeBodyPoint(DepthImageFrame Dif, bool highest)
        {
            Skeleton skeleton = getCurrentSkeleton();
            if (skeleton != null)
            {
                if (skeleton.Joints[JointType.Head].TrackingState != JointTrackingState.Tracked)
                {
                    return null;
                }
                int skeletonIndex = -1;
                for (int i = 0; i < _skeletons.Count(); i++)
                {
                    if (_skeletons[i].TrackingId == _id)
                    {
                        skeletonIndex = i + 1;
                        break;
                    }
                }

                for (int i = highest ? 0 : _depthPixelData.Count() - 1; highest ? i < _depthPixelData.Count():i >= 0;)
                {

                    short pixel = _depthPixelData[i];

                    if ((pixel & DepthImageFrame.PlayerIndexBitmask) == skeletonIndex)
                    {
                        DepthImagePoint dip = new DepthImagePoint();
                        dip.X = (i % Dif.Width);
                        dip.Y = (i / Dif.Width);
                        dip.Depth = pixel >> DepthImageFrame.PlayerIndexBitmaskWidth;
                        return _kinectSensor.CoordinateMapper.MapDepthPointToSkeletonPoint(DepthImageFormat.Resolution640x480Fps30, dip);
                    }
                    if (highest)
                    {
                        i++;
                    }
                    else
                    {
                        i--;
                    }
                }
            }
            return null;
        }

        private void RefreshColorImage(ColorImageFrame imageFrame)
        {
            if (_outputImageColor == null)
            {
                this._outputImageColor = new WriteableBitmap(
                            imageFrame.Width,
                            imageFrame.Height,
                            96,  // DpiX
                            96,  // DpiY
                            PixelFormats.Bgr32,
                            null);
            }

            this._outputImageColor.WritePixels(
                                    new Int32Rect(0, 0, imageFrame.Width, imageFrame.Height),
                                    _colorPixels,
                                    imageFrame.Width * imageFrame.BytesPerPixel,
                                    0);
            ColorImage.Source = this._outputImageColor;
        }


        private void PrintCoords()
        {
            Skeleton skeleton = _skeletons.FirstOrDefault(s => s.TrackingState == SkeletonTrackingState.Tracked);
            if (skeleton != null)
            {
                TextBlockCoords.Text = "X:" + skeleton.Joints[JointType.HandLeft].Position.X + "\n" +
                    "Y:" + skeleton.Joints[JointType.HandLeft].Position.Y + "\n" +
                    "Z:" + skeleton.Joints[JointType.HandLeft].Position.Z + "\n";
            }
        }



        private void RefreshCirclesImage(ColorImageFrame CFrame, SkeletonPoint? highest, SkeletonPoint? lowest)
        {
            if (CFrame != null)
            {
                Bitmap circlesBitmap = new Bitmap(CFrame.Width, CFrame.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                Skeleton skeleton = getCurrentSkeleton();

                Graphics g = Graphics.FromImage(circlesBitmap);
                if (skeleton != null)
                {
                    System.Drawing.Point? p1 = GetJoint2DPoint(JointType.HandLeft, skeleton);
                    System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.Red);
                    pen.Width = 10;
                    if (p1 != null)
                        g.DrawEllipse(pen, p1.Value.X - 25, p1.Value.Y - 25, 50, 50);
                }
                if (highest.HasValue)
                {
                    ColorImagePoint Cloc = _kinectSensor.CoordinateMapper.MapSkeletonPointToColorPoint(highest.Value, ColorImageFormat.RgbResolution640x480Fps30);
                    System.Drawing.Point p = new System.Drawing.Point(Cloc.X, Cloc.Y);
                    System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.White);
                    pen.Width = 10;
                    if (p != null)
                    {
                        try
                        {
                            g.DrawEllipse(pen, p.X - 5, p.Y - 5, 10, 10);
                        }
                        catch { };
                    }
                }
                if (lowest.HasValue)
                {
                    ColorImagePoint Cloc = _kinectSensor.CoordinateMapper.MapSkeletonPointToColorPoint(lowest.Value, ColorImageFormat.RgbResolution640x480Fps30);
                    System.Drawing.Point p = new System.Drawing.Point(Cloc.X, Cloc.Y);
                    System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.White);
                    pen.Width = 10;
                    if (p != null)
                    {
                        try
                        {
                            g.DrawEllipse(pen, p.X - 5, p.Y - 5, 10, 10);
                        }
                        catch { };
                    }
                }
                IntPtr hBmp = circlesBitmap.GetHbitmap();
                System.Windows.Media.Imaging.BitmapSource source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBmp, IntPtr.Zero, System.Windows.Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromWidthAndHeight(circlesBitmap.Width, circlesBitmap.Height));
                circlesBitmap.Dispose();
                CircleImage.Source = source;
                DeleteObject(hBmp);
            }
        }

        private Skeleton getCurrentSkeleton()
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

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);



        BitmapSource DepthToBitmapSource(DepthImageFrame imageFrame)
        {

            BitmapSource bmap = BitmapSource.Create(imageFrame.Width, imageFrame.Height, 96, 96, PixelFormats.Gray16, null, _depthPixelData, imageFrame.Width * imageFrame.BytesPerPixel);
            return bmap;
        }


        System.Drawing.Point? GetJoint2DPoint(JointType j, Skeleton S)
        {
            if (S.Joints[j].TrackingState != JointTrackingState.Tracked) return null;
            SkeletonPoint Sloc = S.Joints[j].Position;

            try
            {
                ColorImagePoint Cloc = _kinectSensor.CoordinateMapper.MapSkeletonPointToColorPoint(Sloc, ColorImageFormat.RgbResolution640x480Fps30);
                return new System.Drawing.Point(Cloc.X, Cloc.Y);
            }
            catch { };
            return new System.Drawing.Point(0, 0);
        }
    }
}
