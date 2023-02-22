using System;

namespace Microsoft.WindowsPhone.Imaging
{
	[CLSCompliant(false)]
	public sealed class ImageConstants
	{
		public const uint ONE_MEGABYTE = 1048576u;

		public const uint ONE_GIGABYTE = 1073741824u;

		public static readonly uint PartitionTypeMbr = FullFlashUpdateImage.PartitionTypeMbr;

		public static readonly uint PartitionTypeGpt = FullFlashUpdateImage.PartitionTypeGpt;

		public static readonly uint MaxPartitionNameSizeInWideChars = 36u;

		public static readonly uint DeviceNameSize = 40u;

		public static readonly uint MaxVolumeNameSize = 260u;

		public static readonly uint DefaultVirtualHardDiskSectorSize = 512u;

		public static readonly uint RegistryKeyMaxNameSize = 255u;

		public static readonly uint RegistryValueMaxNameSize = 16383u;

		public static readonly Guid PARTITION_SYSTEM_GUID = new Guid("{c12a7328-f81f-11d2-ba4b-00a0c93ec93b}");

		public static readonly Guid PARTITION_BASIC_DATA_GUID = new Guid("{ebd0a0a2-b9e5-4433-87c0-68b6b72699c7}");

		public static readonly Guid PARTITION_MSFT_RECOVERY_GUID = new Guid("{de94bba4-06d1-4d40-a16a-bfd50179d6ac}");

		public static readonly Guid SYSTEM_STORE_GUID = new Guid("{AE420040-13DD-41F2-AE7F-0DC35854C8D7}");

		public static readonly ulong GPT_ATTRIBUTE_NO_DRIVE_LETTER = 9223372036854775808uL;

		public static readonly uint SYSTEM_STORE_SIGNATURE = 2923561024u;

		public static readonly uint MINIMUM_NTFS_PARTITION_SIZE = 7340032u;

		public static readonly uint MINIMUM_FAT_PARTITION_SIZE = 7340032u;

		public static readonly uint MINIMUM_FAT32_PARTITION_SIZE = 34603008u;

		public static readonly uint MINIMUM_PARTITION_SIZE = 16384u;

		public static readonly uint PARTITION_TABLE_METADATA_SIZE = 1048576u;

		public static readonly uint MAX_PRIMARY_PARTITIONS = 4u;

		public static readonly uint MBR_METADATA_PARTITION_SIZE = 1048576u;

		public static readonly byte MBR_METADATA_PARTITION_TYPE = 112;

		public static readonly string MBR_METADATA_PARTITION_NAME = "MBR_META";

		public static readonly uint PAYLOAD_BLOCK_SIZE = 131072u;

		public static readonly string MAINOS_PARTITION_NAME = "MainOS";

		public static readonly Guid MAINOS_PARTITION_ID = new Guid("{A76B8CE2-0187-4C13-8FCA-8651C9B0620A}");

		public static readonly string DATA_PARTITION_NAME = "Data";

		public static readonly string SYSTEM_PARTITION_NAME = "EFIESP";

		public static readonly string DPP_PARTITION_NAME = "DPP";

		public static readonly Guid SYSTEM_PARTITION_ID = new Guid("{8183040A-8B44-4592-92F7-C6D9EE0560F7}");

		public static readonly string BCD_FILE_PATH = "boot\\bcd";

		public static readonly string EFI_BCD_FILE_PATH = "efi\\microsoft\\boot\\bcd";

		public static readonly string MMOS_PARTITION_NAME = "MMOS";

		public static readonly Guid MMOS_PARTITION_ID = new Guid("{27A47557-8243-4C8E-9D30-846844C29C52}");

		public const string VHDMutex = "Global\\VHDMutex_{585b0806-2d3b-4226-b259-9c8d3b237d5c}";
	}
}
