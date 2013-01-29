using System;
using System.Linq;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.FaceTracking;

namespace HtwKinect
{
    /*Diese Klasse verwaltet die elementaren Kinect-Resourcen*/
    public class KinectHelper 
    {

        private readonly KinectSensor _kinectSensor;
        private Skeleton[] _skeletons;
        private DepthImagePixel[] _depthImagePixels;
        private short[] _depthPixels;
        private byte[] _colorPixels;
        private readonly FaceTracker _faceTracker;
        private ColorImageFrame _colorImageFrame;
        private DepthImageFrame _depthImageFrame;
        private FaceTrackFrame _faceFrame;
        public static KinectHelper Instance;
        private int _id = -1;

        public event EventHandler AllFramesDispatchedEvent;
        

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
        }

        public static KinectHelper GetInstance()
        {
            if (Instance == null)
                Instance = new KinectHelper(new TransformSmoothParameters
                    {
                        Correction = 0,
                        JitterRadius = 0,
                        MaxDeviationRadius = 0.8f,
                        Prediction = 0,
                        Smoothing = 0.8f
                    },
                    false,
                    ColorImageFormat.RgbResolution640x480Fps30,
                    DepthImageFormat.Resolution640x480Fps30);
            return Instance;
        }

        /*Diese Methode sorgt dafür, dass immer zuverlaessig immer nur der Skeleton der selben Person zurückgegeben wird*/
        public Skeleton GetFixedSkeleton()
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

        private void AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    if (_skeletons == null)
                        _skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(_skeletons);
                    //CorrectRoomCoords();
                    using (ColorImageFrame colorImageFrame = e.OpenColorImageFrame())
                    {
                        if (colorImageFrame != null)
                        {
                            if (_colorPixels == null)
                                _colorPixels = new byte[colorImageFrame.PixelDataLength];
                            colorImageFrame.CopyPixelDataTo(_colorPixels);
                            _colorImageFrame = colorImageFrame;
                            using (DepthImageFrame depthImageFrame = e.OpenDepthImageFrame())
                            {
                                if (depthImageFrame != null)
                                {
                                    if (_depthImagePixels == null)
                                        _depthImagePixels = new DepthImagePixel[depthImageFrame.PixelDataLength];
                                    if (_depthPixels == null)
                                        _depthPixels = new short[depthImageFrame.PixelDataLength];
                                    depthImageFrame.CopyDepthImagePixelDataTo(_depthImagePixels);
                                    depthImageFrame.CopyPixelDataTo(_depthPixels);
                                    _depthImageFrame = depthImageFrame;
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
                    foreach (Skeleton s in _skeletons)
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
            if (AllFramesDispatchedEvent != null)
            {
                AllFramesDispatchedEvent(this, EventArgs.Empty);
            }
        }

        public Skeleton[] GetSkeletons()
        {
            return _skeletons;
        }

        public DepthImagePixel[] GetDepthImagePixels()
        {
            return _depthImagePixels;
        }

        public short[] GetDepthPixels()
        {
            return _depthPixels;
        }

        public byte[] GetColorPixels()
        {
            return _colorPixels;
        } 

        public FaceTrackFrame GetFaceTrackFrame(Skeleton skeleton)
        {
            if (_faceFrame == null) /* Aus effizienzgruenden wird nicht bei jedem Zugriff ein neues Faceframe erzeugt, sondern nur ein Mal pro Frame. Siehe OnAllFramesReady unten.*/
                _faceFrame = _faceTracker.Track(_kinectSensor.ColorStream.Format, _colorPixels, _kinectSensor.DepthStream.Format, _depthPixels, skeleton);
            return _faceFrame;
        }

        public ColorImageFrame GetColorImageFrame()
        {
            return _colorImageFrame;
        }

        public DepthImageFrame GetDepthImageFrame()
        {
            return _depthImageFrame;
        }

        public KinectSensor GetSensor()
        {
            return _kinectSensor;
        }
    } 
}
