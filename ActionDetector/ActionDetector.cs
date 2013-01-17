using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    class ActionDetector
    {
        private List<Skeleton[]> skeletonsList = new List<Skeleton[]>();
        private const int storeElementsInList = 10;
        private const double passingKinectEpsilon = 0.01;

        public ActionDetector()
        {
        }

        public ActionDetector(Skeleton[] skeletons)
        {
            addSkeleton(skeletons);
        }

        /*
         *   returns a Skeleton-List of PositionOnly People
         */
        public List<Skeleton> GetPositionOnlyPeople()
        {
            List<Skeleton> positionOnlyPeople = new List<Skeleton>();
            foreach (Skeleton skeleton in skeletonsList[0])
            {
                if (skeleton.TrackingState == SkeletonTrackingState.PositionOnly)
                {
                    positionOnlyPeople.Add(skeleton);
                }
            }
            return positionOnlyPeople;
        }

        /*
         *   returns a Skeleton-List of Tracked People
         */
        public List<Skeleton> GetTrackedPeople()
        {
            List<Skeleton> trackedPeople = new List<Skeleton>();
            foreach (Skeleton skeleton in skeletonsList[0])
            {
                if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                {
                    trackedPeople.Add(skeleton);
                }
            }
            return trackedPeople;
        }

        /*
         *   returns a Skeleton-List of all recognized People
         */
        public List<Skeleton> GetAllRecognizedPeople()
        {
            List<Skeleton> recognizedPeople = new List<Skeleton>();
            foreach (Skeleton skeleton in skeletonsList[0])
            {
                if (skeleton.TrackingState == SkeletonTrackingState.PositionOnly || skeleton.TrackingState == SkeletonTrackingState.Tracked)
                {
                    recognizedPeople.Add(skeleton);
                }
            }
            return recognizedPeople;
        }

        /**
         *  returns a Skeleton-List of People which are currently passing the kinect
         */
        public List<Skeleton> GetPeoplePassKinect()
        {
            List<Skeleton> passingPeople = new List<Skeleton>();

            foreach (Skeleton currentSkeleton in skeletonsList[0])
            {
                if (currentSkeleton.TrackingState == SkeletonTrackingState.PositionOnly || currentSkeleton.TrackingState == SkeletonTrackingState.Tracked)
                {
                    foreach (Skeleton previousSkeleton in skeletonsList[1])
                    {
                        if (currentSkeleton.TrackingId == previousSkeleton.TrackingId)
                        {
                            if (Math.Abs(currentSkeleton.Position.X - previousSkeleton.Position.X) > passingKinectEpsilon)
                            {
                                passingPeople.Add(currentSkeleton);
                            }
                        }
                    }
                }
            }
            return passingPeople;
        }

        /*
         *  returns a Skeleton-List of People which are currently staying at the kinect
         */
        public List<Skeleton> GetPeopleStayAtKinect()
        {
            List<Skeleton> stayingPeople = new List<Skeleton>();
            foreach (Skeleton skeleton in GetAllRecognizedPeople())
            {
                if (!GetPeoplePassKinect().Contains(skeleton))
                {
                    stayingPeople.Add(skeleton);
                }
            }
            return stayingPeople;
        }

        /*
        *  returns a Skeleton-List of People which are currently looking at the kinect
        */
        public List<Skeleton> GetPeopleLookAtKinect()
        {
            // todo: Maxi algorithmus einbauen
            return null;
        }

        /**
         *  Setter-Method. Must be called each skeleton-frame
         */
        public Skeleton[] Skeletons
        {
            get { return skeletonsList[0]; }
            set
            {
                addSkeleton(value);
            }
        }

        // Store Skeleton from last 10 Frames in a List. Skeleton from newest Frame is always at index 0
        private void addSkeleton(Skeleton[] skeleton)
        {
            if (skeletonsList.Count >= storeElementsInList)
            {
                skeletonsList.RemoveAt(skeletonsList.Count - 1);
            }
            skeletonsList.Insert(0, skeleton);
        }
    }
}