using Microsoft.Kinect;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace PeopleDetector
{
    public struct SkeletonTimestamp
    {
        private Skeleton _skeleton;
        private DateTime _timestamp;

        public SkeletonTimestamp(Skeleton skeleton)
        {
            _skeleton = Clone(skeleton);
            _timestamp = DateTime.Now;
        }

        public static Skeleton Clone(Skeleton skOrigin)
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();

            bf.Serialize(ms, skOrigin);

            ms.Position = 0;
            object obj = bf.Deserialize(ms);
            ms.Close();

            return obj as Skeleton;
        }

        public Skeleton Skeleton { get { return _skeleton; } }

        public DateTime Timestamp { get { return _timestamp; } }
    }
}
