using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	public class ImageStoreHeader
	{
		public static readonly int PlatformIdSizeInBytes = 192;

		private byte[] _platformId = new byte[PlatformIdSizeInBytes];

		private ushort _numberOfStores = 1;

		private ushort _storeIndex = 1;

		private ulong _storePayloadSize;

		private ushort _devicePathLength;

		private byte[] _devicePath;

		[StructVersion(Version = 1)]
		public FullFlashUpdateType UpdateType { get; set; }

		[StructVersion(Version = 1)]
		public ushort MajorVersion { get; set; }

		[StructVersion(Version = 1)]
		public ushort MinorVersion { get; set; }

		[StructVersion(Version = 1)]
		public ushort FullFlashMajorVersion { get; set; }

		[StructVersion(Version = 1)]
		public ushort FullFlashMinorVersion { get; set; }

		[StructVersion(Version = 1)]
		public byte[] PlatformIdentifier
		{
			get
			{
				return _platformId;
			}
			set
			{
				_platformId = value;
			}
		}

		[StructVersion(Version = 1)]
		public uint BytesPerBlock { get; set; }

		[StructVersion(Version = 1)]
		public uint StoreDataEntryCount { get; set; }

		[StructVersion(Version = 1)]
		public uint StoreDataSizeInBytes { get; set; }

		[StructVersion(Version = 1)]
		public uint ValidationEntryCount { get; set; }

		[StructVersion(Version = 1)]
		public uint ValidationDataSizeInBytes { get; set; }

		[StructVersion(Version = 1)]
		public uint InitialPartitionTableBlockIndex { get; set; }

		[StructVersion(Version = 1)]
		public uint InitialPartitionTableBlockCount { get; set; }

		[StructVersion(Version = 1)]
		public uint FlashOnlyPartitionTableBlockIndex { get; set; }

		[StructVersion(Version = 1)]
		public uint FlashOnlyPartitionTableBlockCount { get; set; }

		[StructVersion(Version = 1)]
		public uint FinalPartitionTableBlockIndex { get; set; }

		[StructVersion(Version = 1)]
		public uint FinalPartitionTableBlockCount { get; set; }

		[StructVersion(Version = 2)]
		public ushort NumberOfStores
		{
			get
			{
				if (MajorVersion < 2)
				{
					throw new NotImplementedException("NumberOfStores");
				}
				return _numberOfStores;
			}
			set
			{
				_numberOfStores = value;
			}
		}

		[StructVersion(Version = 2)]
		public ushort StoreIndex
		{
			get
			{
				if (MajorVersion < 2)
				{
					throw new NotImplementedException("StoreIndex");
				}
				return _storeIndex;
			}
			set
			{
				_storeIndex = value;
			}
		}

		[StructVersion(Version = 2)]
		public ulong StorePayloadSize
		{
			get
			{
				if (MajorVersion < 2)
				{
					throw new NotImplementedException("StorePayloadSize");
				}
				return _storePayloadSize;
			}
			set
			{
				_storePayloadSize = value;
			}
		}

		[StructVersion(Version = 2)]
		public ushort DevicePathLength
		{
			get
			{
				if (MajorVersion < 2)
				{
					throw new NotImplementedException("DevicePathLength");
				}
				return _devicePathLength;
			}
			set
			{
				_devicePathLength = value;
			}
		}

		[StructVersion(Version = 2)]
		public byte[] DevicePath
		{
			get
			{
				if (MajorVersion < 2)
				{
					throw new NotImplementedException("DevicePath");
				}
				return _devicePath;
			}
			set
			{
				_devicePath = value;
			}
		}

		private void SetFullFlashVersion(FullFlashUpdateImage fullFlashImage)
		{
			Regex regex = new Regex("(?<MajorVersion>\\d+)\\.(?<MinorVersion>\\d+)");
			if (!regex.IsMatch(fullFlashImage.Version))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: The full flash update version isn't valid. '{fullFlashImage.Version}'");
			}
			Match match = regex.Match(fullFlashImage.Version);
			try
			{
				ushort fullFlashMajorVersion = ushort.Parse(match.Groups["MajorVersion"].Value);
				ushort fullFlashMinorVersion = ushort.Parse(match.Groups["MinorVersion"].Value);
				FullFlashMajorVersion = fullFlashMajorVersion;
				FullFlashMinorVersion = fullFlashMinorVersion;
			}
			catch (Exception innerException)
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: The full flash image version number is invalid. '{fullFlashImage.Version}'", innerException);
			}
		}

		public void Initialize(FullFlashUpdateType updateType, uint blockSizeInBytes, FullFlashUpdateImage fullFlashImage)
		{
			BytesPerBlock = blockSizeInBytes;
			UpdateType = updateType;
			MajorVersion = 1;
			int num = 0;
			foreach (string devicePlatformID in fullFlashImage.DevicePlatformIDs)
			{
				byte[] bytes = Encoding.ASCII.GetBytes(devicePlatformID);
				int num2 = bytes.Length + 1;
				if (num + num2 > PlatformIdSizeInBytes - 1)
				{
					throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: The platform ID group is too large.");
				}
				bytes.CopyTo(PlatformIdentifier, num);
				num += num2;
			}
			SetFullFlashVersion(fullFlashImage);
		}

		public void Initialize2(FullFlashUpdateType updateType, uint blockSizeInBytes, FullFlashUpdateImage fullFlashImage, ushort numberOfStores, ushort storeIndex, string devicePath)
		{
			Initialize(updateType, blockSizeInBytes, fullFlashImage);
			MajorVersion = 2;
			NumberOfStores = numberOfStores;
			StoreIndex = storeIndex;
			DevicePathLength = (ushort)devicePath.Length;
			UnicodeEncoding unicodeEncoding = new UnicodeEncoding();
			DevicePath = unicodeEncoding.GetBytes(devicePath.ToCharArray());
		}

		public void WriteToStream(Stream stream)
		{
			BinaryWriter binaryWriter = new BinaryWriter(stream);
			long position = stream.Position;
			binaryWriter.Write((uint)UpdateType);
			binaryWriter.Write(MajorVersion);
			binaryWriter.Write(MinorVersion);
			binaryWriter.Write(FullFlashMajorVersion);
			binaryWriter.Write(FullFlashMinorVersion);
			binaryWriter.Write(PlatformIdentifier);
			binaryWriter.Write(BytesPerBlock);
			binaryWriter.Write(StoreDataEntryCount);
			binaryWriter.Write(StoreDataSizeInBytes);
			binaryWriter.Write(ValidationEntryCount);
			binaryWriter.Write(ValidationDataSizeInBytes);
			binaryWriter.Write(InitialPartitionTableBlockIndex);
			binaryWriter.Write(InitialPartitionTableBlockCount);
			binaryWriter.Write(FlashOnlyPartitionTableBlockIndex);
			binaryWriter.Write(FlashOnlyPartitionTableBlockCount);
			binaryWriter.Write(FinalPartitionTableBlockIndex);
			binaryWriter.Write(FinalPartitionTableBlockCount);
			if (MajorVersion >= 2)
			{
				binaryWriter.Write(NumberOfStores);
				binaryWriter.Write(StoreIndex);
				binaryWriter.Write(StorePayloadSize);
				binaryWriter.Write(DevicePathLength);
				binaryWriter.Write(DevicePath);
			}
			binaryWriter = null;
		}

		public static ImageStoreHeader ReadFromStream(Stream stream)
		{
			BinaryReader binaryReader = new BinaryReader(stream);
			long position = stream.Position;
			ImageStoreHeader imageStoreHeader = new ImageStoreHeader();
			imageStoreHeader.UpdateType = (FullFlashUpdateType)binaryReader.ReadUInt32();
			imageStoreHeader.MajorVersion = binaryReader.ReadUInt16();
			imageStoreHeader.MinorVersion = binaryReader.ReadUInt16();
			imageStoreHeader.FullFlashMajorVersion = binaryReader.ReadUInt16();
			imageStoreHeader.FullFlashMinorVersion = binaryReader.ReadUInt16();
			imageStoreHeader.PlatformIdentifier = binaryReader.ReadBytes(PlatformIdSizeInBytes);
			imageStoreHeader.BytesPerBlock = binaryReader.ReadUInt32();
			imageStoreHeader.StoreDataEntryCount = binaryReader.ReadUInt32();
			imageStoreHeader.StoreDataSizeInBytes = binaryReader.ReadUInt32();
			imageStoreHeader.ValidationEntryCount = binaryReader.ReadUInt32();
			imageStoreHeader.ValidationDataSizeInBytes = binaryReader.ReadUInt32();
			imageStoreHeader.InitialPartitionTableBlockIndex = binaryReader.ReadUInt32();
			imageStoreHeader.InitialPartitionTableBlockCount = binaryReader.ReadUInt32();
			imageStoreHeader.FlashOnlyPartitionTableBlockIndex = binaryReader.ReadUInt32();
			imageStoreHeader.FlashOnlyPartitionTableBlockCount = binaryReader.ReadUInt32();
			imageStoreHeader.FinalPartitionTableBlockIndex = binaryReader.ReadUInt32();
			imageStoreHeader.FinalPartitionTableBlockCount = binaryReader.ReadUInt32();
			if (imageStoreHeader.MajorVersion >= 2)
			{
				imageStoreHeader.NumberOfStores = binaryReader.ReadUInt16();
				imageStoreHeader.StoreIndex = binaryReader.ReadUInt16();
				imageStoreHeader.StorePayloadSize = binaryReader.ReadUInt64();
				imageStoreHeader.DevicePathLength = binaryReader.ReadUInt16();
				imageStoreHeader.DevicePath = binaryReader.ReadBytes(imageStoreHeader.DevicePathLength * 2);
			}
			binaryReader = null;
			return imageStoreHeader;
		}

		public void LogInfo(IULogger logger)
		{
			LogInfo(logger, 0);
		}

		public void LogInfo(IULogger logger, int indentLevel)
		{
			string @string = Encoding.ASCII.GetString(PlatformIdentifier);
			string text = new StringBuilder().Append(' ', indentLevel).ToString();
			logger.LogInfo(text + "Image Store Payload Header");
			indentLevel = checked(indentLevel + 2);
			text = new StringBuilder().Append(' ', indentLevel).ToString();
			logger.LogInfo(text + "UpdateType                       : {0}", UpdateType);
			logger.LogInfo(text + "MajorVersion                     : 0x{0:x}", MajorVersion);
			logger.LogInfo(text + "MinorVersion                     : 0x{0:x}", MinorVersion);
			logger.LogInfo(text + "FullFlashMajorVersion            : 0x{0:x}", FullFlashMajorVersion);
			logger.LogInfo(text + "FullFlashMinorVersion            : 0x{0:x}", FullFlashMinorVersion);
			logger.LogInfo(text + "PlatformIdentifier               : {0}", @string.Substring(0, @string.IndexOf('\0')));
			logger.LogInfo(text + "BytesPerBlock                    : 0x{0:x}", BytesPerBlock);
			logger.LogInfo(text + "StoreDataEntryCount              : 0x{0:x}", StoreDataEntryCount);
			logger.LogInfo(text + "StoreDataSizeInBytes             : 0x{0:x}", StoreDataSizeInBytes);
			logger.LogInfo(text + "ValidationEntryCount             : 0x{0:x}", ValidationEntryCount);
			logger.LogInfo(text + "ValidationDataSizeInBytes        : 0x{0:x}", ValidationDataSizeInBytes);
			logger.LogInfo(text + "InitialPartitionTableBlockIndex  : 0x{0:x}", InitialPartitionTableBlockIndex);
			logger.LogInfo(text + "InitialPartitionTableBlockCount  : 0x{0:x}", InitialPartitionTableBlockCount);
			logger.LogInfo(text + "FlashOnlyPartitionTableBlockIndex: 0x{0:x}", FlashOnlyPartitionTableBlockIndex);
			logger.LogInfo(text + "FlashOnlyPartitionTableBlockCount: 0x{0:x}", FlashOnlyPartitionTableBlockCount);
			logger.LogInfo(text + "FinalPartitionTableBlockIndex    : 0x{0:x}", FinalPartitionTableBlockIndex);
			logger.LogInfo(text + "FinalPartitionTableBlockCount    : 0x{0:x}", FinalPartitionTableBlockCount);
			if (MajorVersion >= 2)
			{
				string string2 = Encoding.ASCII.GetString(DevicePath);
				logger.LogInfo(text + "NumberOfStores                   : 0x{0:x}", NumberOfStores);
				logger.LogInfo(text + "StoreIndex                       : 0x{0:x}", StoreIndex);
				logger.LogInfo(text + "StorePayloadSize                 : 0x{0:x}", StorePayloadSize);
				logger.LogInfo(text + "DevicePathLength                 : 0x{0:x}", DevicePathLength);
				logger.LogInfo(text + "DevicePath                       : {0}", string2.Substring(0, string2.IndexOf('\0')));
			}
		}

		public int GetStructureSize()
		{
			int num = 0;
			PropertyInfo[] properties = typeof(ImageStoreHeader).GetProperties();
			foreach (PropertyInfo propertyInfo in properties)
			{
				MethodInfo[] accessors = propertyInfo.GetAccessors();
				foreach (MethodInfo methodInfo in accessors)
				{
					if (methodInfo.IsStatic || propertyInfo.PropertyType.IsArray)
					{
						break;
					}
					if (methodInfo.IsPublic)
					{
						StructVersionAttribute[] array = propertyInfo.GetCustomAttributes(typeof(StructVersionAttribute), false) as StructVersionAttribute[];
						if (array != null && (MajorVersion >= 2 || !IsVersion2Field(array)))
						{
							num = ((!propertyInfo.PropertyType.IsEnum) ? (num + Marshal.SizeOf(propertyInfo.PropertyType)) : (num + Marshal.SizeOf(Enum.GetUnderlyingType(propertyInfo.PropertyType))));
						}
						break;
					}
				}
			}
			num += PlatformIdSizeInBytes;
			if (MajorVersion >= 2)
			{
				num += DevicePathLength;
			}
			return num;
		}

		private bool IsVersion2Field(StructVersionAttribute[] structVersions)
		{
			return structVersions[0].Version == 2;
		}
	}
}
