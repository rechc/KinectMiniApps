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

        private bool IsHandTracked(Joint hand)
        {
            return (hand.TrackingState == JointTrackingState.Tracked);
        }

        private DepthImagePoint GetHandPos(KinectSensor sensor, Joint handJoint, DepthImageFormat depthImageFormate)
        {
            return sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(handJoint.Position, depthImageFormate);
        }

        public HandStatus GetHandOpenedClosedStatus(DepthImagePixel[] depthPixels, Joint handJoint, KinectSensor sensor, 
                                                                                        DepthImageFormat depthImageFormate)
        {
            if (!IsHandTracked(handJoint)) return HandStatus.Unknown;

            DepthImagePoint handPos = GetHandPos(sensor, handJoint, depthImageFormate);

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

        public HandStatus GetHandOpenedClosedStatusBuffered(DepthImagePixel[] depthPixels, Joint handJoint, KinectSensor sensor, SkeletonPoint handSkeletonPoint, DepthImageFormat depthImageFormate)
        {
            HandStatus currentHandStatus = GetHandOpenedClosedStatus(depthPixels, handJoint, sensor, depthImageFormate);
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
