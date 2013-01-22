using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    class PeopleDetector
    {
        private Dictionary<int, List<Skeleton>> skeletonsDict = new Dictionary<int, List<Skeleton>>();
        private const int maxNumberOfFramesInSkeletonList = 30;

        // passing People constants
        private const double positionXEpsilon = 0.015;
        private const int compareLastFrames = 16;
        private const int numberOfFramesOK = 6;

        public PeopleDetector()
        {
        }

        public PeopleDetector(Skeleton[] skeletons)
        {
            AddSkeletonsToDictionary(skeletons);
        }

        /// <summary>
        /// Returns a Skeleton-List of PositionOnly People.
        /// </summary>
        public List<Skeleton> GetPositionOnlyPeople()
        {
            List<Skeleton> positionOnlyPeople = new List<Skeleton>();

            foreach (List<Skeleton> skeletonList in skeletonsDict.Values)
            {
                // check last frame in list
                if (skeletonList[0].TrackingState == SkeletonTrackingState.PositionOnly)
                {
                    positionOnlyPeople.Add(skeletonList[0]);
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
            foreach (List<Skeleton> skeletonList in skeletonsDict.Values)
            {
                // check last frame in list
                if (skeletonList[0].TrackingState == SkeletonTrackingState.Tracked)
                {
                    trackedPeople.Add(skeletonList[0]);
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
            recognizedPeople.AddRange(GetPositionOnlyPeople());
            recognizedPeople.AddRange(GetTrackedPeople());
            return recognizedPeople;
        }

        /// <summary>
        /// Returns a Skeleton-List of People which are currently passing the Kinect.
        /// </summary>
        public List<Skeleton> GetPassingPeople()
        {
            List<Skeleton> passingPeople = new List<Skeleton>();
            foreach (List<Skeleton> skeletonList in skeletonsDict.Values)
            {
                int passing = 0;
                for (int i = 0; i < (skeletonList.Count < compareLastFrames ? skeletonList.Count : compareLastFrames) - 1; i++)
                {
                    if (Math.Abs(skeletonList[i].Position.X - skeletonList[i + 1].Position.X) > positionXEpsilon)
                    {
                        passing++;
                    }
                }
                if (passing >= numberOfFramesOK)
                {
                    passingPeople.Add(skeletonList[0]);
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
            foreach (List<Skeleton> skeletonList in skeletonsDict.Values)
            {
                // newest Skeleton
                Skeleton skeleton = skeletonList[0];

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
            set
            {
                AddSkeletonsToDictionary(value);
            }
        }

        /// <summary>
        /// Store valid Skeletons with TrackingId as Key from last 30 Frames in a Dictionary. Newest Frame from a Skeleton is at Index 0
        /// </summary>
        private void AddSkeletonsToDictionary(Skeleton[] skeletons)
        {
            foreach (Skeleton skeleton in skeletons)
            {
                if (skeleton.TrackingState != SkeletonTrackingState.NotTracked)
                {
                    if (skeletonsDict.ContainsKey(skeleton.TrackingId))
                    {
                        List<Skeleton> skeletonList = skeletonsDict[skeleton.TrackingId];
                        if (skeletonList.Count >= maxNumberOfFramesInSkeletonList)
                        {
                            skeletonList.RemoveAt(skeletonList.Count - 1);
                        }
                        skeletonList.Insert(0, skeleton);
                        skeletonsDict[skeleton.TrackingId] = skeletonList;

                    }
                    else
                    {
                        List<Skeleton> skeletonList = new List<Skeleton>();
                        skeletonList.Add(skeleton);
                        skeletonsDict.Add(skeleton.TrackingId, skeletonList);
                    }
                }
            }

            // remove deprecated TrackingIds from Dictionary
            if (skeletonsDict.Count > 0)
            {
                List<int> removeSkeletonList = new List<int>();
                foreach (KeyValuePair<int, List<Skeleton>> dictEntry in skeletonsDict)
                {
                    Skeleton s = skeletons.FirstOrDefault((skeleton) => skeleton.TrackingId == dictEntry.Key);
                    if (s == null)
                    {
                        removeSkeletonList.Add(dictEntry.Key);
                    }
                }
                foreach (int index in removeSkeletonList)
                {
                    skeletonsDict.Remove(index);
                }
            }
        }
    }
}