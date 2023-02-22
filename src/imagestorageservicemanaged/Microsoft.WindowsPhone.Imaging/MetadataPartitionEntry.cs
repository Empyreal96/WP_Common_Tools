using System;
using System.IO;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	internal class MetadataPartitionEntry
	{
		public static int PartitionNameLength = 36;

		public static int BytesPerGuid = 16;

		public Guid PartitionId { get; set; }

		public string Name { get; set; }

		public ulong DiskOffset { get; set; }

		public void ReadFromStream(BinaryReader reader)
		{
			DiskOffset = reader.ReadUInt64();
			byte[] bytes = reader.ReadBytes(PartitionNameLength * 2);
			string @string = Encoding.Unicode.GetString(bytes);
			Name = @string.Substring(0, @string.IndexOf('\0'));
			PartitionId = new Guid(reader.ReadBytes(BytesPerGuid));
		}

		public void LogInfo(IULogger logger, ushort indentLevel = 0)
		{
			new StringBuilder().Append(' ', indentLevel).ToString();
			logger.LogInfo(indentLevel + "Name        : {0}", Name);
			logger.LogInfo(indentLevel + "Partition Id: {0}", PartitionId);
			logger.LogInfo(indentLevel + "Disk Offset : {0}", DiskOffset);
		}
	}
}
