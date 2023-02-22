using System.IO;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	internal class MetadataPartitionHeader
	{
		public uint Signature { get; private set; }

		public uint MaxPartitionCount { get; private set; }

		public uint PartitionCount { get; private set; }

		public void ReadFromStream(BinaryReader reader)
		{
			Signature = reader.ReadUInt32();
			MaxPartitionCount = reader.ReadUInt32();
			PartitionCount = reader.ReadUInt32();
		}

		public void LogInfo(IULogger logger, ushort indentLevel = 0)
		{
			string text = new StringBuilder().Append(' ', indentLevel).ToString();
			logger.LogInfo(text + "Signature          : {0}", Signature);
			logger.LogInfo(text + "Max Partition Count: {0}", MaxPartitionCount);
			logger.LogInfo(text + "Partition Count    : {0}", PartitionCount);
		}
	}
}
