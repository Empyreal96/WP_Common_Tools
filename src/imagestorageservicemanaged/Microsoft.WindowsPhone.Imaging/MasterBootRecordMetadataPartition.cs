using System.Collections.Generic;
using System.IO;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	internal class MasterBootRecordMetadataPartition
	{
		public static byte PartitonType = ImageConstants.MBR_METADATA_PARTITION_TYPE;

		public static uint HeaderSignature = 524289u;

		private IULogger _logger;

		private MetadataPartitionHeader _header = new MetadataPartitionHeader();

		private List<MetadataPartitionEntry> _entries = new List<MetadataPartitionEntry>();

		public List<MetadataPartitionEntry> Entries => _entries;

		public MetadataPartitionHeader Header => _header;

		private MasterBootRecordMetadataPartition()
		{
		}

		public MasterBootRecordMetadataPartition(IULogger logger)
		{
			_logger = logger;
		}

		public void ReadFromStream(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			_header.ReadFromStream(reader);
			for (uint num = 0u; num < _header.PartitionCount; num++)
			{
				MetadataPartitionEntry metadataPartitionEntry = new MetadataPartitionEntry();
				metadataPartitionEntry.ReadFromStream(reader);
				_entries.Add(metadataPartitionEntry);
			}
			reader = null;
		}

		public void LogInfo(ushort indentLevel = 0)
		{
			_header.LogInfo(_logger, (ushort)(indentLevel + 2));
			foreach (MetadataPartitionEntry entry in _entries)
			{
				entry.LogInfo(_logger, (ushort)(indentLevel + 2));
			}
		}
	}
}
