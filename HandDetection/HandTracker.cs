using Microsoft.Kinect;
using System;

/**
 * Mögliche Erweiterungen/ Verbesserungen
 * Handausschnitt über z achse anpassen und variabel halten. --- ok
 * Abweichungswerte nicht Hardcodieren, sondern über static --- ok
 * das proof of concept als queue mit variabler stabilität/länge einbinden --- ok
 * Nun kann eigentlich nur noch der algorithmus zum erkennen verbessert werden ... 
 * eine Möglichkeit Handausrichtung erkennen ... oben seite unten .... und algorithmus diesbezüglich optimieren
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

        // vars for Buffer
        private readonly int[] _handstatusarray;
        private int _bufferIterator;

        //vars for cutout handsize
        public static int EpsilonTolerance = 2;

        public HandTracker(int bufferSize = 15)
        {
            _handstatusarray = new int[bufferSize];
        }

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
            int halfhandCutSize = ComputeHandSize(handJoint)/2;
            if (((handPos.X + EpsilonTolerance + halfhandCutSize) > sensor.DepthStream.FrameWidth || handPos.X - halfhandCutSize - EpsilonTolerance <= 0)      // epsilon +2 wegen möglichem -1  von handPosX/Y
               || ((handPos.Y + halfhandCutSize + EpsilonTolerance) > sensor.DepthStream.FrameHeight || handPos.Y - halfhandCutSize - EpsilonTolerance <= 0))
                return HandStatus.Unknown;

            bool wasBlack = false;
            int blackWidth = 0, blackTimes = 0;
            int ystart = handPos.Y - halfhandCutSize;
            int yend = handPos.Y + halfhandCutSize;
            int xstart = handPos.X - halfhandCutSize;
            int xend = handPos.X + halfhandCutSize;

            for (int yy = ystart; yy < yend - 10; yy += 10)
            {
                for (int xx = xstart; xx < xend; xx++)
                {
                    int depthIndex = xx + (yy * sensor.DepthStream.FrameWidth);
                    short depth = depthPixels[depthIndex].Depth;
                    DepthImagePixel depthPixel = depthPixels[depthIndex];
                    int player = depthPixel.PlayerIndex;

                    if (player > 0)
                    {
                        //need to detect if the hand is in front of the body
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

        /**
         * computes the cout out of the hand, with the help of z position 
         */
        public int ComputeHandSize(Joint handJoint)
        {
            const double g = 0.22; // Objektgroesse in m.
            double r = handJoint.Position.Z;  // Entfernung in m.
            double imgWidth = 2 * Math.Atan(g / (2 * r)) * 600/*(px / g)*/;
            return (int)imgWidth;
        }

        // V2 
        public HandStatus GetBufferedHandStatus(DepthImagePixel[] depthPixels, Joint handJoint, KinectSensor sensor, DepthImageFormat depthImageFormate)
        {
            HandStatus currentHandStatusEnum = GetHandOpenedClosedStatus(depthPixels, handJoint, sensor, depthImageFormate);

            //enum to int
            int currentHandStatus = 0;
            if (currentHandStatusEnum == HandStatus.Closed)
            {
                currentHandStatus = 1;
            }
            else if (currentHandStatusEnum == HandStatus.Opened)
            {
                currentHandStatus = 2;
            }

            //loop overwrite
            _bufferIterator = (_bufferIterator == _handstatusarray.Length) ? 0 : _bufferIterator;

            _handstatusarray[_bufferIterator] = currentHandStatus;
            _bufferIterator++;

            // double Counter
            int openCounter = 0;
            int closedCounter = 0;
            foreach (int currentEntry in _handstatusarray)
            {
                if (currentEntry == 1)
                {
                    closedCounter++;
                }
                else if (currentEntry == 2)
                {
                    openCounter++;
                }
            }

            //output
            /* foreach (int obj in handstatusarray)
                 Console.Write("    {0}", obj);
             Console.WriteLine();*/

            if (closedCounter > _handstatusarray.Length / 2)
            {
                return HandStatus.Closed;
            }
            return openCounter > _handstatusarray.Length / 2 ? HandStatus.Opened : HandStatus.Unknown;
        }
    }
}
