using System;
using System.IO;

namespace FFUComponents
{
	internal class PacketConstructor : IDisposable
	{
		private static readonly int cbDefaultData = 262144;

		private static readonly int cbMaxData = 8388608;

		private int packetNumber;

		public static long DefaultPacketDataSize => cbDefaultData;

		public static long MaxPacketDataSize => cbMaxData;

		public Stream DataStream { internal get; set; }

		public long PacketDataSize { internal get; set; }

		public long Position => DataStream.Position;

		public long Length => DataStream.Length;

		public long RemainingData => DataStream.Length - DataStream.Position;

		public PacketConstructor()
		{
			packetNumber = 0;
			PacketDataSize = cbDefaultData;
		}

		public void Reset()
		{
			DataStream.Seek(0L, SeekOrigin.Begin);
			packetNumber = 0;
		}

		public unsafe byte[] GetNextPacket(bool optimize)
		{
			byte[] array = new byte[PacketDataSize + 12];
			Array.Clear(array, 0, array.Length);
			int value = DataStream.Read(array, 0, (int)PacketDataSize);
			int num = (int)PacketDataSize;
			byte[] bytes = BitConverter.GetBytes(value);
			bytes.CopyTo(array, num);
			num += bytes.Length;
			bytes = BitConverter.GetBytes(packetNumber++);
			bytes.CopyTo(array, num);
			num += bytes.Length;
			uint value2 = 0u;
			if (!optimize)
			{
				fixed (byte* lpBuffer = array)
				{
					value2 = Crc32.GetChecksum(0u, lpBuffer, (uint)(array.Length - 4));
				}
			}
			bytes = BitConverter.GetBytes(value2);
			bytes.CopyTo(array, num);
			return array;
		}

		public byte[] GetZeroLengthPacket()
		{
			DataStream.Seek(0L, SeekOrigin.End);
			return GetNextPacket(false);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		private void Dispose(bool fDisposing)
		{
			if (fDisposing && DataStream != null)
			{
				DataStream.Dispose();
				DataStream = null;
			}
		}
	}
}
