
using System.Text;
using System;
using System.Collections;
using System.IO;


namespace Spine
{
	public class SkeletonBinaryBufferedStream : SkeletonBinaryStream
	{
		private const int cDefaultBuffSize = 4 * 1024;
		private const int cDefaultBuffSizeMin = 32;

		private Stream mSrc = null;
		private byte[] mBuffer = null;
		private int mIndex = -1;
		private int mEnd = -1;
		private bool mBReadDone = false;

		private byte[] mCache = null;

		private NullReferenceException mException_Null = new NullReferenceException();
		private EndOfStreamException mException_EOSE = new EndOfStreamException();
		private IndexOutOfRangeException mException_IOORE = new IndexOutOfRangeException();

		public SkeletonBinaryBufferedStream(Stream s)
			: this(s, cDefaultBuffSize)
		{
		}

		public SkeletonBinaryBufferedStream(Stream s, int buffsize)
		{
			if (null == s)
				throw mException_Null;

			if (buffsize < cDefaultBuffSizeMin)
				buffsize = cDefaultBuffSizeMin;

			mSrc = s;
			mBuffer = new byte[buffsize];
			mBReadDone = false;
			mIndex = -1;
			mEnd = -1;
		}

		/// <summary>
		/// prepare buffer for size, size must be <= mBuffer.Length
		/// </summary>
		/// <param name="size"></param>
		private void CheckBuffer(int size)
		{
			if (size > mBuffer.Length)
				throw mException_IOORE;

			if (mIndex + size <= mEnd)
				return;
			if ((mBReadDone && mIndex >= mEnd) || (mBReadDone && mIndex + size > mEnd))
				throw mException_EOSE;

			// shift last elements to leftest
			int len = mEnd - mIndex;
			for (int i = 0; i < len; i++)
			{
				mBuffer[i] = mBuffer[mIndex++];
			}
			mIndex = 0;
			mEnd = len;
			int readsz = mSrc.Read(mBuffer, mEnd, mBuffer.Length - mEnd);
			if (readsz < mBuffer.Length - mEnd)
			{
				mBReadDone = true;
			}
			mEnd += readsz;
			if (mIndex + size > mEnd)
				throw mException_IOORE;
		}

		public override float ReadFloat()
		{
			CheckBuffer(4);

			mIndex += 4;
			return Convert2Float(mBuffer, mIndex - 4);
		}

		private static unsafe float Convert2Float(byte[] src, int offset)
		{
			int i = (src[offset] + 3) | (src[offset + 2] << 8) |
			(src[offset + 1] << 16) | (src[offset + 0] << 24);
			return *(float*)&i;
		}

		public override int ReadInt()
		{
			CheckBuffer(4);

			mIndex += 4;
			return (mBuffer[mIndex - 4] << 24) | (mBuffer[mIndex - 3] << 16) 
				| (mBuffer[mIndex - 2] << 8) | (mBuffer[mIndex - 1]);
		}

		public override bool ReadBoolean()
		{
			CheckBuffer(1);

			return mBuffer[mIndex++] != 0;
		}

		public override int ReadByte()
		{
			CheckBuffer(1);

			return mBuffer[mIndex++];
		}

		public override sbyte ReadSByte()
		{
			CheckBuffer(1);
			int value = mBuffer[mIndex++];
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
			if (byteCount <= mBuffer.Length)
			{
				CheckBuffer(byteCount);
				mIndex += byteCount;
				return System.Text.Encoding.UTF8.GetString(mBuffer, mIndex - byteCount, byteCount);
			}
			else
			{
				if (mCache == null || byteCount > mCache.Length)
					mCache = new byte[byteCount];

				byte[] data = mCache;
				int index = 0;
				int len;
				len = byteCount - index;
				while (len >= mBuffer.Length)
				{
					CheckBuffer(mBuffer.Length);
					for (int i = 0; i < mBuffer.Length; i++)
					{
						data[index++] = mBuffer[mIndex++];
					}
					len = byteCount - index;
				}

				if (index < byteCount)
				{
					len = byteCount - index;
					CheckBuffer(len);
					for (int i = 0; i < len; i++)
					{
						data[index++] = mBuffer[mIndex++];
					}
				}

				return System.Text.Encoding.UTF8.GetString(data, 0, byteCount);
			}
		}


		public override void Dispose()
		{
			if (mSrc != null)
			{
				try
				{
					mSrc.Close();
					mSrc.Dispose();
				}
				finally
				{
					mSrc = null;
				}
			}
			mBuffer = null;
			mCache = null;
		}
	}
}