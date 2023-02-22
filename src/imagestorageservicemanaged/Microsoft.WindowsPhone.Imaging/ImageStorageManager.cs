using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Win32.SafeHandles;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	[CLSCompliant(false)]
	public class ImageStorageManager
	{
		private IULogger _logger;

		private FullFlashUpdateImage _image;

		private Dictionary<FullFlashUpdateImage.FullFlashUpdateStore, ImageStorage> _storages;

		private uint _virtualHardDiskSectorSize;

		private IList<string> _partitionsTargeted;

		public IULogger Logger => _logger;

		public FullFlashUpdateImage Image => _image;

		public ReadOnlyCollection<ImageStorage> Storages => _storages.Values.ToList().AsReadOnly();

		public ImageStorage MainOSStorage
		{
			get
			{
				FullFlashUpdateImage.FullFlashUpdateStore key = _storages.Keys.Single((FullFlashUpdateImage.FullFlashUpdateStore s) => s.IsMainOSStore);
				return _storages[key];
			}
		}

		public uint VirtualHardDiskSectorSize
		{
			get
			{
				return _virtualHardDiskSectorSize;
			}
			set
			{
				_virtualHardDiskSectorSize = value;
				foreach (ImageStorage value2 in _storages.Values)
				{
					value2.VirtualHardDiskSectorSize = value;
				}
			}
		}

		public bool IsDesktopImage { get; set; }

		public bool RandomizeDiskIds { get; set; }

		public bool RandomizePartitionIDs { get; set; }

		internal uint BytesPerBlock => ImageConstants.PAYLOAD_BLOCK_SIZE;

		public ImageStorageManager()
			: this(new IULogger())
		{
		}

		public ImageStorageManager(IULogger logger)
			: this(logger, null)
		{
		}

		public ImageStorageManager(IULogger logger, IList<string> partitionsTargeted)
		{
			_logger = logger;
			_partitionsTargeted = partitionsTargeted;
			_storages = new Dictionary<FullFlashUpdateImage.FullFlashUpdateStore, ImageStorage>();
			_virtualHardDiskSectorSize = ImageConstants.DefaultVirtualHardDiskSectorSize;
			MountManagerScrubRegistry();
		}

		public void SetFullFlashImage(FullFlashUpdateImage image)
		{
			_image = image;
			foreach (ImageStorage value in _storages.Values)
			{
				value.SetFullFlashImage(image);
			}
		}

		public void CreateFullFlashImage(FullFlashUpdateImage image)
		{
			if (_image != null)
			{
				bool saveChanges = false;
				DismountFullFlashImage(saveChanges);
			}
			int tickCount = Environment.TickCount;
			image.DisplayImageInformation(_logger);
			CheckForDuplicateNames(image);
			ValidateMainOsInImage(image);
			if (image.DefaultPartitionAlignmentInBytes < ImageConstants.PAYLOAD_BLOCK_SIZE)
			{
				image.DefaultPartitionAlignmentInBytes = ImageConstants.PAYLOAD_BLOCK_SIZE;
			}
			ImageStructures.STORE_ID[] array = new ImageStructures.STORE_ID[image.Stores.Count()];
			for (int i = 0; i < image.Stores.Count(); i++)
			{
				ImageStructures.STORE_ID sTORE_ID = default(ImageStructures.STORE_ID);
				FullFlashUpdateImage.FullFlashUpdateStore fullFlashUpdateStore = image.Stores[i];
				sTORE_ID.StoreType = image.ImageStyle;
				if (RandomizeDiskIds)
				{
					sTORE_ID.StoreId_GPT = Guid.NewGuid();
				}
				else if (sTORE_ID.StoreType == ImageConstants.PartitionTypeGpt)
				{
					sTORE_ID.StoreId_GPT = (fullFlashUpdateStore.IsMainOSStore ? ImageConstants.SYSTEM_STORE_GUID : Guid.Parse(fullFlashUpdateStore.Id));
				}
				else
				{
					sTORE_ID.StoreId_MBR = (fullFlashUpdateStore.IsMainOSStore ? ImageConstants.SYSTEM_STORE_SIGNATURE : Convert.ToUInt32(fullFlashUpdateStore.Id));
				}
				array[i] = sTORE_ID;
			}
			for (int j = 0; j < image.Stores.Count(); j++)
			{
				FullFlashUpdateImage.FullFlashUpdateStore fullFlashUpdateStore2 = image.Stores[j];
				if (fullFlashUpdateStore2.SectorSize > BytesPerBlock)
				{
					throw new ImageStorageException(string.Format("The sector size (0x{0:x} bytes) is greater than the image block size (0x{1x} bytes)", fullFlashUpdateStore2.SectorSize, BytesPerBlock));
				}
				if (BytesPerBlock % fullFlashUpdateStore2.SectorSize != 0)
				{
					throw new ImageStorageException(string.Format("The block size (0x{0:x} bytes) is not a mulitple of the sector size (0x{1x} bytes)", BytesPerBlock, fullFlashUpdateStore2.SectorSize));
				}
				long num = fullFlashUpdateStore2.SectorCount;
				if (num == 0L)
				{
					throw new ImageStorageException("Please specify an image size using the MinSectorCount field in the device platform information file.");
				}
				if ((ulong)(num * fullFlashUpdateStore2.SectorSize) % (ulong)BytesPerBlock != 0L)
				{
					throw new ImageStorageException(string.Format("The image size, specified by MinSectorCount, needs to be a multiple of {0} (0x{0:x}) sectors.", BytesPerBlock / fullFlashUpdateStore2.SectorSize));
				}
				for (int k = 0; k < fullFlashUpdateStore2.Partitions.Count; k++)
				{
					foreach (FullFlashUpdateImage.FullFlashUpdatePartition partition in fullFlashUpdateStore2.Partitions)
					{
						if (partition.ByteAlignment != 0 && partition.ByteAlignment < ImageConstants.PAYLOAD_BLOCK_SIZE)
						{
							partition.ByteAlignment = ImageConstants.PAYLOAD_BLOCK_SIZE;
						}
					}
				}
				CreateVirtualHardDisk(fullFlashUpdateStore2, null, image.ImageStyle, true, array[j], array);
			}
			int num2 = Environment.TickCount - tickCount;
			_logger.LogInfo("Storage Service: Created a new image in {0:F1} seconds.", (double)num2 / 1000.0);
			_image = image;
		}

		public uint MountFullFlashImage(FullFlashUpdateImage image, bool randomizeGptIds)
		{
			if (_image != null)
			{
				bool saveChanges = false;
				DismountFullFlashImage(saveChanges);
			}
			uint result = 1u;
			using (FileStream payloadStream = image.GetImageStream())
			{
				PayloadReader payloadReader = new PayloadReader(payloadStream);
				if (payloadReader.Payloads.Count() != image.StoreCount)
				{
					throw new ImageStorageException("Store counts in metadata and store header do not match");
				}
				for (int i = 0; i < image.StoreCount; i++)
				{
					FullFlashUpdateImage.FullFlashUpdateStore fullFlashUpdateStore = image.Stores[i];
					StorePayload storePayload = payloadReader.Payloads[i];
					payloadReader.ValidatePayloadPartitions((int)fullFlashUpdateStore.SectorSize, (long)fullFlashUpdateStore.SectorCount * (long)fullFlashUpdateStore.SectorSize, storePayload, image.ImageStyle, fullFlashUpdateStore.IsMainOSStore, _logger);
					ImageStorage imageStorage = new ImageStorage(_logger, this);
					imageStorage.VirtualHardDiskSectorSize = VirtualHardDiskSectorSize;
					imageStorage.MountFullFlashImageStore(fullFlashUpdateStore, payloadReader, storePayload, randomizeGptIds);
					_storages.Add(fullFlashUpdateStore, imageStorage);
					result = storePayload.StoreHeader.MajorVersion;
				}
			}
			_image = image;
			return result;
		}

		public void DismountFullFlashImage(bool saveChanges)
		{
			OutputWrapper outputWrapper = null;
			if (_image == null || _storages.Count == 0)
			{
				return;
			}
			try
			{
				outputWrapper = new OutputWrapper(_image.Stores[0].BackingFile);
				DismountFullFlashImage(saveChanges, outputWrapper, true, 1u);
			}
			finally
			{
				outputWrapper?.FinalizeWrapper();
			}
		}

		public void DismountFullFlashImage(bool saveChanges, IPayloadWrapper payloadWrapper)
		{
			DismountFullFlashImage(saveChanges, payloadWrapper, true);
		}

		public void DismountFullFlashImage(bool saveChanges, IPayloadWrapper payloadWrapper, bool deleteFile)
		{
			DismountFullFlashImage(saveChanges, payloadWrapper, deleteFile, 1u);
		}

		public void DismountFullFlashImage(bool saveChanges, IPayloadWrapper payloadWrapper, bool deleteFile, uint storeHeaderVersion)
		{
			int tickCount = Environment.TickCount;
			if (_image == null && saveChanges)
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: Cannot save changes because the full flash update image is null.");
			}
			if (_storages.Keys.Count((FullFlashUpdateImage.FullFlashUpdateStore s) => s.IsMainOSStore) != 1)
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: One and only one storage can be the MainOS storage.");
			}
			foreach (ImageStorage value in _storages.Values)
			{
				if (value.SafeStoreHandle == null)
				{
					_logger.DebugLogger("{0}: This function was called when no image is mounted.", MethodBase.GetCurrentMethod().Name);
					return;
				}
				if (value.SafeStoreHandle.IsInvalid)
				{
					throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: This function was called without a mounted image.");
				}
				if (value.Image == null && saveChanges)
				{
					throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: Cannot save changes because the full flash update image is null.");
				}
			}
			if (saveChanges)
			{
				foreach (ImageStorage value2 in _storages.Values)
				{
					if (_image.ImageStyle == ImageConstants.PartitionTypeMbr)
					{
						_logger.LogInfo("{0}:[{1}] Updating the BCD to fix partition offsets.", MethodBase.GetCurrentMethod().Name, (double)(Environment.TickCount - tickCount) / 1000.0);
						value2.UpdateBootConfigurationDatabase(ImageConstants.EFI_BCD_FILE_PATH, ImageConstants.SYSTEM_STORE_SIGNATURE);
					}
					if (value2.IsMainOSStorage)
					{
						try
						{
							string partitionPath = GetPartitionPath("CrashDump");
							if (!string.IsNullOrEmpty(partitionPath))
							{
								string partitionFileSystem = GetPartitionFileSystem("CrashDump");
								if (string.Compare("NTFS", partitionFileSystem, true, CultureInfo.InvariantCulture) != 0)
								{
									using (FileStream stream = File.Create(Path.Combine(partitionPath, "readme.txt")))
									{
										StreamWriter streamWriter = new StreamWriter(stream);
										streamWriter.WriteLine("This is a workaround for bug #48031. Please use NTFS file system for CrashDump partition to avoid this file.");
										streamWriter.Flush();
									}
								}
							}
						}
						catch (Exception)
						{
						}
						if (!IsDesktopImage)
						{
							_logger.LogInfo("{0}:[{1}] Enabling USN journal on partition {2}.", MethodBase.GetCurrentMethod().Name, (double)(Environment.TickCount - tickCount) / 1000.0, ImageConstants.MAINOS_PARTITION_NAME);
							CreateUsnJournal(ImageConstants.MAINOS_PARTITION_NAME);
							_logger.LogInfo("{0}:[{1}] Enabling USN journal on partition {2}.", MethodBase.GetCurrentMethod().Name, (double)(Environment.TickCount - tickCount) / 1000.0, ImageConstants.DATA_PARTITION_NAME);
							CreateUsnJournal(ImageConstants.DATA_PARTITION_NAME);
						}
					}
					if (value2.IsMainOSStorage)
					{
						NativeImaging.WriteMountManagerRegistry2(value2.ServiceHandle, value2.StoreId, true);
						NativeImaging.NormalizeVolumeMountPoints(value2.ServiceHandle, value2.StoreId, GetPartitionPath(ImageConstants.MAINOS_PARTITION_NAME));
					}
				}
				_logger.LogInfo("{0}:[{1}] Flushing all volumes.", MethodBase.GetCurrentMethod().Name, (double)(Environment.TickCount - tickCount) / 1000.0);
				FlushVolumesForDismount();
				using (VirtualDiskPayloadManager virtualDiskPayloadManager = new VirtualDiskPayloadManager(_logger, (ushort)storeHeaderVersion, (ushort)_storages.Count()))
				{
					foreach (ImageStorage value3 in _storages.Values)
					{
						virtualDiskPayloadManager.AddStore(value3);
					}
					virtualDiskPayloadManager.Write(payloadWrapper);
				}
				foreach (ImageStorage value4 in _storages.Values)
				{
					value4.SafeStoreHandle.Close();
					_logger.LogInfo("{0}:[{1}] Final VHD dismount.", MethodBase.GetCurrentMethod().Name, (double)(Environment.TickCount - tickCount) / 1000.0);
					NativeImaging.DismountVirtualHardDisk(value4.ServiceHandle, value4.StoreId, true, deleteFile);
					_logger.LogInfo("{0}:[{1}] Cleaning up temporary paths.", MethodBase.GetCurrentMethod().Name, (double)(Environment.TickCount - tickCount) / 1000.0);
					value4.Cleanup();
				}
			}
			else
			{
				foreach (ImageStorage value5 in _storages.Values)
				{
					if (value5.SafeStoreHandle != null)
					{
						value5.SafeStoreHandle.Close();
					}
					NativeImaging.DismountVirtualHardDiskByName(value5.ServiceHandle, value5.VirtualDiskFilePath, deleteFile);
					_logger.LogInfo("{0}:[{1}] Cleaning up temporary paths.", MethodBase.GetCurrentMethod().Name, (double)(Environment.TickCount - tickCount) / 1000.0);
					value5.Cleanup();
				}
			}
			int num = Environment.TickCount - tickCount;
			_logger.LogInfo("Storage Service: Dismounting the image in {0:F1} seconds.", (double)num / 1000.0);
			_image = null;
			_storages.Clear();
		}

		public string CreateVirtualHardDisk(FullFlashUpdateImage.FullFlashUpdateStore store, string imagePath, uint partitionStyle, bool preparePartitions)
		{
			ImageStructures.STORE_ID sTORE_ID = default(ImageStructures.STORE_ID);
			sTORE_ID.StoreType = partitionStyle;
			if (RandomizeDiskIds)
			{
				sTORE_ID.StoreId_GPT = Guid.NewGuid();
			}
			else if (partitionStyle == ImageConstants.PartitionTypeGpt)
			{
				sTORE_ID.StoreId_GPT = (store.IsMainOSStore ? ImageConstants.SYSTEM_STORE_GUID : Guid.Parse(store.Id));
			}
			else
			{
				sTORE_ID.StoreId_MBR = (store.IsMainOSStore ? ImageConstants.SYSTEM_STORE_SIGNATURE : Convert.ToUInt32(store.Id));
			}
			return CreateVirtualHardDisk(store, imagePath, partitionStyle, preparePartitions, sTORE_ID, new ImageStructures.STORE_ID[1] { sTORE_ID });
		}

		public string CreateVirtualHardDisk(FullFlashUpdateImage.FullFlashUpdateStore store, string imagePath, uint partitionStyle, bool preparePartitions, ImageStructures.STORE_ID storeId, ImageStructures.STORE_ID[] storeIds)
		{
			if (_image != null)
			{
				DismountFullFlashImage(false);
			}
			ImageStorage imageStorage = new ImageStorage(_logger, this, storeId);
			imageStorage.VirtualHardDiskSectorSize = VirtualHardDiskSectorSize;
			imageStorage.CreateVirtualHardDiskFromStore(store, imagePath, partitionStyle, preparePartitions, storeIds);
			_storages.Add(store, imageStorage);
			return imageStorage.VirtualDiskFilePath;
		}

		public void MountExistingVirtualHardDisk(string imagePath, bool readOnly)
		{
			ImageStorage imageStorage = new ImageStorage(_logger, this);
			imageStorage.VirtualHardDiskSectorSize = VirtualHardDiskSectorSize;
			imageStorage.MountExistingVirtualHardDisk(imagePath, readOnly);
			CreateFullFlashObjectFromAttachedImage(imageStorage);
		}

		public ImageStorage MountDesktopVirtualHardDisk(string imagePath, bool readOnly)
		{
			ImageStorage imageStorage = new ImageStorage(_logger, this);
			imageStorage.VirtualHardDiskSectorSize = VirtualHardDiskSectorSize;
			imageStorage.MountExistingVirtualHardDisk(imagePath, readOnly);
			return imageStorage;
		}

		public void DismountVirtualHardDisk()
		{
			DismountVirtualHardDisk(false, false, false);
		}

		public void DismountVirtualHardDisk(bool skipPostProcessing, bool deleteFile)
		{
			DismountVirtualHardDisk(skipPostProcessing, deleteFile, false);
		}

		public void DismountVirtualHardDisk(bool skipPostProcessing, bool deleteFile, bool normalizeDiskSignature)
		{
			foreach (ImageStorage value in _storages.Values)
			{
				value.DismountVirtualHardDisk(skipPostProcessing, normalizeDiskSignature);
				if (deleteFile)
				{
					LongPathFile.Delete(value.VirtualDiskFilePath);
				}
			}
			_storages.Clear();
		}

		public FullFlashUpdateImage CreateFullFlashObjectFromAttachedImage(ImageStorage storage)
		{
			List<ImageStorage> list = new List<ImageStorage>();
			list.Add(storage);
			return CreateFullFlashObjectFromAttachedImage(list);
		}

		public FullFlashUpdateImage CreateFullFlashObjectFromAttachedImage(List<ImageStorage> storages)
		{
			string text = null;
			string text2 = null;
			try
			{
				ImageStorage imageStorage = storages.Single((ImageStorage s) => s.IsMainOSStorage);
				text = Path.Combine(imageStorage.GetPartitionPath(ImageConstants.MAINOS_PARTITION_NAME), DevicePaths.DeviceLayoutFilePath);
				text2 = Path.Combine(imageStorage.GetPartitionPath(ImageConstants.MAINOS_PARTITION_NAME), DevicePaths.OemDevicePlatformFilePath);
			}
			catch (Exception)
			{
				throw new ImageStorageException("Unable to find MainOS store or there are more than one.");
			}
			return CreateFullFlashObjectFromAttachedImage(storages, text, text2);
		}

		public FullFlashUpdateImage CreateFullFlashObjectFromAttachedImage(List<ImageStorage> storages, string deviceLayoutPath, string platformInfoPath)
		{
			ImageGenerator imageGenerator = new ImageGenerator();
			ImageGeneratorParameters imageGeneratorParameters = new ImageGeneratorParameters();
			try
			{
				imageGeneratorParameters.Initialize(_logger);
				imageGeneratorParameters.ProcessInputXML(deviceLayoutPath, platformInfoPath);
				for (int i = 0; i < imageGeneratorParameters.Stores.Count; i++)
				{
					InputStore inputStore = imageGeneratorParameters.Stores[i];
					ImageStorage imageStorage = storages[i];
					imageStorage.VirtualHardDiskSectorSize = imageGeneratorParameters.VirtualHardDiskSectorSize;
					InputPartition[] partitions = inputStore.Partitions;
					foreach (InputPartition inputPartition in partitions)
					{
						if (inputPartition.MinFreeSectors != 0)
						{
							inputPartition.TotalSectors = (uint)imageStorage.GetPartitionSize(inputPartition.Name);
						}
					}
				}
				imageGenerator.Initialize(imageGeneratorParameters, _logger, IsDesktopImage);
				FullFlashUpdateImage fullFlashUpdateImage = imageGenerator.CreateFFU();
				if (storages.Count != fullFlashUpdateImage.StoreCount)
				{
					throw new ImageStorageException("Number of ImageStorage objects and stores in device layout do not match");
				}
				for (int k = 0; k < storages.Count; k++)
				{
					storages[k].SetFullFlashUpdateStore(fullFlashUpdateImage.Stores[k]);
					_storages.Add(fullFlashUpdateImage.Stores[k], storages[k]);
				}
				_image = fullFlashUpdateImage;
			}
			catch (Exception innerException)
			{
				throw new ImageStorageException("Unable to create a FullFlashImage object.", innerException);
			}
			return _image;
		}

		public ImageStorage AttachToMountedVirtualHardDisk(string physicalDiskPath, bool readOnly)
		{
			return AttachToMountedVirtualHardDisk(physicalDiskPath, readOnly, true);
		}

		public ImageStorage AttachToMountedVirtualHardDisk(string physicalDiskPath, bool readOnly, bool isMainOSStore)
		{
			ImageStorage imageStorage = new ImageStorage(_logger, this);
			imageStorage.VirtualHardDiskSectorSize = VirtualHardDiskSectorSize;
			imageStorage.AttachToMountedVirtualHardDisk(physicalDiskPath, readOnly, isMainOSStore);
			return imageStorage;
		}

		public void DetachVirtualHardDisk(bool deleteFile)
		{
			foreach (ImageStorage value in _storages.Values)
			{
				value.DetachVirtualHardDisk(deleteFile);
			}
			_storages.Clear();
		}

		public ImageStorage GetImageStorage(FullFlashUpdateImage.FullFlashUpdateStore store)
		{
			return _storages[store];
		}

		public bool IsPartitionTargeted(string partition)
		{
			if (_partitionsTargeted == null)
			{
				return true;
			}
			return _partitionsTargeted.Any((string p) => string.Compare(partition, p, true, CultureInfo.InvariantCulture) == 0);
		}

		private static void CheckForDuplicateNames(FullFlashUpdateImage image)
		{
			List<string> list = new List<string>();
			foreach (FullFlashUpdateImage.FullFlashUpdateStore store in image.Stores)
			{
				foreach (FullFlashUpdateImage.FullFlashUpdatePartition partition in store.Partitions)
				{
					if (list.Contains(partition.Name))
					{
						throw new ImageStorageException($"Partition {partition.Name} is included more than once.");
					}
					list.Add(partition.Name);
				}
			}
			list = null;
		}

		private static void ValidateMainOsInImage(FullFlashUpdateImage image)
		{
			bool flag = false;
			foreach (FullFlashUpdateImage.FullFlashUpdatePartition partition in image.Stores.Single((FullFlashUpdateImage.FullFlashUpdateStore s) => s.IsMainOSStore).Partitions)
			{
				if (string.Compare(partition.Name, ImageConstants.MAINOS_PARTITION_NAME, true, CultureInfo.InvariantCulture) == 0)
				{
					if (string.IsNullOrEmpty(partition.FileSystem))
					{
						throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: Partition '{ImageConstants.MAINOS_PARTITION_NAME}' must have a valid file system.");
					}
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: The full flash update image must contain a partition '{ImageConstants.MAINOS_PARTITION_NAME}'.");
			}
		}

		public Guid GetPartitionTypeGpt(string partitionName)
		{
			return GetImageStorageByPartitionName(partitionName).GetPartitionTypeGpt(partitionName);
		}

		public byte GetPartitionTypeMbr(string partitionName)
		{
			return GetImageStorageByPartitionName(partitionName).GetPartitionTypeMbr(partitionName);
		}

		public string GetPartitionPath(string partitionName)
		{
			return GetImageStorageByPartitionName(partitionName).GetPartitionPath(partitionName);
		}

		public ulong GetPartitionSize(string partitionName)
		{
			return GetImageStorageByPartitionName(partitionName).GetPartitionSize(partitionName);
		}

		public void SetPartitionType(string partitionName, Guid partitionType)
		{
			GetImageStorageByPartitionName(partitionName).SetPartitionType(partitionName, partitionType);
		}

		public void SetPartitionType(string partitionName, byte partitionType)
		{
			GetImageStorageByPartitionName(partitionName).SetPartitionType(partitionName, partitionType);
		}

		public string GetPartitionFileSystem(string partitionName)
		{
			return GetImageStorageByPartitionName(partitionName).GetPartitionFileSystem(partitionName);
		}

		public bool PartitionIsMountedRaw(string partitionName)
		{
			return GetImageStorageByPartitionName(partitionName).PartitionIsMountedRaw(partitionName);
		}

		public void FormatPartition(string partitionName, string fileSsytem, uint cbClusterSize)
		{
			GetImageStorageByPartitionName(partitionName).FormatPartition(partitionName, fileSsytem, cbClusterSize);
		}

		public SafeFileHandle OpenVolumeHandle(string partitionName)
		{
			return GetImageStorageByPartitionName(partitionName).OpenVolumeHandle(partitionName);
		}

		public void WaitForVolume(string volumeName)
		{
			GetImageStorageByPartitionName(volumeName).WaitForVolume(volumeName);
		}

		public void FlushVolumesForDismount()
		{
			foreach (FullFlashUpdateImage.FullFlashUpdateStore key in _storages.Keys)
			{
				ImageStorage imageStorage = _storages[key];
				foreach (FullFlashUpdateImage.FullFlashUpdatePartition partition in key.Partitions)
				{
					imageStorage.WaitForVolume(partition.Name);
					if (!imageStorage.PartitionIsMountedRaw(partition.Name))
					{
						using (SafeVolumeHandle safeVolumeHandle = new SafeVolumeHandle(imageStorage, partition.Name))
						{
							Win32Exports.FlushFileBuffers(safeVolumeHandle.VolumeHandle);
						}
					}
				}
			}
		}

		public ulong GetFreeBytesOnVolume(string partitionName)
		{
			return GetImageStorageByPartitionName(partitionName).GetFreeBytesOnVolume(partitionName);
		}

		public void CreateJunction(string sourceName, string targetPartition, string targetPath)
		{
			CreateJunction(sourceName, targetPartition, targetPath, false);
		}

		public void CreateJunction(string sourceName, string targetPartition, string targetPath, bool useWellKnownGuids)
		{
			GetImageStorageByPartitionName(targetPartition).CreateJunction(sourceName, targetPartition, targetPath, useWellKnownGuids);
		}

		public void CreateUsnJournal(string partitionName)
		{
			GetImageStorageByPartitionName(partitionName).CreateUsnJournal(partitionName);
		}

		public void AttachWOFToVolume(string partitionName)
		{
			GetImageStorageByPartitionName(partitionName).AttachWOFToVolume(partitionName);
		}

		public void LockAndDismountVolume(string partitionName)
		{
			LockAndDismountVolume(partitionName, false);
		}

		public void LockAndDismountVolume(string partitionName, bool forceDismount)
		{
			GetImageStorageByPartitionName(partitionName).LockAndDismountVolume(partitionName, forceDismount);
		}

		public void UnlockVolume(string partitionName)
		{
			GetImageStorageByPartitionName(partitionName).UnlockVolume(partitionName);
		}

		private ImageStorage GetImageStorageByPartitionName(string partitionName)
		{
			FullFlashUpdateImage.FullFlashUpdateStore key = _storages.Keys.Single((FullFlashUpdateImage.FullFlashUpdateStore s) => s.Partitions.Exists((FullFlashUpdateImage.FullFlashUpdatePartition p) => string.Compare(partitionName, p.Name, true, CultureInfo.InvariantCulture) == 0));
			return _storages[key];
		}

		private void MountManagerScrubRegistry()
		{
			using (SafeFileHandle safeFileHandle = Win32Exports.CreateFile(Win32Exports.MountManagerPath, Win32Exports.DesiredAccess.GENERIC_READ | Win32Exports.DesiredAccess.GENERIC_WRITE, Win32Exports.ShareMode.FILE_SHARE_READ | Win32Exports.ShareMode.FILE_SHARE_WRITE, Win32Exports.CreationDisposition.OPEN_EXISTING, Win32Exports.FlagsAndAttributes.FILE_ATTRIBUTE_NORMAL))
			{
				int bytesReturned = 0;
				Win32Exports.DeviceIoControl(safeFileHandle.DangerousGetHandle(), 7192632u, null, 0, null, 0, out bytesReturned);
			}
		}
	}
}
