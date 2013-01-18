using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    class ActionDetector
    {
        private const int storeElementsInList = 10;
        private const double passingKinectEpsilon = 0.01;

        private List<Skeleton[]> skeletonsList = new List<Skeleton[]>();

        public ActionDetector()
        {
        }

        public ActionDetector(Skeleton[] skeletons)
        {
            AddSkeleton(skeletons);
        }

        /// <summary>
        /// Returns a Skeleton-List of PositionOnly People.
        /// </summary>
        public List<Skeleton> GetPositionOnlyPeople()
        {
            List<Skeleton> positionOnlyPeople = new List<Skeleton>();
            foreach (Skeleton skeleton in Skeletons)
            {
                if (skeleton.TrackingState == SkeletonTrackingState.PositionOnly)
                {
                    positionOnlyPeople.Add(skeleton);
                }
            }
            return positionOnlyPeople;
        }

        /// <summary>
        /// Returns a Skeleton-List of Tracked People.
        /// </summary>
        public List<Skeleton> GetTrackedPeople()
        {
            List<Skeleton> trackedPeople = new List<Skeleton>();
            foreach (Skeleton skeleton in Skeletons)
            {
                if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                {
                    trackedPeople.Add(skeleton);
                }
            }
            return trackedPeople;
        }

        /// <summary>
        /// Returns a Skeleton-List of all recognized People.
        /// </summary>
        public List<Skeleton> GetAllRecognizedPeople()
        {
            List<Skeleton> recognizedPeople = new List<Skeleton>();
            foreach (Skeleton skeleton in Skeletons)
            {
                if (skeleton.TrackingState != SkeletonTrackingState.NotTracked)
                {
                    recognizedPeople.Add(skeleton);
                }
            }
            return recognizedPeople;
        }

        /// <summary>
        /// Returns a Skeleton-List of People which are currently passing the Kinect.
        /// </summary>
        public List<Skeleton> GetPassingPeople()
        {
            List<Skeleton> passingPeople = new List<Skeleton>();

            foreach (Skeleton currentSkeleton in Skeletons)
            {
                if (currentSkeleton.TrackingState != SkeletonTrackingState.NotTracked)
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

        /// <summary>
        /// Returns a Skeleton-List of People which are currently staying at the Kinect.
        /// </summary>
        public List<Skeleton> GetStayingPeople()
        {
            List<Skeleton> stayingPeople = new List<Skeleton>();
            foreach (Skeleton skeleton in GetAllRecognizedPeople())
            {
                if (!GetPassingPeople().Contains(skeleton))
                {
                    stayingPeople.Add(skeleton);
                }
            }
            return stayingPeople;
        }

        /// <summary>
        /// Returns a List of Skeletons which are currently looking at the Kinect.
        /// </summary>
        public List<Skeleton> GetLookingPeople()
        {
            List<Skeleton> lookingPeople = new List<Skeleton>();
            foreach (Skeleton skeleton in Skeletons)
            {
                if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                    continue;

                Joint head = skeleton.Joints[JointType.Head];
                if (head.TrackingState == JointTrackingState.NotTracked)
                    continue;

                BoneOrientation headOrientation = skeleton.BoneOrientations[JointType.Head];

                // Defines a four-dimensional vector (x,y,z,w), which is used to efficiently rotate an
                // object about the (x, y, z) vector by the angle theta, where w = cos(theta/2).
                var rot = headOrientation.AbsoluteRotation.Quaternion;
                double angle = Math.Acos(rot.W) * 2 * 180 / Math.PI;
                //Console.WriteLine("[{0} {1} {2} {3}", rot.X, rot.Y, rot.Z, angle);
                // I don't care about the vector, for now.
                if (Math.Abs(180 - angle) < 30)
                {
                    lookingPeople.Add(skeleton);
                }
            }
            return lookingPeople;
        }

        /// <summary>
        /// returns true if people with transfered TrackingId is currently passing the Kinect
        /// </summary>
        /// <param name="TrackingId"></param>
        /// <returns></returns>
        public bool IsPeoplePassingTheKinect(int TrackingId)
        {
            foreach(Skeleton s in GetPassingPeople())
            {
                if (s.TrackingId == TrackingId)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// returns true if people with transfered TrackingId is currently staying at the Kinect
        /// </summary>
        /// <param name="TrackingId"></param>
        /// <returns></returns>
        public bool IsPeopleStayingAtKinect(int TrackingId)
        {
            foreach (Skeleton s in GetStayingPeople())
            {
                if (s.TrackingId == TrackingId)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// returns true if people with transfered TrackingId is currently looking at the Kinect
        /// </summary>
        /// <param name="TrackingId"></param>
        /// <returns></returns>
        public bool IsPeopleLookingAtKinect(int TrackingId)
        {
            foreach (Skeleton s in GetLookingPeople())
            {
                if (s.TrackingId == TrackingId)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// The Skeletons property must be set each skeleton frame.
        /// </summary>
        public Skeleton[] Skeletons
        {
            get { return skeletonsList[0]; }
            set
            {
                AddSkeleton(value);
            }
        }

        /// <summary>
        /// Store Skeleton from last 10 Frames in a List. Skeleton from newest Frame is always at index 0.
        /// </summary>
        private void AddSkeleton(Skeleton[] skeleton)
        {
            if (skeletonsList.Count >= storeElementsInList)
            {
                skeletonsList.RemoveAt(skeletonsList.Count - 1);
            }
            skeletonsList.Insert(0, skeleton);
        }
    }
}