using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PeopleDetector
{
    public class PeoplePositionDetector
    {
        private Dictionary<int, List<SkeletonTimestamp>> _skeletonsDict = new Dictionary<int, List<SkeletonTimestamp>>();
        private const int _maxNumberOfFramesInSkeletonList = 20;

        // Define the walking gesture
        private const double _walkingDistance = 0.22;
        private const int _walkingDuration = 500; // Time in miliseconds

        /// <summary>
        /// Returns a List of PositionOnly Skeletons.
        /// </summary>
        public List<Skeleton> GetPositionOnlyPeople()
        {
            return _skeletonsDict.Values
                    .Select(list => list[0].Skeleton)
                    .Where(le => le.TrackingState == SkeletonTrackingState.PositionOnly).ToList();
        }

        /// <summary>
        /// Returns a List of Tracked Skeletons.
        /// </summary>
        public List<Skeleton> GetTrackedPeople()
        {
            return _skeletonsDict.Values
                .Select(list => list[0].Skeleton)
                .Where(le => le.TrackingState == SkeletonTrackingState.Tracked).ToList();

            // alternativ mit LINQ

            //return (from le in skeletonsDict.Values
            //          where le[0].TrackingState == SkeletonTrackingState.Tracked
            //          select le[0]).ToList();
        }

        /// <summary>
        /// Returns a List of all recognized Skeletons.
        /// </summary>
        public List<Skeleton> GetAllPeople()
        {
            List<Skeleton> recognizedPeople = GetPositionOnlyPeople();
            recognizedPeople.AddRange(GetTrackedPeople());
            return recognizedPeople;
        }


        /// <summary>
        /// Returns a List of Skeletons which are currently walking in front of the Kinect.
        /// </summary>
        public List<Skeleton> GetWalkingPeople()
        {
            List<Skeleton> walkingPeople = new List<Skeleton>();
            foreach (List<SkeletonTimestamp> skeletonList in _skeletonsDict.Values)
            {
                if (IsWalking(skeletonList))
                {
                    walkingPeople.Add(skeletonList[0].Skeleton);
                }
            }
            return walkingPeople;
        }

        private bool IsWalking(List<SkeletonTimestamp> skeletonList)
        {
            double currentDistance = 0.0;
            int currentDuration = 0;
            DateTime now = DateTime.Now;
            for (int i = 0; i < skeletonList.Count - 1 && currentDuration < _walkingDuration; i++)
            {
                currentDistance += Math.Abs(skeletonList[i].Skeleton.Position.X - skeletonList[i + 1].Skeleton.Position.X);
                currentDuration = (now - skeletonList[i].Timestamp).Milliseconds;
            }

            return currentDistance > _walkingDistance;
        }

        /// <summary>
        /// Returns a List of Skeletons which are currently standing still.
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
            foreach (List<SkeletonTimestamp> skeletonList in _skeletonsDict.Values)
            {
                // newest Skeleton
                Skeleton skeleton = skeletonList[0].Skeleton;

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
        /// Returns true if the given id is tracked.
        /// </summary>
        public bool IsPeopleTracked(int trackingId)
        {
            return GetTrackedPeople().Exists((skel) => skel.TrackingId == trackingId);
        }

        /// <summary>
        /// Returns true if the given id is currently walking.
        /// </summary>
        public bool IsPeopleWalking(int trackingId)
        {
            return GetWalkingPeople().Any(skel => skel.TrackingId == trackingId);
        }

        /// <summary>
        /// Returns true if the given id is currently standing still.
        /// </summary>
        public bool IsPeopleStaying(int trackingId)
        {
            return GetStayingPeople().Any(skel => skel.TrackingId == trackingId);
        }

        /// <summary>
        /// Returns true if the given id is currently looking at the Kinect.
        /// </summary>
        public bool IsPeopleLooking(int trackingId)
        {
            return GetLookingPeople().Any(skel => skel.TrackingId == trackingId);
        }

        /// <summary>
        /// returns a Dictionary with
        /// Key: SkeletonTrackingId
        /// Value: A list of Skeletons of the last x frames
        /// </summary>
        public Dictionary<int, List<SkeletonTimestamp>> SkeletonsDict
        {
            get
            {
                return _skeletonsDict;
            }
        }

        /// <summary>
        /// Store valid Skeletons with TrackingId as Key from last 20 Frames in
        /// a Dictionary. Newest Frame from a Skeleton is at Index 0.
        /// </summary>
        public void TrackSkeletons(Skeleton[] skeletons)
        {
            if (skeletons!=null){
                foreach (Skeleton skeleton in skeletons)
                {
                    if (skeleton.TrackingState != SkeletonTrackingState.NotTracked)
                    {
                        if (_skeletonsDict.ContainsKey(skeleton.TrackingId))
                        {
                            List<SkeletonTimestamp> skeletonList = _skeletonsDict[skeleton.TrackingId];
                            if (skeletonList.Count >= _maxNumberOfFramesInSkeletonList)
                            {
                                skeletonList.RemoveAt(skeletonList.Count - 1);
                            }
                            skeletonList.Insert(0, new SkeletonTimestamp(skeleton));
                        }
                        else
                        {
                            List<SkeletonTimestamp> skeletonList = new List<SkeletonTimestamp>();
                            skeletonList.Add(new SkeletonTimestamp(skeleton));
                            _skeletonsDict.Add(skeleton.TrackingId, skeletonList);
                        }
                    }
                }
            }

            // remove deprecated TrackingIds from Dictionary
            if (_skeletonsDict.Count > 0)
            {
                List<int> removeSkeletonList = new List<int>();
                foreach (KeyValuePair<int, List<SkeletonTimestamp>> dictEntry in _skeletonsDict)
                {
                    Skeleton s = skeletons.FirstOrDefault((skeleton) => skeleton.TrackingId == dictEntry.Key);
                    if (s == null)
                    {
                        removeSkeletonList.Add(dictEntry.Key);
                    }
                }
                foreach (int index in removeSkeletonList)
                {
                    _skeletonsDict.Remove(index);
                }
            }
        }
    }
}
