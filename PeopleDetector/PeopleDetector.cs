using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PeopleDetector
{
    public class PeopleDetector
    {
        private Dictionary<int, List<Skeleton>> skeletonsDict = new Dictionary<int, List<Skeleton>>();
        private const int maxNumberOfFramesInSkeletonList = 30;

        // walking People constants
        private const double walkingDistance = 0.22;
        private const int compareLastFrames = 16; // nice to have: duration of gesture in milis instead of frames

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
            return skeletonsDict.Values
                    .Select(list => list[0])
                    .Where(le => le.TrackingState == SkeletonTrackingState.PositionOnly).ToList();
        }

        /// <summary>
        /// Returns a Skeleton-List of Tracked People.
        /// </summary>
        public List<Skeleton> GetTrackedPeople()
        {
            return skeletonsDict.Values
                .Select(list => list[0])
                .Where(le => le.TrackingState == SkeletonTrackingState.Tracked).ToList();

            // alternativ mit LINQ

            //return (from le in skeletonsDict.Values
            //          where le[0].TrackingState == SkeletonTrackingState.Tracked
            //          select le[0]).ToList();
        }

        /// <summary>
        /// Returns a Skeleton-List of all recognized People.
        /// </summary>
        public List<Skeleton> GetAllPeople()
        {
            List<Skeleton> recognizedPeople = GetPositionOnlyPeople();
            recognizedPeople.AddRange(GetTrackedPeople());
            return recognizedPeople;
        }

        /// <summary>
        /// Returns a Skeleton-List of People which are currently walkting in front of the Kinect.
        /// </summary>
        public List<Skeleton> GetWalkingPeople()
        {
            List<Skeleton> walkingPeople = new List<Skeleton>();

            foreach (List<Skeleton> skeletonList in skeletonsDict.Values)
            {
                double distance = 0.0;

                for (int i = 0; i < (skeletonList.Count < compareLastFrames ? skeletonList.Count : compareLastFrames) - 1; i++)
                {
                    distance += Math.Abs(skeletonList[i].Position.X - skeletonList[i + 1].Position.X);
                }

                if (distance > walkingDistance)
                {
                    walkingPeople.Add(skeletonList[0]);
                }
            }
            return walkingPeople;
        }

        /// <summary>
        /// Returns a Skeleton-List of People which are currently staying at the Kinect.
        /// </summary>
        public List<Skeleton> GetStayingPeople()
        {
            List<Skeleton> stayingPeople = GetAllPeople();
            foreach (Skeleton skeleton in GetAllPeople())
            {
                if (GetWalkingPeople().Contains(skeleton))
                {
                    stayingPeople.Remove(skeleton);
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
        /// returns true if people with transfered TrackingId is currently walking
        /// </summary>
        /// <param name="TrackingId"></param>
        /// <returns></returns>
        public bool IsPeopleWalking(int TrackingId)
        {
            return GetWalkingPeople().Any(skel => skel.TrackingId == TrackingId);
        }

        /// <summary>
        /// returns true if people with transfered TrackingId is currently staying at the Kinect
        /// </summary>
        /// <param name="TrackingId"></param>
        /// <returns></returns>
        public bool IsPeopleStaying(int TrackingId)
        {
            return GetStayingPeople().Any(skel => skel.TrackingId == TrackingId);
        }

        /// <summary>
        /// returns true if people with transfered TrackingId is currently looking at the Kinect
        /// </summary>
        /// <param name="TrackingId"></param>
        /// <returns></returns>
        public bool IsPeopleLooking(int TrackingId)
        {
            return GetLookingPeople().Any(skel => skel.TrackingId == TrackingId);
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
        /// returns a Dictionary with
        /// Key: SkeletonTrackingId
        /// Value: A list of Skeletons of the last x frames
        /// </summary>
        public Dictionary<int, List<Skeleton>> SkeletonsDict
        {
            get
            {
                return skeletonsDict;
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