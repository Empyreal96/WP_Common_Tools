using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using Microsoft.Win32.SafeHandles;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	[CLSCompliant(false)]
	public class ImageStorage
	{
		private class PartitionInfo
		{
			public byte MbrType { get; set; }

			public Guid GptType { get; set; }

			public byte MbrAttributes { get; set; }

			public ulong GptAttributes { get; set; }
		}

		private ImageStorageManager _manager;

		private SafeFileHandle _storeHandle;

		private FullFlashUpdateImage _image;

		private FullFlashUpdateImage.FullFlashUpdateStore _store;

		private NativeServiceHandle _service;

		private ImageStructures.STORE_ID _storeId;

		private IULogger _logger;

		private LogFunction _logError;

		private LogFunction _logWarning;

		private LogFunction _logInfo;

		private LogFunction _logDebug;

		private List<string> _pathsToRemove;

		private bool _isMainOSStorage;

		public IntPtr StoreHandle => _storeHandle.DangerousGetHandle();

		public SafeFileHandle SafeStoreHandle => _storeHandle;

		public bool IsMainOSStorage => _isMainOSStorage;

		public ImageStructures.STORE_ID StoreId
		{
			get
			{
				return _storeId;
			}
			set
			{
				_storeId = value;
			}
		}

		public uint VirtualHardDiskSectorSize { get; set; }

		public string VirtualDiskFilePath { get; private set; }

		public IULogger Logger => _logger;

		internal FullFlashUpdateImage Image => _image;

		internal FullFlashUpdateImage.FullFlashUpdateStore Store => _store;

		internal NativeServiceHandle ServiceHandle => _service;

		internal uint BytesPerBlock => ImageConstants.PAYLOAD_BLOCK_SIZE;

		internal bool ReadOnlyVirtualDisk { get; private set; }

		private uint ImageSectorCount { get; set; }

		private bool PostProcessVHD { get; set; }

		public ImageStorage(IULogger logger, ImageStorageManager manager)
		{
			_logger = logger;
			_manager = manager;
			_logError = LogError;
			_service = new NativeServiceHandle(_logError);
			_storeId = default(ImageStructures.STORE_ID);
			_pathsToRemove = new List<string>();
			_isMainOSStorage = true;
			PrepareLogging();
		}

		public ImageStorage(IULogger logger, ImageStorageManager manager, ImageStructures.STORE_ID storeId)
		{
			_logger = logger;
			_manager = manager;
			_logError = LogError;
			_service = new NativeServiceHandle(_logError);
			_storeId = storeId;
			_pathsToRemove = new List<string>();
			_isMainOSStorage = true;
			PrepareLogging();
		}

		public void Cleanup()
		{
			CleanupTemporaryPaths();
			_image = null;
			_store = null;
			_storeHandle = null;
		}

		public void LogError(string message)
		{
			Logger.LogError("{0}", message);
		}

		public void LogWarning(string message)
		{
			Logger.LogWarning("{0}", message);
		}

		public void LogInfo(string message)
		{
			Logger.LogInfo("{0}", message);
		}

		public void LogDebug(string message)
		{
			Logger.LogDebug("{0}", message);
		}

		public void SetFullFlashImage(FullFlashUpdateImage image)
		{
			_image = image;
		}

		public void CreateJunction(string sourceName, string targetPartition, string targetPath)
		{
			CreateJunction(sourceName, targetPartition, targetPath, false);
		}

		public void CreateJunction(string sourceName, string targetPartition, string targetPath, bool useWellKnownGuids)
		{
			NativeImaging.CreateJunction(ServiceHandle, StoreId, sourceName, targetPartition, targetPath, useWellKnownGuids);
		}

		public void SetFullFlashUpdateStore(FullFlashUpdateImage.FullFlashUpdateStore store)
		{
			if (_image != null)
			{
				throw new ImageStorageException("ImageStorage already has a FullFlashUpdateImage.");
			}
			if (_store != null)
			{
				throw new ImageStorageException("ImageStorage already has a FullFlashUpdateStore.");
			}
			_image = store.Image;
			_store = store;
		}

		public void DetachVirtualHardDisk(bool deleteFile)
		{
			NativeImaging.DismountVirtualHardDisk(_service, _storeId, true, deleteFile, true);
		}

		public void CreateVirtualHardDiskFromStore(FullFlashUpdateImage.FullFlashUpdateStore store, string imagePath, uint partitionStyle, bool preparePartitions, ImageStructures.STORE_ID[] storeIds)
		{
			_image = store.Image;
			_store = store;
			_isMainOSStorage = store.IsMainOSStore;
			if (string.IsNullOrEmpty(imagePath))
			{
				imagePath = CreateBackingVhdFileName(store.SectorSize);
			}
			VirtualDiskFilePath = imagePath;
			List<FullFlashUpdateImage.FullFlashUpdatePartition> partitions = store.Partitions;
			List<string> list = new List<string>();
			if (store.MinSectorCount != 0)
			{
				ImageSectorCount = store.MinSectorCount;
			}
			else
			{
				ImageSectorCount = (uint)(10737418240uL / (ulong)store.SectorCount);
			}
			int num = partitions.Count;
			if (partitionStyle == ImageConstants.PartitionTypeMbr)
			{
				num++;
			}
			uint num2 = 1u;
			if (ImageConstants.MINIMUM_PARTITION_SIZE > store.SectorSize)
			{
				num2 = ImageConstants.MINIMUM_PARTITION_SIZE / store.SectorSize;
			}
			foreach (FullFlashUpdateImage.FullFlashUpdatePartition partition in store.Partitions)
			{
				if (partition.TotalSectors < num2 && !partition.UseAllSpace)
				{
					partition.TotalSectors = num2;
				}
				if (string.Compare(partition.PrimaryPartition, partition.Name, true, CultureInfo.InvariantCulture) != 0)
				{
					if (list.Contains(partition.Name))
					{
						throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: A duplicate partition cannot be used as a primary partition for another duplicate partition.");
					}
					if (!list.Contains(partition.PrimaryPartition))
					{
						list.Add(partition.PrimaryPartition);
					}
					list.Add(partition.Name);
				}
			}
			int num3 = -1;
			ImageStructures.PARTITION_ENTRY[] array = new ImageStructures.PARTITION_ENTRY[num];
			for (int i = 0; i < array.Length; i++)
			{
				int num4 = i;
				if (i == array.Length - 1 && partitionStyle == ImageConstants.PartitionTypeMbr)
				{
					if (num3 == -1)
					{
						array[i].PartitionName = ImageConstants.MBR_METADATA_PARTITION_NAME;
						array[i].FileSystem = "";
						array[i].SectorCount = ImageConstants.MBR_METADATA_PARTITION_SIZE / store.SectorSize;
						array[i].MBRFlags = 0;
						array[i].MBRType = ImageConstants.MBR_METADATA_PARTITION_TYPE;
						array[i].AlignmentSizeInBytes = store.SectorSize;
						continue;
					}
					num4 = num3;
				}
				FullFlashUpdateImage.FullFlashUpdatePartition fullFlashUpdatePartition = partitions[num4];
				if (fullFlashUpdatePartition.UseAllSpace && partitionStyle == ImageConstants.PartitionTypeMbr && num4 != num3)
				{
					if (num3 != -1)
					{
						throw new ImageStorageException($"There are two partition set to use all remaining space on disk: {store.Partitions[num4].Name} and {store.Partitions[num3].Name}");
					}
					num3 = num4;
					array[i].PartitionName = ImageConstants.MBR_METADATA_PARTITION_NAME;
					array[i].FileSystem = "";
					array[i].SectorCount = ImageConstants.MBR_METADATA_PARTITION_SIZE / store.SectorSize;
					array[i].MBRFlags = 0;
					array[i].MBRType = ImageConstants.MBR_METADATA_PARTITION_TYPE;
					array[i].AlignmentSizeInBytes = store.SectorSize;
					continue;
				}
				ValidatePartitionStrings(fullFlashUpdatePartition);
				uint alignmentInBytes = store.SectorSize;
				if (fullFlashUpdatePartition.ByteAlignment != 0)
				{
					if (fullFlashUpdatePartition.ByteAlignment < store.SectorSize)
					{
						throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: The alignment for partition '{fullFlashUpdatePartition.Name}' is smaller than the sector size: 0x{fullFlashUpdatePartition.ByteAlignment:x}/0x{store.SectorSize:x}.");
					}
					alignmentInBytes = fullFlashUpdatePartition.ByteAlignment;
				}
				else if (_image.DefaultPartitionAlignmentInBytes > store.SectorSize)
				{
					alignmentInBytes = _image.DefaultPartitionAlignmentInBytes;
				}
				PreparePartitionEntry(ref array[i], store, fullFlashUpdatePartition, partitionStyle, alignmentInBytes);
			}
			ulong num5 = 0uL;
			if (array[array.Length - 1].SectorCount == uint.MaxValue)
			{
				for (int j = 0; j < array.Length; j++)
				{
					if (partitionStyle == ImageConstants.PartitionTypeMbr && num3 != -1 && j >= 3)
					{
						num5 += 65536u / store.SectorSize;
					}
					uint num6 = array[j].AlignmentSizeInBytes / store.SectorSize;
					if (num5 == 0L || num5 % num6 != 0L)
					{
						num5 += num6 - num5 % num6;
					}
					if (j == array.Length - 1)
					{
						break;
					}
					num5 += array[j].SectorCount;
				}
				uint num7 = 0u;
				if (partitionStyle == ImageConstants.PartitionTypeGpt)
				{
					num7 += 2 * ImageConstants.PARTITION_TABLE_METADATA_SIZE;
				}
				else
				{
					num7 = ImageConstants.PAYLOAD_BLOCK_SIZE;
					if (array.Length > 3)
					{
						num7 += (uint)((array.Length - 3) * (int)ImageConstants.PAYLOAD_BLOCK_SIZE);
					}
				}
				num5 += num7 / store.SectorSize;
			}
			if (num5 > ImageSectorCount)
			{
				throw new ImageStorageException("The store's minSectorCount is less than the count of sectors in its partitions.");
			}
			for (int k = 0; k < array.Length; k++)
			{
				if (array[k].SectorCount == uint.MaxValue)
				{
					array[k].SectorCount = ImageSectorCount - num5;
					ulong num8 = array[k].SectorCount * store.SectorSize;
					if (num8 % ImageConstants.PAYLOAD_BLOCK_SIZE == 0L)
					{
						break;
					}
					ulong num9 = num8;
					num8 = num9 - num9 % ImageConstants.PAYLOAD_BLOCK_SIZE;
					array[k].SectorCount = num8 / store.SectorSize;
					if (array[k].SectorCount != 0L)
					{
						break;
					}
					throw new ImageStorageException("The store's minSectorCount is less than the count of sectors in its partitions.");
				}
			}
			try
			{
				ulong maxSizeInBytes = (ulong)ImageSectorCount * (ulong)store.SectorSize;
				CleanupAllMountedDisks();
				IntPtr preexistingHandle = NativeImaging.InitializeVirtualHardDisk(_service, VirtualDiskFilePath, maxSizeInBytes, ref _storeId, array, preparePartitions, store.IsMainOSStore, VirtualHardDiskSectorSize, storeIds);
				_storeHandle = new SafeFileHandle(preexistingHandle, true);
			}
			catch (ImageStorageException)
			{
				throw;
			}
			catch (Exception innerException)
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: Unable to create the VHD.", innerException);
			}
			if (store.IsMainOSStore)
			{
				_pathsToRemove.Add(GetPartitionPath(ImageConstants.MAINOS_PARTITION_NAME));
			}
			PostProcessVHD = true;
		}

		public void MountExistingVirtualHardDisk(string imagePath, bool readOnly)
		{
			_storeId = default(ImageStructures.STORE_ID);
			ReadOnlyVirtualDisk = readOnly;
			VirtualDiskFilePath = imagePath;
			try
			{
				using (DynamicHardDisk dynamicHardDisk = new DynamicHardDisk(imagePath))
				{
					using (VirtualDiskStream stream = new VirtualDiskStream(dynamicHardDisk))
					{
						MasterBootRecord masterBootRecord = new MasterBootRecord(Logger, (int)dynamicHardDisk.SectorSize, _manager.IsDesktopImage);
						masterBootRecord.ReadFromStream(stream, MasterBootRecord.MbrParseType.Normal);
						if (masterBootRecord.IsValidProtectiveMbr())
						{
							GuidPartitionTable guidPartitionTable = new GuidPartitionTable((int)dynamicHardDisk.SectorSize, Logger);
							guidPartitionTable.ReadFromStream(stream, true, _manager.IsDesktopImage);
							bool flag = false;
							foreach (GuidPartitionTableEntry entry in guidPartitionTable.Entries)
							{
								if (string.Compare(entry.PartitionName, ImageConstants.MAINOS_PARTITION_NAME, true, CultureInfo.InvariantCulture) == 0)
								{
									flag = true;
								}
							}
							if (!flag)
							{
								throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: The given VHD does not contain the partition '{ImageConstants.MAINOS_PARTITION_NAME}'.");
							}
							_storeId.StoreType = ImageConstants.PartitionTypeGpt;
							_storeId.StoreId_GPT = guidPartitionTable.Header.DiskId;
						}
						else
						{
							if (masterBootRecord.FindPartitionByName(ImageConstants.MAINOS_PARTITION_NAME) == null)
							{
								throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: The given VHD does not contain the partition '{ImageConstants.MAINOS_PARTITION_NAME}'.");
							}
							_storeId.StoreType = ImageConstants.PartitionTypeMbr;
							_storeId.StoreId_MBR = masterBootRecord.DiskSignature;
						}
						masterBootRecord = null;
					}
				}
			}
			catch (ImageStorageException)
			{
			}
			NativeImaging.DismountVirtualHardDisk(ServiceHandle, StoreId, false, false);
			IntPtr preexistingHandle = NativeImaging.OpenVirtualHardDisk(_service, imagePath, out _storeId, readOnly);
			_storeHandle = new SafeFileHandle(preexistingHandle, true);
			string empty = string.Empty;
			try
			{
				empty = BuildPaths.GetImagingTempPath(Path.GetTempPath());
				empty += ".mnt\\";
				Directory.CreateDirectory(empty);
				_pathsToRemove.Add(empty);
			}
			catch (SecurityException innerException)
			{
				throw new ImageStorageException("Unable to retrieve a temporary path.", innerException);
			}
			NativeImaging.AddAccessPath(_service, _storeId, ImageConstants.MAINOS_PARTITION_NAME, empty);
		}

		public void DismountVirtualHardDisk(bool skipPostProcessing)
		{
			DismountVirtualHardDisk(skipPostProcessing, false);
		}

		public void DismountVirtualHardDisk(bool skipPostProcessing, bool normalizeDiskSignature)
		{
			int tickCount = Environment.TickCount;
			if (_storeHandle == null)
			{
				Logger.DebugLogger("{0}: This function was called when no image is mounted.", MethodBase.GetCurrentMethod().Name);
				return;
			}
			if (!ReadOnlyVirtualDisk && !skipPostProcessing)
			{
				Logger.LogInfo("{0}:[{1}] Enabling USN journal on partition {2}.", MethodBase.GetCurrentMethod().Name, (double)(Environment.TickCount - tickCount) / 1000.0, ImageConstants.MAINOS_PARTITION_NAME);
				NativeImaging.CreateUsnJournal(_service, StoreId, ImageConstants.MAINOS_PARTITION_NAME);
				Logger.LogInfo("{0}:[{1}] Enabling USN journal on partition {2}.", MethodBase.GetCurrentMethod().Name, (double)(Environment.TickCount - tickCount) / 1000.0, ImageConstants.DATA_PARTITION_NAME);
				NativeImaging.CreateUsnJournal(_service, StoreId, ImageConstants.DATA_PARTITION_NAME);
				if (StoreId.StoreType == ImageConstants.PartitionTypeMbr)
				{
					Logger.LogInfo("{0}:[{1}] Updating the BCD.", MethodBase.GetCurrentMethod().Name, (double)(Environment.TickCount - tickCount) / 1000.0);
					UpdateBootConfigurationDatabase(ImageConstants.BCD_FILE_PATH, ImageConstants.SYSTEM_STORE_SIGNATURE);
				}
				if (normalizeDiskSignature)
				{
					NativeImaging.WriteMountManagerRegistry2(_service, _storeId, true);
					NativeImaging.NormalizeVolumeMountPoints(_service, _storeId, GetPartitionPath(ImageConstants.MAINOS_PARTITION_NAME));
				}
			}
			_storeHandle.Close();
			_storeHandle = null;
			NativeImaging.DismountVirtualHardDisk(_service, _storeId, false, false, true);
			if (!ReadOnlyVirtualDisk && !skipPostProcessing && PostProcessVHD && _image != null)
			{
				string fileSystem = null;
				string bootPartitionName = null;
				foreach (FullFlashUpdateImage.FullFlashUpdatePartition partition in _store.Partitions)
				{
					if (partition.Bootable)
					{
						fileSystem = partition.FileSystem;
						bootPartitionName = partition.Name;
						break;
					}
				}
				Logger.LogInfo("{0}:[{1}] Making the virtual disk bootable.", MethodBase.GetCurrentMethod().Name, (double)(Environment.TickCount - tickCount) / 1000.0);
				PostProcessVirtualHardDisk(VirtualDiskFilePath, Logger, bootPartitionName, fileSystem, normalizeDiskSignature);
			}
			Logger.LogInfo("{0}:[{1}] Cleaning up temporary paths.", MethodBase.GetCurrentMethod().Name, (double)(Environment.TickCount - tickCount) / 1000.0);
			CleanupTemporaryPaths();
			_image = null;
			_store = null;
			_storeHandle = null;
			_storeId = default(ImageStructures.STORE_ID);
			int num = Environment.TickCount - tickCount;
			Logger.LogInfo("Storage Service: Dismounting the image in {0:F1} seconds.", (double)num / 1000.0);
		}

		public string GetPartitionPath(string partitionName)
		{
			StringBuilder stringBuilder = new StringBuilder("path", 1024);
			if (_storeId.StoreId_GPT == Guid.Empty && _storeId.StoreId_MBR == 0)
			{
				NativeImaging.GetPartitionPathNoContext(partitionName, stringBuilder, (uint)stringBuilder.Capacity);
			}
			else
			{
				NativeImaging.GetPartitionPath(_service, _storeId, partitionName, stringBuilder, (uint)stringBuilder.Capacity);
			}
			return stringBuilder.ToString();
		}

		public static string GetPartitionPathNoContext(string partitionName)
		{
			StringBuilder stringBuilder = new StringBuilder("path", 1024);
			NativeImaging.GetPartitionPathNoContext(partitionName, stringBuilder, (uint)stringBuilder.Capacity);
			return stringBuilder.ToString();
		}

		public string GetDiskName()
		{
			return NativeImaging.GetDiskName(ServiceHandle, StoreId);
		}

		public void SetDiskAttributes(ImageStructures.DiskAttributes attributes, ImageStructures.DiskAttributes attributesMask, bool persist)
		{
			ImageStructures.SetDiskAttributes setDiskAttributes = default(ImageStructures.SetDiskAttributes);
			setDiskAttributes.Version = (uint)Marshal.SizeOf((object)setDiskAttributes);
			setDiskAttributes.Persist = (byte)(persist ? 1 : 0);
			setDiskAttributes.AttributesMask = attributesMask;
			setDiskAttributes.Attributes = attributes;
			NativeImaging.SetDiskAttributes(_service, StoreHandle, setDiskAttributes);
		}

		public void FormatPartition(string partitionName, string fileSystem, uint cbClusterSize)
		{
			NativeImaging.FormatPartition(_service, _storeId, partitionName, fileSystem, cbClusterSize);
		}

		public void AttachWOFToVolume(string partitionName)
		{
			StringBuilder stringBuilder = new StringBuilder(1024);
			NativeImaging.GetPartitionPath(_service, _storeId, partitionName, stringBuilder, (uint)stringBuilder.Capacity);
			NativeImaging.AttachWOFToVolume(_service, stringBuilder.ToString());
		}

		public Guid GetPartitionTypeGpt(string partitionName)
		{
			return NativeImaging.GetPartitionType(_service, _storeId, partitionName).gptType;
		}

		public byte GetPartitionTypeMbr(string partitionName)
		{
			return NativeImaging.GetPartitionType(_service, _storeId, partitionName).mbrType;
		}

		public void SetPartitionType(string partitionName, Guid partitionType)
		{
			ImageStructures.PartitionType partitionType2 = default(ImageStructures.PartitionType);
			partitionType2.gptType = partitionType;
			NativeImaging.SetPartitionType(_service, _storeId, partitionName, partitionType2);
		}

		public void SetPartitionType(string partitionName, byte partitionType)
		{
			ImageStructures.PartitionType partitionType2 = default(ImageStructures.PartitionType);
			partitionType2.mbrType = partitionType;
			NativeImaging.SetPartitionType(_service, _storeId, partitionName, partitionType2);
		}

		public ulong GetPartitionAttributesGpt(string partitionName)
		{
			if (_storeId.StoreType != ImageConstants.PartitionTypeGpt)
			{
				throw new ImageStorageException("UInt64 GetPartitionAttributes(string) can only be called on an GPT style disk.");
			}
			return NativeImaging.GetPartitionAttributes(_service, _storeId, partitionName).gptAttributes;
		}

		public byte GetPartitionAttributesMbr(string partitionName)
		{
			if (_storeId.StoreType != ImageConstants.PartitionTypeMbr)
			{
				throw new ImageStorageException("byte GetPartitionAttributes(string) can only be called on an MBR style disk.");
			}
			return NativeImaging.GetPartitionAttributes(_service, _storeId, partitionName).mbrAttributes;
		}

		public void SetPartitionAttributes(string partitionName, ulong attributes)
		{
			ImageStructures.PartitionAttributes attributes2 = default(ImageStructures.PartitionAttributes);
			attributes2.gptAttributes = attributes;
			NativeImaging.SetPartitionAttributes(_service, _storeId, partitionName, attributes2);
		}

		public ulong GetPartitionSize(string partitionName)
		{
			return NativeImaging.GetPartitionSize(_service, _storeId, partitionName);
		}

		public string GetPartitionFileSystem(string partitionName)
		{
			return NativeImaging.GetPartitionFileSystem(_service, StoreId, partitionName);
		}

		public bool IsPartitionTargeted(string partition)
		{
			return _manager.IsPartitionTargeted(partition);
		}

		public bool IsBackingFileVhdx()
		{
			return VirtualDiskFilePath.EndsWith(".vhdx", true, CultureInfo.InvariantCulture);
		}

		public ulong GetFreeBytesOnVolume(string partitionName)
		{
			return NativeImaging.GetFreeBytesOnVolume(_service, _storeId, partitionName);
		}

		public SafeFileHandle OpenVolumeHandle(string partitionName)
		{
			return NativeImaging.OpenVolumeHandle(_service, _storeId, partitionName, FileAccess.ReadWrite, FileShare.ReadWrite);
		}

		public SafeFileHandle OpenVolumeHandle(string partitionName, FileAccess access, FileShare share)
		{
			return NativeImaging.OpenVolumeHandle(_service, _storeId, partitionName, access, share);
		}

		public void WaitForVolume(string strVolumeName)
		{
			WaitForVolume(strVolumeName, int.MaxValue);
		}

		public void WaitForVolume(string strVolumeName, int timeout)
		{
			NativeImaging.WaitForVolumeArrival(_service, _storeId, strVolumeName, timeout);
		}

		public void LockAndDismountVolume(string partitionName)
		{
			LockAndDismountVolume(partitionName, false);
		}

		public void LockAndDismountVolume(string partitionName, bool forceDismount)
		{
			using (SafeVolumeHandle safeVolumeHandle = new SafeVolumeHandle(this, partitionName))
			{
				NativeImaging.LockAndDismountVolume(_service, safeVolumeHandle.VolumeHandle, forceDismount);
			}
		}

		public void UnlockVolume(string partitionName)
		{
			using (SafeVolumeHandle safeVolumeHandle = new SafeVolumeHandle(this, partitionName))
			{
				NativeImaging.UnlockVolume(_service, safeVolumeHandle.VolumeHandle);
			}
		}

		public bool PartitionIsMountedRaw(string partitionName)
		{
			string partitionFileSystem = GetPartitionFileSystem(partitionName);
			if (string.Compare("RAW", partitionFileSystem, true, CultureInfo.InvariantCulture) == 0)
			{
				return true;
			}
			return false;
		}

		public void CreateUsnJournal(string partitionName)
		{
			NativeImaging.CreateUsnJournal(_service, StoreId, partitionName);
		}

		internal void MountFullFlashImageStore(FullFlashUpdateImage.FullFlashUpdateStore store, PayloadReader payloadReader, StorePayload payload, bool randomizeGptIds)
		{
			VirtualDiskFilePath = CreateBackingVhdFileName(store.SectorSize);
			_image = store.Image;
			_store = store;
			int tickCount = Environment.TickCount;
			if (store.SectorSize > BytesPerBlock)
			{
				throw new ImageStorageException(string.Format("The sector size (0x{0:x} bytes) is greater than the image block size (0x{1x} bytes)", store.SectorSize, BytesPerBlock));
			}
			if (BytesPerBlock % store.SectorSize != 0)
			{
				throw new ImageStorageException(string.Format("The block size (0x{0:x} bytes) is not a mulitple of the sector size (0x{1x} bytes)", BytesPerBlock, store.SectorSize));
			}
			ulong num = store.SectorCount;
			if (_image == null)
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: The full flash update image has not been set.");
			}
			if (num == 0L)
			{
				num = 10737418240uL / (ulong)store.SectorSize;
			}
			_storeId.StoreType = store.Image.ImageStyle;
			if (store.Id != null)
			{
				if (_storeId.StoreType == ImageConstants.PartitionTypeGpt)
				{
					_storeId.StoreId_GPT = Guid.Parse(store.Id);
				}
				else
				{
					_storeId.StoreId_MBR = Convert.ToUInt32(store.Id);
				}
			}
			IntPtr preexistingHandle = NativeImaging.CreateEmptyVirtualDisk(_service, VirtualDiskFilePath, ref _storeId, num * store.SectorSize, VirtualHardDiskSectorSize);
			_storeHandle = new SafeFileHandle(preexistingHandle, true);
			int num2 = Environment.TickCount - tickCount;
			Logger.LogInfo("Storage Service: Created a new image in {0:F1} seconds.", (double)num2 / 1000.0);
			string empty = string.Empty;
			string text = string.Empty;
			bool flag = false;
			foreach (FullFlashUpdateImage.FullFlashUpdatePartition partition in store.Partitions)
			{
				if (partition.Name.Equals(ImageConstants.SYSTEM_PARTITION_NAME, StringComparison.OrdinalIgnoreCase))
				{
					if (!IsPartitionHidden(partition))
					{
						flag = true;
					}
					break;
				}
			}
			try
			{
				string imagingTempPath = BuildPaths.GetImagingTempPath(Path.GetTempPath());
				empty = imagingTempPath + ".mnt\\";
				Directory.CreateDirectory(empty);
				_pathsToRemove.Add(empty);
				if (flag)
				{
					text = imagingTempPath + ".efiesp.mnt\\";
					Directory.CreateDirectory(text);
					_pathsToRemove.Add(text);
				}
			}
			catch (SecurityException innerException)
			{
				throw new ImageStorageException("Unable to retrieve a temporary path.", innerException);
			}
			try
			{
				MountFullFlashImageStoreInternal(store, payloadReader, payload, randomizeGptIds);
			}
			catch (Win32ExportException innerException2)
			{
				throw new ImageStorageException("Unable to mount the existing full flash update image.", innerException2);
			}
			if (!store.IsMainOSStore)
			{
				return;
			}
			NativeImaging.AddAccessPath(_service, _storeId, ImageConstants.MAINOS_PARTITION_NAME, empty);
			if (flag)
			{
				NativeImaging.AddAccessPath(_service, _storeId, ImageConstants.SYSTEM_PARTITION_NAME, text);
			}
			else
			{
				Logger.LogDebug("{0}: Not mounting the system partition because it is absent or hidden", MethodBase.GetCurrentMethod().Name);
			}
			if (IsImageCompressed(empty))
			{
				try
				{
					AttachWOFToVolume(ImageConstants.MAINOS_PARTITION_NAME);
					AttachWOFToVolume(ImageConstants.DATA_PARTITION_NAME);
				}
				catch (Exception)
				{
					Logger.LogWarning($"{MethodBase.GetCurrentMethod().Name}: Unable to attach WOF to a volume.");
				}
			}
		}

		internal void AttachToMountedVirtualHardDisk(string physicalDiskPath, bool readOnly, bool isMainOSStore)
		{
			string imagePath = string.Empty;
			_storeId = default(ImageStructures.STORE_ID);
			IntPtr storeHandle = IntPtr.Zero;
			NativeImaging.AttachToMountedImage(_service, physicalDiskPath, readOnly, out imagePath, out _storeId, out storeHandle);
			_storeHandle = new SafeFileHandle(storeHandle, true);
			if (isMainOSStore)
			{
				WaitForVolume(ImageConstants.MAINOS_PARTITION_NAME);
			}
			_isMainOSStorage = isMainOSStore;
			VirtualDiskFilePath = imagePath;
		}

		private string CreateBackingVhdFileName(uint sectorSize)
		{
			try
			{
				string text = null;
				text = ((sectorSize != 512) ? (Guid.NewGuid().ToString("N") + ".vhdx") : (Guid.NewGuid().ToString("N") + ".vhd"));
				string text2 = Environment.GetEnvironmentVariable("VHDTMP");
				if (text2 == null)
				{
					text2 = Path.GetDirectoryName(BuildPaths.GetImagingTempPath(Path.GetTempPath()));
				}
				return Path.Combine(text2, text);
			}
			catch (SecurityException innerException)
			{
				throw new ImageStorageException("Unable to retrieve a temporary path.", innerException);
			}
		}

		private static void PostProcessVirtualHardDisk(string virtualImagePath, IULogger logger, string bootPartitionName, string fileSystem, bool normalizeDiskSignature)
		{
			MasterBootRecord masterBootRecord = null;
			bool flag = false;
			using (DynamicHardDisk dynamicHardDisk = new DynamicHardDisk(virtualImagePath, true))
			{
				using (VirtualDiskStream stream = new VirtualDiskStream(dynamicHardDisk))
				{
					masterBootRecord = new MasterBootRecord(logger, (int)dynamicHardDisk.SectorSize);
					masterBootRecord.ReadFromStream(stream, MasterBootRecord.MbrParseType.Normal);
					if (!masterBootRecord.IsValidProtectiveMbr() && !string.IsNullOrEmpty(fileSystem) && !string.IsNullOrEmpty(bootPartitionName))
					{
						if (masterBootRecord.FindPartitionByName(bootPartitionName) == null)
						{
							throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: No bootable partition was found in the image.");
						}
						flag = true;
					}
					if (normalizeDiskSignature && masterBootRecord.DiskSignature != ImageConstants.SYSTEM_STORE_SIGNATURE)
					{
						masterBootRecord.DiskSignature = ImageConstants.SYSTEM_STORE_SIGNATURE;
						flag = true;
					}
					if (flag)
					{
						masterBootRecord.WriteToStream(stream, true);
					}
					masterBootRecord = null;
				}
			}
		}

		private void PrepareLogging()
		{
			_logWarning = LogWarning;
			_logInfo = LogInfo;
			_logDebug = LogDebug;
			NativeImaging.SetLoggingFunction(_service, NativeImaging.LogLevel.levelWarning, _logWarning);
			NativeImaging.SetLoggingFunction(_service, NativeImaging.LogLevel.levelInfo, _logInfo);
			NativeImaging.SetLoggingFunction(_service, NativeImaging.LogLevel.levelDebug, _logDebug);
			string eTWLogPath = NativeImaging.GetETWLogPath(_service);
			LogInfo($"ETW Log Path: {eTWLogPath}");
			OperatingSystem oSVersion = Environment.OSVersion;
			LogInfo($"OS Version: {oSVersion.VersionString}");
		}

		private void CleanupTemporaryPaths()
		{
			foreach (string item in _pathsToRemove)
			{
				Logger.LogInfo("{0}: Cleaning up temporary path {1}.", MethodBase.GetCurrentMethod().Name, item);
				FileUtils.DeleteTree(item);
			}
			_pathsToRemove.Clear();
		}

		public void UpdateBootConfigurationDatabase(string bcdFile, uint diskSignature)
		{
			bool save = false;
			ulong partitionOffset = NativeImaging.GetPartitionOffset(_service, _storeId, ImageConstants.MAINOS_PARTITION_NAME);
			ulong partitionOffset2 = NativeImaging.GetPartitionOffset(_service, _storeId, ImageConstants.SYSTEM_PARTITION_NAME);
			if (_image == null)
			{
				partitionOffset *= VirtualHardDiskSectorSize;
				partitionOffset2 *= VirtualHardDiskSectorSize;
			}
			else
			{
				partitionOffset *= _image.Stores[0].SectorSize;
				partitionOffset2 *= _image.Stores[0].SectorSize;
			}
			string text = null;
			try
			{
				text = GetPartitionPath(ImageConstants.SYSTEM_PARTITION_NAME) + bcdFile;
			}
			catch (ImageStorageException)
			{
				Logger.LogInfo("{0}: Not updating the BCD - unable to find the '{1}' partition.", MethodBase.GetCurrentMethod().Name, ImageConstants.SYSTEM_PARTITION_NAME);
				return;
			}
			if (!File.Exists(text))
			{
				Logger.LogInfo("{0}: Not updating the BCD - unable to find the path: {1}", MethodBase.GetCurrentMethod().Name, text);
				return;
			}
			PartitionIdentifierEx identifier = PartitionIdentifierEx.CreateSimpleMbr(partitionOffset, diskSignature);
			PartitionIdentifierEx identifier2 = PartitionIdentifierEx.CreateSimpleMbr(partitionOffset2, diskSignature);
			using (BootConfigurationDatabase bootConfigurationDatabase = new BootConfigurationDatabase(text))
			{
				bootConfigurationDatabase.Mount();
				BcdObject @object = bootConfigurationDatabase.GetObject(BcdObjects.WindowsLoader);
				BcdObject object2 = bootConfigurationDatabase.GetObject(BcdObjects.BootManager);
				BcdObject object3 = bootConfigurationDatabase.GetObject(BcdObjects.UpdateOSWim);
				BcdObject object4 = bootConfigurationDatabase.GetObject(BcdObjects.WindowsSetupRamdiskOptions);
				if (object2 == null)
				{
					throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: The Boot Manager Object was not found.");
				}
				for (int i = 0; i < bootConfigurationDatabase.Objects.Count; i++)
				{
					BcdObject bcdObject = bootConfigurationDatabase.Objects[i];
					for (int j = 0; j < bcdObject.Elements.Count; j++)
					{
						BcdElement bcdElement = bcdObject.Elements[j];
						if (bcdElement.DataType.Format != ElementFormat.Device)
						{
							continue;
						}
						BcdElementDevice bcdElementDevice = bcdElement as BcdElementDevice;
						if (bcdElementDevice == null)
						{
							throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: The default application's device element is invalid.");
						}
						if (bcdElementDevice.BootDevice.Type == BcdElementBootDevice.DeviceType.BlockIo)
						{
							if (object3 != null && bcdObject.Id == object3.Id)
							{
								bcdElementDevice.ReplaceRamDiskDeviceIdentifier(identifier);
								bootConfigurationDatabase.SaveElementValue(bcdObject, bcdElement);
								save = true;
							}
						}
						else if (bcdElementDevice.BootDevice.Type == BcdElementBootDevice.DeviceType.Boot && (bcdElementDevice.DataType.Equals(BcdElementDataTypes.OsLoaderDevice) || bcdElementDevice.DataType.Equals(BcdElementDataTypes.OsLoaderType) || bcdElementDevice.DataType.Equals(BcdElementDataTypes.RamDiskSdiDevice)))
						{
							if (bcdObject.Id == object2.Id)
							{
								bcdElementDevice.ReplaceBootDeviceIdentifier(identifier2);
								bootConfigurationDatabase.SaveElementValue(bcdObject, bcdElement);
								save = true;
								continue;
							}
							if (@object != null && bcdObject.Id == @object.Id)
							{
								bcdElementDevice.ReplaceBootDeviceIdentifier(identifier);
								bootConfigurationDatabase.SaveElementValue(bcdObject, bcdElement);
								save = true;
								continue;
							}
							if (object4 != null && bcdObject.Id == object4.Id)
							{
								bcdElementDevice.ReplaceBootDeviceIdentifier(identifier2);
								bootConfigurationDatabase.SaveElementValue(bcdObject, bcdElement);
								save = true;
								continue;
							}
							Logger.LogInfo("{0}: Modifying unknown object device elements to point to the system partition. ID is {1}", MethodBase.GetCurrentMethod().Name, bcdObject.Id);
							bcdElementDevice.ReplaceBootDeviceIdentifier(identifier2);
							bootConfigurationDatabase.SaveElementValue(bcdObject, bcdElement);
							save = true;
						}
					}
				}
				bootConfigurationDatabase.DismountHive(save);
			}
		}

		private void CleanupAllMountedDisks()
		{
			Logger.LogInfo("{0}: Cleaning up all mounted disks.", MethodBase.GetCurrentMethod().Name);
			try
			{
				for (int i = 0; i < 10; i++)
				{
					NativeImaging.DismountVirtualHardDisk(_service, _storeId, true, false);
				}
			}
			catch (ImageStorageException)
			{
			}
		}

		private void MountFullFlashImageStoreInternal(FullFlashUpdateImage.FullFlashUpdateStore store, PayloadReader payloadReader, StorePayload payload, bool randomizeGptIds)
		{
			uint imageStyle = _image.ImageStyle;
			payloadReader.WriteToDisk(SafeStoreHandle, payload);
			using (DiskStreamSource streamSource = new DiskStreamSource(SafeStoreHandle, payload.StoreHeader.BytesPerBlock))
			{
				using (DataBlockStream dataBlockStream = new DataBlockStream(streamSource, payload.StoreHeader.BytesPerBlock))
				{
					if (imageStyle == ImageConstants.PartitionTypeGpt && store.IsMainOSStore)
					{
						GuidPartitionTable guidPartitionTable = new GuidPartitionTable((int)NativeImaging.GetSectorSize(ServiceHandle, SafeStoreHandle), _logger);
						guidPartitionTable.ReadFromStream(dataBlockStream, true);
						guidPartitionTable.GetEntry(ImageConstants.MAINOS_PARTITION_NAME).Attributes |= ImageConstants.GPT_ATTRIBUTE_NO_DRIVE_LETTER;
						if (randomizeGptIds)
						{
							guidPartitionTable.RandomizeGptIds();
						}
						guidPartitionTable.FixCrcs();
						guidPartitionTable.WriteToStream(dataBlockStream, true, false);
						using (VirtualMemoryPtr virtualMemoryPtr = new VirtualMemoryPtr(payload.StoreHeader.BytesPerBlock))
						{
							foreach (DataBlockEntry blockEntry in dataBlockStream.BlockEntries)
							{
								if (blockEntry.DataSource.Source == DataBlockSource.DataSource.Memory)
								{
									long distanceToMove = (long)blockEntry.BlockLocationsOnDisk[0].BlockIndex * (long)payload.StoreHeader.BytesPerBlock;
									long newFileLocation = 0L;
									uint bytesWritten = 0u;
									Marshal.Copy(blockEntry.DataSource.GetMemoryData(), 0, virtualMemoryPtr.AllocatedPointer, (int)payload.StoreHeader.BytesPerBlock);
									Win32Exports.SetFilePointerEx(SafeStoreHandle, distanceToMove, out newFileLocation, Win32Exports.MoveMethod.FILE_BEGIN);
									Win32Exports.WriteFile(SafeStoreHandle, virtualMemoryPtr.AllocatedPointer, payload.StoreHeader.BytesPerBlock, out bytesWritten);
								}
							}
						}
					}
				}
			}
			NativeImaging.UpdateDiskLayout(_service, _storeHandle);
			_storeId = NativeImaging.GetDiskId(_service, _storeHandle);
			List<ImageStructures.PARTITION_ENTRY> list = new List<ImageStructures.PARTITION_ENTRY>();
			for (int i = 0; i < store.PartitionCount; i++)
			{
				FullFlashUpdateImage.FullFlashUpdatePartition partition = store.Partitions[i];
				ImageStructures.PARTITION_ENTRY partitionEntry = default(ImageStructures.PARTITION_ENTRY);
				PreparePartitionEntry(ref partitionEntry, store, partition, imageStyle, 1u);
				list.Add(partitionEntry);
			}
			list.TrimExcess();
			NativeImaging.WaitForPartitions(_service, _storeId, list.ToArray());
		}

		private void ValidatePartitionStrings(FullFlashUpdateImage.FullFlashUpdatePartition partition)
		{
			if (partition.Name.Length > 32)
			{
				throw new ImageStorageException($"The partition name is too long: {partition.Name}.");
			}
			if (!string.IsNullOrEmpty(partition.FileSystem) && partition.FileSystem.Length > 32)
			{
				throw new ImageStorageException($"Partition {partition.Name}'s file system is too long.");
			}
		}

		private ulong FlagsFromPartition(FullFlashUpdateImage.FullFlashUpdatePartition partition)
		{
			ulong num = 0uL;
			if (partition.Hidden)
			{
				num |= 0x4000000000000000uL;
			}
			if (partition.ReadOnly)
			{
				num |= 0x1000000000000000uL;
			}
			if (!partition.AttachDriveLetter)
			{
				num |= 0x8000000000000000uL;
			}
			return num;
		}

		private void PreparePartitionEntry(ref ImageStructures.PARTITION_ENTRY partitionEntry, FullFlashUpdateImage.FullFlashUpdateStore store, FullFlashUpdateImage.FullFlashUpdatePartition partition, uint partitionStyle, uint alignmentInBytes)
		{
			if (partitionStyle == ImageConstants.PartitionTypeGpt)
			{
				Guid partitionType;
				try
				{
					partitionType = new Guid(partition.PartitionType);
				}
				catch (Exception ex)
				{
					throw new ImageStorageException($"Partition {partition.Name}'s TYPE is invalid: {partition.PartitionType}: {ex.Message}");
				}
				partitionEntry.PartitionType = partitionType;
				Guid guid = Guid.Empty;
				bool flag = false;
				if (!string.IsNullOrEmpty(partition.PartitionId))
				{
					try
					{
						guid = new Guid(partition.PartitionId);
					}
					catch (Exception ex2)
					{
						throw new ImageStorageException($"Partition {partition.Name}'s ID is invalid: {partition.PartitionId}: {ex2.Message}");
					}
					flag = true;
				}
				if (_manager.RandomizePartitionIDs)
				{
					partitionEntry.PartitionId = Guid.NewGuid();
				}
				else if (string.Compare(ImageConstants.MAINOS_PARTITION_NAME, partition.Name, true, CultureInfo.InvariantCulture) == 0)
				{
					partitionEntry.PartitionId = ImageConstants.MAINOS_PARTITION_ID;
					if (flag)
					{
						throw new ImageStorageException($"Unable to override protected partition {partition.Name}'s ID with {partition.PartitionId}");
					}
				}
				else if (string.Compare(ImageConstants.SYSTEM_PARTITION_NAME, partition.Name, true, CultureInfo.InvariantCulture) == 0)
				{
					partitionEntry.PartitionId = ImageConstants.SYSTEM_PARTITION_ID;
					if (flag)
					{
						throw new ImageStorageException($"Unable to override protected partition {partition.Name}'s ID with {partition.PartitionId}");
					}
				}
				else if (string.Compare(ImageConstants.MMOS_PARTITION_NAME, partition.Name, true, CultureInfo.InvariantCulture) == 0)
				{
					partitionEntry.PartitionId = ImageConstants.MMOS_PARTITION_ID;
					if (flag)
					{
						throw new ImageStorageException($"Unable to override protected partition {partition.Name}'s ID with {partition.PartitionId}");
					}
				}
				else
				{
					partitionEntry.PartitionId = (flag ? guid : Guid.NewGuid());
				}
				partitionEntry.PartitionFlags = FlagsFromPartition(partition);
			}
			else
			{
				if (partition.Bootable)
				{
					partitionEntry.MBRFlags = 128;
				}
				string partitionType2 = partition.PartitionType;
				byte result = 0;
				if (partitionType2.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
				{
					if (!byte.TryParse(partitionType2.Substring(2, partitionType2.Length - 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result))
					{
						throw new ImageStorageException($"Partition MBR style {partition.Name}'s type cannot be parsed.");
					}
				}
				else if (!byte.TryParse(partitionType2, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
				{
					throw new ImageStorageException($"Partition GPT style {partition.Name}'s type cannot be parsed.");
				}
				partitionEntry.MBRType = result;
			}
			if (string.IsNullOrEmpty(partition.FileSystem))
			{
				partitionEntry.FileSystem = string.Empty;
			}
			else
			{
				partitionEntry.FileSystem = partition.FileSystem;
			}
			partitionEntry.PartitionName = partition.Name;
			partitionEntry.AlignmentSizeInBytes = alignmentInBytes;
			partitionEntry.ClusterSize = partition.ClusterSize;
			if (partition.UseAllSpace)
			{
				partitionEntry.SectorCount = 4294967295uL;
			}
			else if (store.SectorSize != VirtualHardDiskSectorSize)
			{
				partitionEntry.SectorCount = partition.TotalSectors * (store.SectorSize / VirtualHardDiskSectorSize);
			}
			else
			{
				partitionEntry.SectorCount = partition.TotalSectors;
			}
		}

		private bool IsImageCompressed(string accessPath)
		{
			IntPtr intPtr = IntPtr.Zero;
			IntPtr intPtr2 = IntPtr.Zero;
			try
			{
				intPtr = OfflineRegUtils.OpenHive(Path.Combine(accessPath, "Windows\\system32\\config\\SYSTEM"));
				intPtr2 = OfflineRegUtils.OpenKey(intPtr, "Setup");
				return BitConverter.ToUInt32(OfflineRegUtils.GetValue(intPtr2, "Compact"), 0) == 1;
			}
			catch (Win32Exception)
			{
			}
			catch (Exception)
			{
				Logger.LogWarning($"{MethodBase.GetCurrentMethod().Name}: Unable to get Compact regkey value.");
			}
			finally
			{
				if (intPtr2 != IntPtr.Zero)
				{
					OfflineRegUtils.CloseKey(intPtr2);
					intPtr2 = IntPtr.Zero;
				}
				if (intPtr != IntPtr.Zero)
				{
					OfflineRegUtils.CloseHive(intPtr);
					intPtr = IntPtr.Zero;
				}
			}
			return false;
		}

		private static bool IsPartitionHidden(FullFlashUpdateImage.FullFlashUpdatePartition partition)
		{
			if (partition.Hidden)
			{
				return true;
			}
			Guid result;
			if (Guid.TryParse(partition.PartitionType, out result) && result == ImageConstants.PARTITION_SYSTEM_GUID)
			{
				return true;
			}
			return false;
		}

		[Conditional("DEBUG")]
		internal void TestMountVirtualDisk(string existingDisk)
		{
			string tempFileName = Path.GetTempFileName();
			File.Copy(existingDisk, tempFileName, true);
			MountExistingVirtualHardDisk(tempFileName, true);
		}

		[Conditional("DEBUG")]
		internal void TestDismountVirtualDisk()
		{
			if (!_storeHandle.IsInvalid)
			{
				DismountVirtualHardDisk(false);
			}
		}

		[Conditional("DEBUG")]
		internal void TestValidateFileBuffer(byte[] fileBuffer, ulong diskOffset)
		{
			using (VirtualMemoryPtr virtualMemoryPtr = new VirtualMemoryPtr((uint)fileBuffer.Length))
			{
				uint bytesRead = 0u;
				long newFileLocation = 0L;
				Win32Exports.SetFilePointerEx(_storeHandle, (long)diskOffset, out newFileLocation, Win32Exports.MoveMethod.FILE_BEGIN);
				Win32Exports.ReadFile(_storeHandle, virtualMemoryPtr, virtualMemoryPtr.MemorySize, out bytesRead);
				if (Win32Exports.memcmp(fileBuffer, virtualMemoryPtr, (UIntPtr)bytesRead) != 0)
				{
					throw new ImageStorageException($"TEST: ValidateFileBuffer failed at disk offset {diskOffset}");
				}
			}
		}
	}
}
