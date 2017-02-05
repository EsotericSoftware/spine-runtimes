

using System;
using System.Collections;
using System.Text;
using System.IO;

namespace Spine
{
	public class SkeletonBinaryArrayReaderStream : SkeletonBinaryStream
	{

		private byte[] mSrc = null;
		private int mIndex = -1;
		private int mEnd = -1;

		private NullReferenceException mException_Null = new NullReferenceException();
		private EndOfStreamException mException_EOSE = new EndOfStreamException();
		private IndexOutOfRangeException mException_IOORE = new IndexOutOfRangeException();

		public SkeletonBinaryArrayReaderStream(byte[] src) : this(src, 0, src.Length)
		{
		}

		public SkeletonBinaryArrayReaderStream(byte[] src, int offset, int len)
		{
			if (src == null)
				throw mException_Null;
			if (src.Length - offset < len)
				throw mException_IOORE;

			mSrc = src;
			mIndex = offset;
			mEnd = offset + len;
		}

		public override void Dispose()
		{
			mSrc = null;
			mIndex = -1;
			mEnd = -1;
		}

		#region read func
		public override float ReadFloat()
		{
			if (mIndex + 4 > mEnd)
				throw mException_EOSE;

			mIndex += 4;
			return Convert2Float(mSrc, mIndex - 4);
		}

		[System.Security.SecuritySafeCritical]
		private static unsafe float Convert2Float(byte[] src, int offset)
		{
			int i = (src[offset + 3]) | (src[offset + 2] << 8) |
			(src[offset + 1] << 16) | (src[offset + 0] << 24);
			return *(float*)&i;
		}

		public override int ReadInt()
		{
			if (mIndex + 4 > mEnd)
				throw mException_EOSE;

			mIndex += 4;
			return (mSrc[mIndex - 4] << 24) | (mSrc[mIndex - 3] << 16) | (mSrc[mIndex - 2] << 8)
				| (mSrc[mIndex - 1]);
		}

		public override bool ReadBoolean()
		{
			if (mIndex + 1 > mEnd)
				throw mException_EOSE;

			return mSrc[mIndex++] != 0;
		}

		public override int ReadByte()
		{
			if (mIndex + 1 > mEnd)
				throw mException_EOSE;

			return mSrc[mIndex++];
		}

		public override sbyte ReadSByte()
		{
			if (mIndex + 1 > mEnd)
				throw mException_EOSE;
			int value = mSrc[mIndex++];
			if (value == -1) throw mException_EOSE;
			return (sbyte)value;
		}

		public override int ReadVarint(bool optimizePositive)
		{
			int b = ReadByte();
			int result = b & 0x7F;
			if ((b & 0x80) != 0)
			{
				b = ReadByte();
				result |= (b & 0x7F) << 7;
				if ((b & 0x80) != 0)
				{
					b = ReadByte();
					result |= (b & 0x7F) << 14;
					if ((b & 0x80) != 0)
					{
						b = ReadByte();
						result |= (b & 0x7F) << 21;
						if ((b & 0x80) != 0) result |= (ReadByte() & 0x7F) << 28;
					}
				}
			}
			return optimizePositive ? result : ((result >> 1) ^ -(result & 1));
		}

		public override string ReadString()
		{
			int byteCount = ReadVarint(true);
			switch (byteCount)
			{
				case 0:
					return null;
				case 1:
					return "";
			}
			byteCount--;
			if (mIndex + byteCount > mEnd)
				throw mException_EOSE;
			mIndex += byteCount;
			return System.Text.Encoding.UTF8.GetString(mSrc, mIndex - byteCount, byteCount);
		}
		#endregion
	}
}
