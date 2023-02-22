using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.WindowsPhone.Imaging
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	internal struct VhdFooter
	{
		private enum VhdFooterFeatures : uint
		{
			VHD_FOOTER_FEATURES_NONE,
			VHD_FOOTER_FEATURES_TEMPORARY,
			VHD_FOOTER_FEATURES_RESERVED
		}

		private enum VhdFooterCreaterHostOS : uint
		{
			VHD_FOOTER_CREATOR_HOST_OS_WINDOWS = 1466511979u,
			VHD_FOOTER_CREATOR_HOST_OS_MACINTOSH = 1298228000u
		}

		private const string VHD_FOOTER_COOKIE = "conectix";

		private const uint VHD_FILE_FORMAT_VERSION = 65536u;

		private const uint VHD_FOOTER_CREATOR_APPLICATION = 1987278701u;

		private const uint VHD_FOOTER_CREATOR_VERSION = 65536u;

		private const int VHD_FOOTER_RESERVED_REGION_SIZE = 427;

		public ulong Cookie;

		public uint Features;

		public uint FileFormatVersion;

		public ulong DataOffset;

		public uint TimeStamp;

		public uint CreatorApplication;

		public uint CreatorVersion;

		public uint CreatorHostOs;

		public ulong OriginalSize;

		public ulong CurrentSize;

		private uint DriveGeometry;

		private uint DriveType;

		private uint CheckSum;

		private Guid UniqueId;

		private byte SavedState;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 427)]
		private byte[] Reserved;

		public VhdFooter(ulong vhdFileSize, VhdType vhdType, ulong dataOffset)
		{
			Cookie = BitConverter.ToUInt64(Encoding.ASCII.GetBytes("conectix"), 0);
			Features = 0u;
			FileFormatVersion = 65536u;
			DataOffset = dataOffset;
			TimeStamp = (uint)(DateTime.UtcNow - new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
			CreatorApplication = 1987278701u;
			CreatorVersion = 65536u;
			CreatorHostOs = 1466511979u;
			OriginalSize = vhdFileSize;
			CurrentSize = vhdFileSize;
			UniqueId = default(Guid);
			DriveType = (uint)vhdType;
			SavedState = 0;
			Reserved = new byte[427];
			DriveGeometry = GetDriveGeometry(vhdFileSize);
			CheckSum = 0u;
			CheckSum = VhdCommon.CalculateChecksum(ref this);
		}

		public VhdFooter(ulong vhdFileSize, VhdType type)
			: this(vhdFileSize, type, ulong.MaxValue)
		{
		}

		public void Write(FileStream writer)
		{
			ChangeByteOrder();
			try
			{
				writer.WriteStruct(ref this);
			}
			finally
			{
				ChangeByteOrder();
				writer.Flush();
			}
		}

		public static VhdFooter Read(FileStream reader)
		{
			VhdFooter result = reader.ReadStruct<VhdFooter>();
			result.ChangeByteOrder();
			return result;
		}

		private void ChangeByteOrder()
		{
			Features = VhdCommon.Swap32(Features);
			FileFormatVersion = VhdCommon.Swap32(FileFormatVersion);
			DataOffset = VhdCommon.Swap64(DataOffset);
			TimeStamp = VhdCommon.Swap32(TimeStamp);
			CreatorApplication = VhdCommon.Swap32(CreatorApplication);
			CreatorVersion = VhdCommon.Swap32(CreatorVersion);
			CreatorHostOs = VhdCommon.Swap32(CreatorHostOs);
			OriginalSize = VhdCommon.Swap64(OriginalSize);
			CurrentSize = VhdCommon.Swap64(CurrentSize);
			DriveGeometry = VhdCommon.Swap32(DriveGeometry);
			DriveType = VhdCommon.Swap32(DriveType);
			CheckSum = VhdCommon.Swap32(CheckSum);
		}

		private static uint GetDriveGeometry(ulong vhdFileSize)
		{
			uint num = (uint)(vhdFileSize / VhdCommon.VHDSectorSize);
			uint num2 = 0u;
			uint num3 = 0u;
			uint num4 = 0u;
			uint num5 = 0u;
			if (num > 267382800)
			{
				num = 267382800u;
			}
			if (num >= 66059280)
			{
				num5 = 255u;
				num4 = 16u;
				num3 = num / num5;
				num2 = num3 / num4;
			}
			else
			{
				num5 = 17u;
				num3 = num / num5;
				num4 = (num3 + 1023) / 1024u;
				if (num4 < 4)
				{
					num4 = 4u;
				}
				if (num3 >= num4 * 1024 || num4 > 16)
				{
					num5 = 31u;
					num4 = 16u;
					num3 = num / num5;
				}
				if (num3 >= num4 * 1024)
				{
					num5 = 63u;
					num4 = 16u;
					num3 = num / num5;
				}
				num2 = num3 / num4;
			}
			return 0u | (num2 << 16) | (num4 << 8) | num5;
		}
	}
}
