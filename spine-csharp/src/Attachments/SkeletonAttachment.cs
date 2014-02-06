using System;

namespace Spine
{
    public class SkeletonAttachment : Attachment
    {
        private Skeleton skeleton;

        private Skeleton Skeleton
        {
            get { return skeleton; }
            set { skeleton = value; }
        }

        public SkeletonAttachment(String name) : base(name)
        {
        }
    }
}