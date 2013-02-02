using Microsoft.Kinect;
using System;

namespace PeopleDetector
{
    public class SkeletonTimestamp
    {
        private Skeleton skeleton;

        public SkeletonTimestamp(Skeleton skeleton) : this(skeleton,DateTime.Now)
        {
        }

        public SkeletonTimestamp(Skeleton skeleton, DateTime timestamp)
        {
            this.Skeleton = skeleton;
            this.Timestamp = DateTime.Now;
        }

        public Skeleton Skeleton
        {
            get
            {
                return skeleton;
            }
            set
            {
                skeleton = value;
                Timestamp = DateTime.Now;
            }
        }

        public DateTime Timestamp { get; private set; }
    }
}