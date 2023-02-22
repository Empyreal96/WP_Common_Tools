using System.IO;

namespace Microsoft.WindowsPhone.Imaging
{
	internal class ValidationEntry
	{
		private byte[] _compareData;

		public uint SectorIndex { get; set; }

		public int SectorOffset { get; set; }

		public int ByteCount { get; set; }

		public void SetCompareData(byte[] data)
		{
			_compareData = data;
		}

		public byte[] GetCompareData()
		{
			return _compareData;
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(SectorIndex);
			writer.Write(SectorOffset);
			writer.Write(ByteCount);
			writer.Write(GetCompareData());
		}

		public void Read(BinaryReader reader)
		{
			SectorIndex = reader.ReadUInt32();
			SectorOffset = reader.ReadInt32();
			ByteCount = reader.ReadInt32();
			SetCompareData(reader.ReadBytes(ByteCount));
		}
	}
}
