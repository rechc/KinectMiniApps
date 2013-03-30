using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AccessoryLib
{
	/**
	 * Klasse zum zeichnen der Accessoiries
	 */
    public partial class AccessoryControl : UserControl
    {
        private KinectSensor _sensor;
        private Skeleton[] _skeletons;
        private Skeleton _activeSkeleton;
        private Boolean _oneHut;
        public Rect AccessoryRect { get; private set; }

        // Liste von Accessoiries
        public List<AccessoryItem> AccessoryItems { get; private set; }

		/**
		 * Konstruktor
		 */
        public AccessoryControl()
        {
            InitializeComponent();
            AccessoryItems = new List<AccessoryItem>();
        }

		/**
		 * Setzt die Skeletons neu
		 */
        public void SetSkeletons(Skeleton[] skeletons)
        {
            _skeletons = skeletons;
            _oneHut = false;
            InvalidateVisual();
        }

		/**
		 * Setzt den aktiven Skeleton neu
		 */
        public void SetActiveSkeleton(Skeleton activeSkeleton)
        {
            _activeSkeleton = activeSkeleton;
            _oneHut = true;
            InvalidateVisual();
        }

		/**
		 * Initialisiert den Sensor
		 */
        public void Start(KinectSensor sensor)
        {
            _sensor = sensor;
        }

        /**
		 * Überladung der OnRender Methode 
		 */
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            
            if (_oneHut)
            {
				// Nur Accessoiries für die aktive Person
                if (_activeSkeleton == null)
                    return;

                if (_activeSkeleton.TrackingState == SkeletonTrackingState.Tracked)
                    RenderAccessories(drawingContext, _activeSkeleton);
            }
            else
            {
			 	// Alle Personen bekommen die Accessories
                if (_skeletons == null)
                    return;

                foreach (Skeleton person in _skeletons)
                {
                    if (person.TrackingState == SkeletonTrackingState.Tracked)
                        RenderAccessories(drawingContext, person);
                }
            }
        }

        /**
		 * Überprüft ob die aktive Person geändert werden soll
		 */
        public Boolean CheckAccessoriesNew()
        {
            if (_activeSkeleton != null && AccessoryRect != null)
            {
                Point left = SkeletonPointToScreen(_activeSkeleton.Joints[JointType.HandLeft].Position);
                Point right = SkeletonPointToScreen(_activeSkeleton.Joints[JointType.HandRight].Position);
                if ((left.X >= AccessoryRect.Left && left.X <= AccessoryRect.Right &&
                     left.Y >= AccessoryRect.Top && left.Y <= AccessoryRect.Bottom) ||
                    (right.X >= AccessoryRect.Left && right.X <= AccessoryRect.Right &&
                     right.Y >= AccessoryRect.Top && right.Y <= AccessoryRect.Bottom))
                {
                    return true;
                }
            }
            return false;
        }

        /**
		 * Methode zum mappen der Skeleton Punkte zu Farbpunkten 
		 */
        private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            int marginWindow = 50;
            ColorImagePoint colorPoint = _sensor.CoordinateMapper.MapSkeletonPointToColorPoint(skelpoint, _sensor.ColorStream.Format);
            double scalX = (System.Windows.SystemParameters.PrimaryScreenWidth - marginWindow) / _sensor.ColorStream.FrameWidth; 
            double scalY = (System.Windows.SystemParameters.PrimaryScreenHeight - marginWindow) / _sensor.ColorStream.FrameHeight;
            return new Point(colorPoint.X * scalX, colorPoint.Y * scalY);
        }

        /**
		 * Zeichnet alle Items fuer eine einzelne Person. 
		 */
        private void RenderAccessories(DrawingContext drawingContext, Skeleton person)
        {
            foreach (var item in AccessoryItems)
            {
                RenderAccessoryItem(drawingContext, person, item);
            }
        }

        /**
		 * Zeichnet ein Item
		 */
        private void RenderAccessoryItem(DrawingContext drawingContext, Skeleton person, AccessoryItem item)
        {
            SkeletonPoint headPos = person.Joints[JointType.Head].Position;

            ColorImagePoint colorImagePoint = _sensor.CoordinateMapper.MapSkeletonPointToColorPoint(headPos,
                                                                                                    _sensor.ColorStream
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
                    offsetY = -1.1*imgHeight;
                    break;
                case AccessoryPositon.Beard:
                    offsetY = imgHeight/4;
                    break;
            }

            double headX = colorImagePoint.X * (ActualWidth / _sensor.ColorStream.FrameWidth) + offsetX;
            double headY = colorImagePoint.Y * (ActualHeight / _sensor.ColorStream.FrameHeight) + offsetY;

            AccessoryRect = new Rect(headX - imgWidth / 2, headY, imgWidth, imgHeight);
            drawingContext.DrawImage(item.Image, AccessoryRect);
        }
    }
}
