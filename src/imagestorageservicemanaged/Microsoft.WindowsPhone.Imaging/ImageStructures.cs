using System;
using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.Imaging
{
	public class ImageStructures
	{
		[Flags]
		public enum DiskAttributes : ulong
		{
			Offline = 1uL,
			ReadOnly = 2uL
		}

		[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
		public struct PARTITION_ENTRY
		{
			[FieldOffset(0)]
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 36)]
			private string name;

			[FieldOffset(72)]
			private ulong sectorCount;

			[FieldOffset(80)]
			private uint alignmentSizeInBytes;

			[FieldOffset(84)]
			private uint clusterSize;

			[FieldOffset(88)]
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			private string fileSystem;

			[FieldOffset(152)]
			private Guid id;

			[FieldOffset(168)]
			private Guid type;

			[FieldOffset(184)]
			private ulong flags;

			[FieldOffset(152)]
			private byte mbrFlags;

			[FieldOffset(153)]
			private byte mbrType;

			public string PartitionName
			{
				get
				{
					return name;
				}
				set
				{
					name = value;
				}
			}

			[CLSCompliant(false)]
			public ulong SectorCount
			{
				get
				{
					return sectorCount;
				}
				set
				{
					sectorCount = value;
				}
			}

			[CLSCompliant(false)]
			public uint AlignmentSizeInBytes
			{
				get
				{
					return alignmentSizeInBytes;
				}
				set
				{
					alignmentSizeInBytes = value;
				}
			}

			[CLSCompliant(false)]
			public uint ClusterSize
			{
				get
				{
					return clusterSize;
				}
				set
				{
					clusterSize = value;
				}
			}

			public string FileSystem
			{
				get
				{
					return fileSystem;
				}
				set
				{
					fileSystem = value;
				}
			}

			public Guid PartitionId
			{
				get
				{
					return id;
				}
				set
				{
					id = value;
				}
			}

			public Guid PartitionType
			{
				get
				{
					return type;
				}
				set
				{
					type = value;
				}
			}

			[CLSCompliant(false)]
			public ulong PartitionFlags
			{
				get
				{
					return flags;
				}
				set
				{
					flags = value;
				}
			}

			public byte MBRFlags
			{
				get
				{
					return mbrFlags;
				}
				set
				{
					mbrFlags = value;
				}
			}

			public byte MBRType
			{
				get
				{
					return mbrType;
				}
				set
				{
					mbrType = value;
				}
			}
		}

		[StructLayout(LayoutKind.Explicit)]
		[CLSCompliant(false)]
		public struct STORE_ID
		{
			[FieldOffset(0)]
			private uint storeType;

			[FieldOffset(4)]
			private Guid storeId_GPT;

			[FieldOffset(4)]
			private uint storeId_MBR;

			public uint StoreType
			{
				get
				{
					return storeType;
				}
				set
				{
					storeType = value;
				}
			}

			public Guid StoreId_GPT
			{
				get
				{
					return storeId_GPT;
				}
				set
				{
					storeId_GPT = value;
				}
			}

			public uint StoreId_MBR
			{
				get
				{
					return storeId_MBR;
				}
				set
				{
					storeId_MBR = value;
				}
			}

			public bool IsEmpty
			{
				get
				{
					if (storeId_GPT == Guid.Empty)
					{
						return storeId_MBR == 0;
					}
					return false;
				}
			}
		}

		[StructLayout(LayoutKind.Explicit)]
		[CLSCompliant(false)]
		public struct PartitionAttributes
		{
			[FieldOffset(0)]
			public ulong gptAttributes;

			[FieldOffset(0)]
			public byte mbrAttributes;
		}

		[StructLayout(LayoutKind.Explicit)]
		public struct PartitionType
		{
			[FieldOffset(0)]
			public Guid gptType;

			[FieldOffset(0)]
			public byte mbrType;
		}

		public struct SetDiskAttributes
		{
			public uint Version;

			public byte Persist;

			private byte dummy1;

			private byte dummy2;

			private byte dummy3;

			public DiskAttributes Attributes;

			public DiskAttributes AttributesMask;

			private Guid Reserved;
		}
	}
}
