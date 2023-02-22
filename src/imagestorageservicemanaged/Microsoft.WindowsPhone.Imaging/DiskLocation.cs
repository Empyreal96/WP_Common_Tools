using System.IO;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	internal class DiskLocation
	{
		public enum DiskAccessMethod : uint
		{
			DiskBegin = 0u,
			DiskEnd = 2u
		}

		public DiskAccessMethod AccessMethod { get; set; }

		public uint BlockIndex { get; set; }

		public static int SizeInBytes => 8;

		public DiskLocation()
		{
			BlockIndex = 0u;
			AccessMethod = DiskAccessMethod.DiskBegin;
		}

		public DiskLocation(uint blockIndex)
		{
			BlockIndex = blockIndex;
			AccessMethod = DiskAccessMethod.DiskBegin;
		}

		public DiskLocation(uint blockIndex, DiskAccessMethod accessMethod)
		{
			BlockIndex = blockIndex;
			AccessMethod = accessMethod;
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write((uint)AccessMethod);
			writer.Write(BlockIndex);
		}

		public void Read(BinaryReader reader)
		{
			AccessMethod = (DiskAccessMethod)reader.ReadUInt32();
			BlockIndex = reader.ReadUInt32();
		}

		public void LogInfo(IULogger logger, ushort indentLevel = 0)
		{
			string text = new StringBuilder().Append(' ', indentLevel).ToString();
			logger.LogInfo(text + "Access Method: {0}", AccessMethod);
			logger.LogInfo(text + "Block Index  : {0}", BlockIndex);
			logger.LogInfo("");
		}
	}
}
