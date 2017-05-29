

using System.Collections;
using System;
using System.Text;


namespace Spine
{
	public abstract class SkeletonBinaryStream : IDisposable
	{

		public abstract float ReadFloat();

		public abstract int ReadInt();

		public abstract bool ReadBoolean();

		public abstract int ReadByte();

		public abstract sbyte ReadSByte();

		public abstract int ReadVarint(bool optimizePositive);

		public abstract string ReadString();


		public abstract void Dispose();
	}

}
