using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

/**
 * Mögliche Erweiterungen/ Verbesserungen
 * Handausschnitt über z achse anpassen und variabel halten.
 * Abweichungswerte nicht Hardcodieren, sondern über static
 * das proof of concept als queue mit variabler stabilität/länge einbinden
 */

namespace HandDetection
{
    public enum HandStatus
    {
        Opened,
        Closed,
        Unknown
    }

    public class HandTracker
    {
        private KinectSensor sensor;
        private bool handtracked = false;
        private SkeletonPoint rightHandPos;
        private Skeleton[] skeletons = new Skeleton[0];
        private DepthImagePixel[] depthPixels;
        private DepthImagePoint handPos = new DepthImagePoint();
        private DepthImageFormat depthImageFormate = DepthImageFormat.Resolution640x480Fps30;

        public HandTracker()
        {
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (sensor == null)
                throw new Exception("No Kinect ready");

            depthPixels = new DepthImagePixel[sensor.DepthStream.FramePixelDataLength];

            this.sensor.SkeletonStream.Enable();
            this.sensor.SkeletonFrameReady += SensorDetectHandReady;

            this.sensor.DepthStream.Enable(depthImageFormate);
            this.sensor.DepthFrameReady += SensorDepthFrameReady;

            try
            {
                this.sensor.Start();
            }
            catch (IOException)
            {
                this.sensor = null;
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
                
                    var person = skeletons.FirstOrDefault(p => p.TrackingState == SkeletonTrackingState.Tracked);
                    if (person != null)
                    {
                        handtracked = (person.Joints[JointType.HandRight].TrackingState == JointTrackingState.Tracked);

                        if (handtracked)
                        {
                            rightHandPos = person.Joints[JointType.HandRight].Position;
                            handPos = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(rightHandPos, depthImageFormate);
                        }
                    }
                }
            }
        }

        private void SensorDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    depthFrame.CopyDepthImagePixelDataTo(depthPixels);
                }
            }
        }

        public HandStatus GetHandOpenedClosedStatus()
        {
            if (!handtracked) return HandStatus.Unknown;
            if (((handPos.X + 42) > 640 || handPos.X - 42 <= 0)      // epsilon +2 wegen möglichem -1  von handPosX/Y
               || ((handPos.Y + 42) > 480 || handPos.Y - 42 <= 0))  // TODO die 40 und epsilon als static in HAndDetect
                    return HandStatus.Unknown;

            bool wasBlack = false;
            int blackWidth = 0, blackTimes = 0;
            int ystart = handPos.Y-40;
            int yend = handPos.Y + 40;
            int xstart = handPos.X - 40;
            int xend = handPos.X + 40;

            for (int yy = ystart; yy < yend - 10; yy += 10)
            {
                for (int xx = xstart; xx < xend; xx++)
                {
                    int depthIndex = xx + (yy * 640);
                    short depth = depthPixels[depthIndex].Depth;
                    DepthImagePixel depthPixel = depthPixels[depthIndex];
                    int player = depthPixel.PlayerIndex;

                    if (player > 0)
                    {
                        //need to detect if the hand is before the body
                        if (Math.Abs(handPos.Depth - depth) > 80)
                        {
                            wasBlack = false;
                            continue;
                        }

                        if (!wasBlack)
                        {
                            if (blackWidth > 1 && blackWidth < 15)
                                blackTimes++;
                            blackWidth = 0;
                        }
                        else
                        {
                            blackWidth++;
                        }
                        wasBlack = true;
                    }
                    else
                    {
                        wasBlack = false;
                    }
                }
                if (blackTimes > 1) 
                    return HandStatus.Opened;
            }
            return HandStatus.Closed;
        }

        //only proof of concept

        private List<HandStatus> handStatusList = new List<HandStatus>();
        private HandStatus lastHandStatus;

        public HandStatus GetHandOpenedClosedStatusBuffered()
        {
            HandStatus currentHandStatus = GetHandOpenedClosedStatus();
            handStatusList.Add(currentHandStatus);

            if (handStatusList.Count() == 3)
            {
                if(handStatusList.All(x => x == handStatusList.First()))
                {
                    lastHandStatus = currentHandStatus;
                }
                handStatusList.Clear();   
            }

            return lastHandStatus;
        }
    }
}
