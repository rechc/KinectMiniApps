using Microsoft.Kinect;
using System;

namespace PeopleDetector
{
    public struct SkeletonTimestamp
    {
        private Skeleton _skeleton;
        private DateTime _timestamp;

        public SkeletonTimestamp(Skeleton skeleton)
        {
            _skeleton = skeleton;
            _timestamp = DateTime.Now;
        }

        public Skeleton Skeleton { get { return _skeleton; } }

        public DateTime Timestamp { get { return _timestamp; } }
    }
}
