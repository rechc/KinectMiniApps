using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AccessoryLib;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.FaceTracking;
using System.Windows.Media;

namespace HtwKinect
{
    /*Diese Klasse verwaltet die elementaren Kinect-Resourcen*/
    public class KinectHelper 
    {
        private readonly KinectSensor _kinectSensor;
        private readonly FaceTracker _faceTracker;
        private FaceTrackFrame _faceFrame;
        private int _id = -1;
        private static KinectHelper _instance;

        public static KinectHelper Instance {
            get
            {
                if (_instance == null)
                    _instance = new KinectHelper(new TransformSmoothParameters
                    {
                        Correction = 0,
                        JitterRadius = 0,
                        MaxDeviationRadius = 0.8f,
                        Prediction = 0,
                        Smoothing = 0.8f
                    },
                        false,
                        ColorImageFormat.RgbResolution1280x960Fps12,
                        DepthImageFormat.Resolution640x480Fps30);
                return _instance;
            }
        }
        public KinectSensor Sensor { get { return _kinectSensor; } }
        public Skeleton[] Skeletons { get; private set; }
        public DepthImagePixel[] DepthImagePixels { get; private set; }
        public short[] DepthPixels { get; private set; }
        public byte[] ColorPixels { get; private set; }
        public ColorImageFrame ColorImageFrame { get; private set; }
        public DepthImageFrame DepthImageFrame { get; private set; }
        public DepthImageFormat DepthImageFormat { get; private set; }
        public ColorImageFormat ColorImageFormat { get; private set; }

        public event EventHandler ReadyEvent;
        
        private KinectHelper(TransformSmoothParameters tsp, bool near, ColorImageFormat colorFormat, DepthImageFormat depthFormat) {
            _kinectSensor = KinectSensor.KinectSensors.FirstOrDefault(s => s.Status == KinectStatus.Connected);

            if (_kinectSensor == null)
            {
                throw new Exception("No Kinect-Sensor found.");
            }
            if (near)
            {
                _kinectSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                _kinectSensor.DepthStream.Range = DepthRange.Near;
                _kinectSensor.SkeletonStream.EnableTrackingInNearRange = true;
            }
            _kinectSensor.SkeletonStream.Enable(tsp);
            _kinectSensor.ColorStream.Enable(colorFormat);
            _kinectSensor.DepthStream.Enable(depthFormat);
            _kinectSensor.AllFramesReady += AllFramesReady;
            
            _kinectSensor.Start();
            _faceTracker = new FaceTracker(_kinectSensor);

            DepthImageFormat = depthFormat;
            ColorImageFormat = colorFormat;
        }


        /*Diese Methode sorgt dafür, dass immer zuverlaessig immer nur der Skeleton der selben Person zurückgegeben wird*/
        public Skeleton GetFixedSkeleton()
        {
            Skeleton skeleton = null;
            if (Skeletons != null)
            {
                if (_id == -1)
                {
                    skeleton = Skeletons.FirstOrDefault(s => s.TrackingState == SkeletonTrackingState.Tracked);
                    if (skeleton != null)
                    {
                        _id = skeleton.TrackingId;
                    }
                }
                else
                {
                    skeleton = Skeletons.FirstOrDefault(s => s.TrackingState == SkeletonTrackingState.Tracked && s.TrackingId == _id);
                    if (skeleton == null)
                    {
                        skeleton = Skeletons.FirstOrDefault(s => s.TrackingState == SkeletonTrackingState.Tracked);
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

        private void AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    if (Skeletons == null)
                        Skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(Skeletons);
                    //CorrectRoomCoords();
                    using (ColorImageFrame colorImageFrame = e.OpenColorImageFrame())
                    {
                        if (colorImageFrame != null)
                        {
                            if (ColorPixels == null)
                                ColorPixels = new byte[colorImageFrame.PixelDataLength];
                            colorImageFrame.CopyPixelDataTo(ColorPixels);
                            ColorImageFrame = colorImageFrame;
                            using (DepthImageFrame depthImageFrame = e.OpenDepthImageFrame())
                            {
                                if (depthImageFrame != null)
                                {
                                    if (DepthImagePixels == null)
                                        DepthImagePixels = new DepthImagePixel[depthImageFrame.PixelDataLength];
                                    depthImageFrame.CopyDepthImagePixelDataTo(DepthImagePixels);
                                    if (DepthPixels == null)
                                        DepthPixels = new short[depthImageFrame.PixelDataLength];
                                    depthImageFrame.CopyPixelDataTo(DepthPixels);
                                    DepthImageFrame = depthImageFrame;
                                    _faceFrame = null;
                                    FireAllFramesDispatched();
                                }
                            }
                        }
                    }
                }
            }
        }

        /*Korrigiert die Raumverzerrung bei geneigter Kinect*/
        private void CorrectRoomCoords()
        {
            if (_kinectSensor.IsRunning)
            {
                int elevationAngle = _kinectSensor.ElevationAngle;
                if (elevationAngle != 0)
                {
                    int angle = elevationAngle * -1;
                    foreach (Skeleton s in Skeletons)
                    {
                        foreach (JointType jt in Enum.GetValues(typeof(JointType)))
                        {
                            var joint = s.Joints[jt];
                            var position = joint.Position;
                            position.Y = (float)(position.Y * Math.Cos(angle * Math.PI / 180) - position.Z * Math.Sin(angle * Math.PI / 180));
                            position.Z = (float)(position.Y * Math.Sin(angle * Math.PI / 180) + position.Z * Math.Cos(angle * Math.PI / 180));
                            joint.Position = position;
                            s.Joints[jt] = joint;
                        }
                    }
                }
            }
        }

        private void FireAllFramesDispatched()
        {
            if (ReadyEvent != null)
            {
                ReadyEvent(this, EventArgs.Empty);
            }
        }

        public FaceTrackFrame GetFaceTrackFrame(Skeleton skeleton)
        {
            if (_faceFrame == null) /* Aus effizienzgruenden wird nicht bei jedem Zugriff ein neues Faceframe erzeugt, sondern nur ein Mal pro Frame. Siehe OnAllFramesReady unten.*/
                _faceFrame = _faceTracker.Track(_kinectSensor.ColorStream.Format, ColorPixels, _kinectSensor.DepthStream.Format, DepthPixels, skeleton);
            return _faceFrame;
        }

        /// <summary>
        /// Erstellt ein Transform-Objekt, das den Bildbereich auf den aktiven
        /// Bereich des Tiefensensors begrenzt.
        /// </summary>
        /// <param name="depthmm">Entfernung, die fuer das Mapping von
        /// Tiefeninformation auf das Farbbild verwendet wird.
        /// Standart: 1000</param>
        public Transform CreateTransform(int depthmm = 1000)
        {
            var transforms = new TransformGroup();
            var mapper = Sensor.CoordinateMapper;
            int w = Sensor.DepthStream.FrameWidth;
            int h = Sensor.DepthStream.FrameHeight;
            double y0 = mapper.MapDepthPointToColorPoint(
                DepthImageFormat,
                new DepthImagePoint() { X = w/2, Y = 0, Depth = depthmm },
                ColorImageFormat).Y;
            double yh = mapper.MapDepthPointToColorPoint(
                DepthImageFormat,
                new DepthImagePoint() { X = w/2, Y = h, Depth = depthmm },
                ColorImageFormat).Y;
            double x0 = mapper.MapDepthPointToColorPoint(
                DepthImageFormat,
                new DepthImagePoint() { X = 0, Y = h/2, Depth = depthmm },
                ColorImageFormat).X;
            double xw = mapper.MapDepthPointToColorPoint(
                DepthImageFormat,
                new DepthImagePoint() { X = w, Y = h/2, Depth = depthmm },
                ColorImageFormat).X;
            transforms.Children.Add(new TranslateTransform(-x0, -y0));
            transforms.Children.Add(new ScaleTransform(
                Sensor.ColorStream.FrameWidth / (xw - x0),
                Sensor.ColorStream.FrameHeight / (yh - y0)));
            return transforms;
        }

        public Viewbox GetScaledControl(FrameworkElement frameWorkElement)
        {
            frameWorkElement.RenderTransform = CreateTransform();
            frameWorkElement.Width = Sensor.ColorStream.FrameWidth; //TODO ist das wirklich notwendig??
            frameWorkElement.Height = Sensor.ColorStream.FrameHeight; //TODO ist das wirklich notwendig??
            Viewbox box = new Viewbox { Child = frameWorkElement, Stretch = Stretch.Fill, ClipToBounds = true };
            return box;
        }
    } 
}
