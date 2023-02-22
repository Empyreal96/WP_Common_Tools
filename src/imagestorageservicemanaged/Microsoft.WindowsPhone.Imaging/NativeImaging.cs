using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.WindowsPhone.Imaging
{
	internal sealed class NativeImaging
	{
		public enum LogLevel
		{
			levelError,
			levelWarning,
			levelInfo,
			levelDebug,
			levelInvalid
		}

		[DllImport("ImageStorageService.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "CreateImageStorageService")]
		private static extern int CreateImageStorageServiceNative(out IntPtr serviceHandle, [MarshalAs(UnmanagedType.FunctionPtr)] LogFunction logError);

		public static IntPtr CreateImageStorageService(LogFunction logError)
		{
			IntPtr serviceHandle = IntPtr.Zero;
			int num = CreateImageStorageServiceNative(out serviceHandle, logError);
			if (Win32Exports.FAILED(num) || serviceHandle == IntPtr.Zero)
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name} failed: {num:x}.");
			}
			return serviceHandle;
		}

		[DllImport("ImageStorageService.dll", CallingConvention = CallingConvention.StdCall)]
		public static extern void CloseImageStorageService(IntPtr service);

		[DllImport("ImageStorageService.dll", CallingConvention = CallingConvention.StdCall)]
		public static extern void SetLoggingFunction(IntPtr service, LogLevel level, [MarshalAs(UnmanagedType.FunctionPtr)] LogFunction logFunction);

		[DllImport("ImageStorageService.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "GetETWLogPath")]
		private static extern int GetETWLogPath_Native(IntPtr serviceHandle, StringBuilder logPath, uint pathLength);

		public static string GetETWLogPath(IntPtr serviceHandle)
		{
			StringBuilder stringBuilder = new StringBuilder("etwLogPath", 1024);
			int eTWLogPath_Native = GetETWLogPath_Native(serviceHandle, stringBuilder, (uint)stringBuilder.Capacity);
			if (Win32Exports.FAILED(eTWLogPath_Native))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name} failed: {eTWLogPath_Native:x}");
			}
			return stringBuilder.ToString();
		}

		[DllImport("ImageStorageService.dll", EntryPoint = "UpdateDiskLayout")]
		private static extern int UpdateDiskLayout_Native(IntPtr service, SafeFileHandle diskHandle);

		public static void UpdateDiskLayout(IntPtr service, SafeFileHandle diskHandle)
		{
			int num = 0;
			num = UpdateDiskLayout_Native(service, diskHandle);
			if (Win32Exports.FAILED(num))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name} failed with error code: {num:x}");
			}
		}

		[DllImport("ImageStorageService.dll", CharSet = CharSet.Unicode, EntryPoint = "InitializeVirtualHardDisk")]
		private static extern int InitializeVirtualHardDisk_Native(IntPtr service, string fileName, [MarshalAs(UnmanagedType.Bool)] bool preparePartitions, ulong maxSizeInBytes, ref ImageStructures.STORE_ID storeId, uint partitionCount, uint sectorSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 5)] ImageStructures.PARTITION_ENTRY[] partitions, bool fAssignMountPoints, uint storeIdsCount, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 10)] ImageStructures.STORE_ID[] storeIds, out IntPtr storeHandle);

		public static IntPtr InitializeVirtualHardDisk(IntPtr service, string fileName, ulong maxSizeInBytes, ref ImageStructures.STORE_ID storeId, ImageStructures.PARTITION_ENTRY[] partitions, bool preparePartitions, bool fAssignMountPoints, uint sectorSize, ImageStructures.STORE_ID[] storeIds)
		{
			IntPtr storeHandle = IntPtr.Zero;
			int num = 0;
			num = InitializeVirtualHardDisk_Native(service, fileName, preparePartitions, maxSizeInBytes, ref storeId, (uint)partitions.Length, sectorSize, partitions, fAssignMountPoints, (uint)storeIds.Length, storeIds, out storeHandle);
			if (Win32Exports.FAILED(num))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name} failed with error code: {num:x}");
			}
			return storeHandle;
		}

		[DllImport("ImageStorageService.dll", CharSet = CharSet.Unicode, EntryPoint = "_NormalizeVolumeMountPoints@28")]
		private static extern int NormalizeVolumeMountPoints_Native(IntPtr service, ImageStructures.STORE_ID storeId, string mountPath);

		public static void NormalizeVolumeMountPoints(IntPtr service, ImageStructures.STORE_ID storeId, string mountPath)
		{
			IntPtr zero = IntPtr.Zero;
			int num = 0;
			num = NormalizeVolumeMountPoints_Native(service, storeId, mountPath);
			if (Win32Exports.FAILED(num))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name} failed with error code: {num:x}");
			}
		}

		[DllImport("ImageStorageService.dll", CharSet = CharSet.Unicode, EntryPoint = "WriteMountManagerRegistry2")]
		private static extern int WriteMountManagerRegistry2_Native(IntPtr service, ImageStructures.STORE_ID storeId, bool useWellKnownGuids);

		public static void WriteMountManagerRegistry2(IntPtr service, ImageStructures.STORE_ID storeId, bool useWellKnownGuids)
		{
			IntPtr zero = IntPtr.Zero;
			int num = 0;
			num = WriteMountManagerRegistry2_Native(service, storeId, useWellKnownGuids);
			if (Win32Exports.FAILED(num))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name} failed with error code: {num:x}");
			}
		}

		[DllImport("ImageStorageService.dll", CharSet = CharSet.Unicode, EntryPoint = "CreateEmptyVirtualDisk")]
		private static extern int CreateEmptyVirtualDisk_Native(IntPtr service, string fileName, ref ImageStructures.STORE_ID storeId, ulong maxSizeInBytes, uint sectorSize, out IntPtr storeHandle);

		public static IntPtr CreateEmptyVirtualDisk(IntPtr service, string fileName, ref ImageStructures.STORE_ID storeId, ulong maxSizeInBytes, uint sectorSize)
		{
			IntPtr storeHandle = IntPtr.Zero;
			int num = 0;
			num = CreateEmptyVirtualDisk_Native(service, fileName, ref storeId, maxSizeInBytes, sectorSize, out storeHandle);
			if (Win32Exports.FAILED(num))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name} failed with error code: {num:x}");
			}
			return storeHandle;
		}

		[DllImport("ImageStorageService.dll", CharSet = CharSet.Unicode, EntryPoint = "OpenVirtualHardDisk")]
		private static extern int OpenVirtualHardDisk_Native(IntPtr service, string fileName, [MarshalAs(UnmanagedType.Bool)] bool readOnly, out ImageStructures.STORE_ID storeId, out IntPtr storeHandle);

		public static IntPtr OpenVirtualHardDisk(IntPtr service, string fileName, out ImageStructures.STORE_ID storeId, bool readOnly)
		{
			IntPtr storeHandle = IntPtr.Zero;
			int num = 0;
			num = OpenVirtualHardDisk_Native(service, fileName, readOnly, out storeId, out storeHandle);
			if (Win32Exports.FAILED(num))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}({fileName}) failed with error code: {num:x}");
			}
			return storeHandle;
		}

		[DllImport("ImageStorageService.dll", CharSet = CharSet.Unicode, EntryPoint = "AttachToMountedImage")]
		private static extern int AttachToMountedImage_Native(IntPtr service, string mountedPath, [MarshalAs(UnmanagedType.Bool)] bool openWithWriteAccess, StringBuilder imagePath, uint imagePathCharacterCount, out ImageStructures.STORE_ID storeId, out IntPtr storeHandle);

		public static void AttachToMountedImage(IntPtr service, string mountedDrivePath, bool readOnly, out string imagePath, out ImageStructures.STORE_ID storeId, out IntPtr storeHandle)
		{
			int num = 0;
			StringBuilder stringBuilder = new StringBuilder("imagePath", 32768);
			num = AttachToMountedImage_Native(service, mountedDrivePath, !readOnly, stringBuilder, (uint)stringBuilder.Capacity, out storeId, out storeHandle);
			if (Win32Exports.FAILED(num))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}({mountedDrivePath}) failed with error code: {num:x}");
			}
			imagePath = stringBuilder.ToString();
		}

		[DllImport("ImageStorageService.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "GetPartitionPath")]
		private static extern int GetPartitionPath_Native(IntPtr serviceHandle, ImageStructures.STORE_ID storeId, string partitionName, StringBuilder path, uint pathSizeInCharacters);

		public static void GetPartitionPath(IntPtr serviceHandle, ImageStructures.STORE_ID storeId, string partitionName, StringBuilder path, uint pathSizeInCharacters)
		{
			int partitionPath_Native = GetPartitionPath_Native(serviceHandle, storeId, partitionName, path, pathSizeInCharacters);
			if (Win32Exports.FAILED(partitionPath_Native))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}({partitionName}) failed: {partitionPath_Native:x}");
			}
		}

		[DllImport("ImageStorageService.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "GetPartitionPathNoContext")]
		private static extern int GetPartitionPathNoContext_Native(string partitionName, StringBuilder path, uint pathSizeInCharacters);

		public static void GetPartitionPathNoContext(string partitionName, StringBuilder path, uint pathSizeInCharacters)
		{
			int partitionPathNoContext_Native = GetPartitionPathNoContext_Native(partitionName, path, pathSizeInCharacters);
			if (Win32Exports.FAILED(partitionPathNoContext_Native))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}({partitionName}) failed: {partitionPathNoContext_Native:x}");
			}
		}

		[DllImport("ImageStorageService.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "GetPartitionFileSystem")]
		private static extern int GetPartitionFileSystem_Native(IntPtr serviceHandle, ImageStructures.STORE_ID storeId, string partitionName, StringBuilder fileSystem, uint fileSystemSizeInCharacters);

		public static string GetPartitionFileSystem(IntPtr serviceHandle, ImageStructures.STORE_ID storeId, string partitionName)
		{
			StringBuilder stringBuilder = new StringBuilder("fileSystem", 260);
			int partitionFileSystem_Native = GetPartitionFileSystem_Native(serviceHandle, storeId, partitionName, stringBuilder, (uint)stringBuilder.Capacity);
			if (Win32Exports.FAILED(partitionFileSystem_Native))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}({partitionName}) failed: {partitionFileSystem_Native:x}");
			}
			return stringBuilder.ToString();
		}

		[DllImport("ImageStorageService.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "GetDiskName")]
		private static extern int GetDiskName_Native(IntPtr serviceHandle, ImageStructures.STORE_ID storeId, StringBuilder fileSystem, uint fileSystemSizeInCharacters);

		public static string GetDiskName(IntPtr serviceHandle, ImageStructures.STORE_ID storeId)
		{
			StringBuilder stringBuilder = new StringBuilder("diskName", 32768);
			int diskName_Native = GetDiskName_Native(serviceHandle, storeId, stringBuilder, (uint)stringBuilder.Capacity);
			if (Win32Exports.FAILED(diskName_Native))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name} failed: {diskName_Native:x}");
			}
			return stringBuilder.ToString();
		}

		[DllImport("ImageStorageService.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "GetVirtualHardDiskFileName")]
		private static extern int GetVhdFileName_Native(IntPtr serviceHandle, ImageStructures.STORE_ID storeId, StringBuilder imagePath, uint imagePathSizeInCharacters);

		public static string GetVhdFileName(IntPtr serviceHandle, ImageStructures.STORE_ID storeId)
		{
			StringBuilder stringBuilder = new StringBuilder("imagePath", 32768);
			int vhdFileName_Native = GetVhdFileName_Native(serviceHandle, storeId, stringBuilder, (uint)stringBuilder.Capacity);
			if (Win32Exports.FAILED(vhdFileName_Native))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name} failed: {vhdFileName_Native:x}");
			}
			return stringBuilder.ToString();
		}

		[DllImport("ImageStorageService.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "GetDiskId")]
		private static extern int GetDiskId_Native(IntPtr serviceHandle, SafeFileHandle diskHandle, out ImageStructures.STORE_ID storeId);

		public static ImageStructures.STORE_ID GetDiskId(IntPtr serviceHandle, SafeFileHandle diskHandle)
		{
			ImageStructures.STORE_ID storeId = default(ImageStructures.STORE_ID);
			int diskId_Native = GetDiskId_Native(serviceHandle, diskHandle, out storeId);
			if (Win32Exports.FAILED(diskId_Native))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name} failed: {diskId_Native:x}");
			}
			return storeId;
		}

		[DllImport("ImageStorageService.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "GetPartitionType")]
		private static extern int GetPartitionType_Native(IntPtr serviceHandle, ImageStructures.STORE_ID storeId, string partitionName, out ImageStructures.PartitionType partitionType);

		public static ImageStructures.PartitionType GetPartitionType(IntPtr serviceHandle, ImageStructures.STORE_ID storeId, string partitionName)
		{
			ImageStructures.PartitionType partitionType = default(ImageStructures.PartitionType);
			int partitionType_Native = GetPartitionType_Native(serviceHandle, storeId, partitionName, out partitionType);
			if (Win32Exports.FAILED(partitionType_Native))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}({partitionName}) failed: {partitionType_Native:x}");
			}
			return partitionType;
		}

		[DllImport("ImageStorageService.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "SetPartitionType")]
		private static extern int SetPartitionType_Native(IntPtr serviceHandle, ImageStructures.STORE_ID storeId, string partitionName, ImageStructures.PartitionType partitionType);

		public static void SetPartitionType(IntPtr serviceHandle, ImageStructures.STORE_ID storeId, string partitionName, ImageStructures.PartitionType partitionType)
		{
			int num = SetPartitionType_Native(serviceHandle, storeId, partitionName, partitionType);
			if (Win32Exports.FAILED(num))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}({partitionName}) failed: {num:x}");
			}
		}

		[DllImport("ImageStorageService.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "SetPartitionAttributes")]
		private static extern int SetPartitionAttributes_Native(IntPtr serviceHandle, ImageStructures.STORE_ID storeId, string partitionName, ImageStructures.PartitionAttributes attributes);

		public static void SetPartitionAttributes(IntPtr serviceHandle, ImageStructures.STORE_ID storeId, string partitionName, ImageStructures.PartitionAttributes attributes)
		{
			int num = SetPartitionAttributes_Native(serviceHandle, storeId, partitionName, attributes);
			if (Win32Exports.FAILED(num))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}({partitionName}) failed: {num:x}");
			}
		}

		[DllImport("ImageStorageService.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "GetPartitionAttributes")]
		private static extern int GetPartitionAttributes_Native(IntPtr serviceHandle, ImageStructures.STORE_ID storeId, string partitionName, out ImageStructures.PartitionAttributes attributes);

		public static ImageStructures.PartitionAttributes GetPartitionAttributes(IntPtr serviceHandle, ImageStructures.STORE_ID storeId, string partitionName)
		{
			ImageStructures.PartitionAttributes attributes = default(ImageStructures.PartitionAttributes);
			int partitionAttributes_Native = GetPartitionAttributes_Native(serviceHandle, storeId, partitionName, out attributes);
			if (Win32Exports.FAILED(partitionAttributes_Native))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}({partitionName}) failed: {partitionAttributes_Native:x}");
			}
			return attributes;
		}

		[DllImport("ImageStorageService.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "SetDiskAttributes")]
		private static extern int SetDiskAttributes_Native(IntPtr serviceHandle, IntPtr diskHandle, out ImageStructures.SetDiskAttributes attributes);

		public static void SetDiskAttributes(IntPtr serviceHandle, IntPtr diskHandle, ImageStructures.SetDiskAttributes attributes)
		{
			ImageStructures.SetDiskAttributes attributes2 = attributes;
			int num = SetDiskAttributes_Native(serviceHandle, diskHandle, out attributes2);
			if (Win32Exports.FAILED(num))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name} failed: {num:x}");
			}
		}

		[DllImport("ImageStorageService.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "DismountVirtualHardDisk")]
		private static extern int DismountVirtualHardDisk_Native(IntPtr service, ImageStructures.STORE_ID storeId, [MarshalAs(UnmanagedType.Bool)] bool removeAccessPaths, [MarshalAs(UnmanagedType.Bool)] bool deleteFile, [MarshalAs(UnmanagedType.Bool)] bool fFailIfDiskMissing);

		public static void DismountVirtualHardDisk(IntPtr service, ImageStructures.STORE_ID storeId, bool removeAccessPaths, bool deleteFile, bool failIfDiskMissing = false)
		{
			int num = DismountVirtualHardDisk_Native(service, storeId, removeAccessPaths, deleteFile, failIfDiskMissing);
			if (Win32Exports.FAILED(num))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name} failed: {num:x}.");
			}
		}

		[DllImport("ImageStorageService.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "DismountVirtualHardDiskByFileName")]
		private static extern int DismountVirtualHardDiskByFileName_Native(IntPtr service, string fileName, [MarshalAs(UnmanagedType.Bool)] bool deleteFile);

		public static void DismountVirtualHardDiskByName(IntPtr service, string fileName, bool deleteFile)
		{
			int num = DismountVirtualHardDiskByFileName_Native(service, fileName, deleteFile);
			if (Win32Exports.FAILED(num))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name} failed: {num:x}.");
			}
		}

		[DllImport("ImageStorageService.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "GetSectorSize")]
		private static extern int GetSectorSize_Native(IntPtr service, ImageStructures.STORE_ID storeId, out uint bytesPerSector);

		public static uint GetSectorSize(IntPtr service, ImageStructures.STORE_ID storeId)
		{
			uint bytesPerSector = 0u;
			int sectorSize_Native = GetSectorSize_Native(service, storeId, out bytesPerSector);
			if (Win32Exports.FAILED(sectorSize_Native))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name} failed: {sectorSize_Native:x}.");
			}
			return bytesPerSector;
		}

		[DllImport("ImageStorageService.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "GetPartitionOffset")]
		private static extern int GetPartitionOffset_Native(IntPtr service, ImageStructures.STORE_ID storeId, string partitionName, out ulong startingSector);

		public static ulong GetPartitionOffset(IntPtr service, ImageStructures.STORE_ID storeId, string partitionName)
		{
			ulong startingSector = 0uL;
			int partitionOffset_Native = GetPartitionOffset_Native(service, storeId, partitionName, out startingSector);
			if (Win32Exports.FAILED(partitionOffset_Native))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name} failed: {partitionOffset_Native:x}.");
			}
			return startingSector;
		}

		[DllImport("ImageStorageService.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "GetPartitionSize")]
		private static extern int GetPartitionSize_Native(IntPtr service, ImageStructures.STORE_ID storeId, string partitionName, out ulong sectorCount);

		public static ulong GetPartitionSize(IntPtr service, ImageStructures.STORE_ID storeId, string partitionName)
		{
			ulong sectorCount = 0uL;
			int partitionSize_Native = GetPartitionSize_Native(service, storeId, partitionName, out sectorCount);
			if (Win32Exports.FAILED(partitionSize_Native))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name} failed: {partitionSize_Native:x}.");
			}
			return sectorCount;
		}

		[DllImport("ImageStorageService.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "GetFreeBytesOnVolume")]
		private static extern int GetFreeBytesOnVolume_Native(IntPtr service, ImageStructures.STORE_ID storeId, string partitionName, out ulong freeBytes);

		public static ulong GetFreeBytesOnVolume(IntPtr service, ImageStructures.STORE_ID storeId, string partitionName)
		{
			ulong freeBytes = 0uL;
			int freeBytesOnVolume_Native = GetFreeBytesOnVolume_Native(service, storeId, partitionName, out freeBytes);
			if (Win32Exports.FAILED(freeBytesOnVolume_Native))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name} failed for partition {partitionName}: {freeBytesOnVolume_Native:x}.");
			}
			return freeBytes;
		}

		[DllImport("ImageStorageService.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "OpenVolume")]
		private static extern int OpenVolumeHandle_Native(IntPtr service, ImageStructures.STORE_ID storeId, string partitionName, uint requestedAccess, uint shareMode, out IntPtr volumeHandle);

		public static SafeFileHandle OpenVolumeHandle(IntPtr service, ImageStructures.STORE_ID storeId, string partitionName, FileAccess access, FileShare share)
		{
			IntPtr volumeHandle = IntPtr.Zero;
			uint num = 0u;
			uint num2 = 0u;
			if ((access & FileAccess.Read) != 0)
			{
				num2 |= 0x80000000u;
			}
			if ((access & FileAccess.Write) != 0)
			{
				num2 |= 0x40000000u;
			}
			if ((access & FileAccess.ReadWrite) != 0)
			{
				num2 |= 0x80000000u;
				num2 |= 0x40000000u;
			}
			if ((share & FileShare.Read) != 0)
			{
				num |= 1u;
			}
			if ((share & FileShare.Write) != 0)
			{
				num |= 2u;
			}
			if ((share & FileShare.ReadWrite) != 0)
			{
				num |= 1u;
				num |= 2u;
			}
			if ((share & FileShare.Delete) != 0)
			{
				num |= 4u;
			}
			int num3 = OpenVolumeHandle_Native(service, storeId, partitionName, num2, num, out volumeHandle);
			if (Win32Exports.FAILED(num3))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name} failed: {num3:x}.");
			}
			SafeFileHandle safeFileHandle = new SafeFileHandle(volumeHandle, true);
			if (safeFileHandle.IsInvalid)
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name} returned an invalid handle.");
			}
			return safeFileHandle;
		}

		[DllImport("ImageStorageService.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "CloseVolumeHandle")]
		private static extern int CloseVolumeHandle_Native(IntPtr service, IntPtr volumeHandle);

		public static void CloseVolumeHandle(IntPtr service, IntPtr volumeHandle)
		{
			int num = CloseVolumeHandle_Native(service, volumeHandle);
			if (Win32Exports.FAILED(num))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name} failed: {num:x}.");
			}
		}

		[DllImport("ImageStorageService.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "LockAndDismountVolumeByHandle")]
		private static extern int LockAndDismountVolumeByHandle_Native(IntPtr service, SafeFileHandle volumeHandle, [MarshalAs(UnmanagedType.Bool)] bool forceDismount);

		public static void LockAndDismountVolume(IntPtr service, SafeFileHandle volumeHandle, bool forceDismount)
		{
			int num = LockAndDismountVolumeByHandle_Native(service, volumeHandle, forceDismount);
			if (Win32Exports.FAILED(num))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name} failed: {num:x}.");
			}
		}

		[DllImport("ImageStorageService.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "UnlockVolumeByHandle")]
		private static extern int UnlockVolumeByHandle_Native(IntPtr service, IntPtr volumeHandle);

		public static void UnlockVolume(IntPtr service, SafeHandle volumeHandle)
		{
			int num = UnlockVolumeByHandle_Native(service, volumeHandle.DangerousGetHandle());
			if (Win32Exports.FAILED(num))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name} failed: {num:x}.");
			}
		}

		[DllImport("ImageStorageService.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "FormatPartition")]
		private static extern int FormatPartition_Native(IntPtr service, ImageStructures.STORE_ID storeId, string partitionName, string fileSystem, uint cbClusterSize);

		public static void FormatPartition(IntPtr service, ImageStructures.STORE_ID storeId, string partitionName, string fileSystem, uint cbClusterSize)
		{
			int num = FormatPartition_Native(service, storeId, partitionName, fileSystem, cbClusterSize);
			if (Win32Exports.FAILED(num))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}({partitionName}) failed: {num:x}.");
			}
		}

		[DllImport("ImageStorageService.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "AttachWOFToVolume")]
		private static extern int AttachWOFToVolume_Native(IntPtr service, string volumePath);

		public static void AttachWOFToVolume(IntPtr service, string volumePath)
		{
			int num = AttachWOFToVolume_Native(service, volumePath);
			if (Win32Exports.FAILED(num))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}({volumePath}) failed: {num:x}.");
			}
		}

		[DllImport("ImageStorageService.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "AddAccessPath")]
		private static extern int AddAccessPath_Native(IntPtr service, ImageStructures.STORE_ID storeId, string partitionName, string accessPath);

		public static void AddAccessPath(IntPtr service, ImageStructures.STORE_ID storeId, string partitionName, string accessPath)
		{
			int num = AddAccessPath_Native(service, storeId, partitionName, accessPath);
			if (Win32Exports.FAILED(num))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}({partitionName}) failed: {num:x}.");
			}
		}

		[DllImport("ImageStorageService.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "WaitForVolumeArrival")]
		private static extern int WaitForVolumeArrival_Native(IntPtr service, ImageStructures.STORE_ID storeId, string partitionName, int timeout);

		public static void WaitForVolumeArrival(IntPtr service, ImageStructures.STORE_ID storeId, string partitionName, int timeout)
		{
			int num = WaitForVolumeArrival_Native(service, storeId, partitionName, timeout);
			if (Win32Exports.FAILED(num))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}({partitionName}) failed: {num:x}.");
			}
		}

		[DllImport("ImageStorageService.dll", CharSet = CharSet.Unicode, EntryPoint = "ReadFromDisk")]
		private static extern int ReadFromDisk_Native(IntPtr service, ImageStructures.STORE_ID storeId, ulong diskOffset, uint byteCount, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] buffer);

		public static void ReadFromDisk(IntPtr service, ImageStructures.STORE_ID storeId, ulong diskOffset, byte[] buffer)
		{
			int num = ReadFromDisk_Native(service, storeId, diskOffset, (uint)buffer.Length, buffer);
			if (Win32Exports.FAILED(num))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name} failed with error code: {num:x}");
			}
		}

		[DllImport("ImageStorageService.dll", CharSet = CharSet.Unicode, EntryPoint = "GetPartitionStyle")]
		private static extern int GetPartitionStyle_Native(IntPtr service, SafeFileHandle hStore, out uint partitionStyle);

		public static uint GetPartitionStyle(IntPtr service, SafeFileHandle storeHandle)
		{
			uint partitionStyle = 0u;
			int partitionStyle_Native = GetPartitionStyle_Native(service, storeHandle, out partitionStyle);
			if (Win32Exports.FAILED(partitionStyle_Native))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name} failed: {partitionStyle_Native:x}.");
			}
			return partitionStyle;
		}

		[DllImport("ImageStorageService.dll", CharSet = CharSet.Unicode, EntryPoint = "GetSectorCount")]
		private static extern int GetSectorCount_Native(IntPtr service, SafeFileHandle hStore, out ulong sectorCount);

		public static ulong GetSectorCount(IntPtr service, SafeFileHandle storeHandle)
		{
			ulong sectorCount = 0uL;
			int sectorCount_Native = GetSectorCount_Native(service, storeHandle, out sectorCount);
			if (Win32Exports.FAILED(sectorCount_Native))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name} failed: {sectorCount_Native:x}.");
			}
			return sectorCount;
		}

		[DllImport("ImageStorageService.dll", CharSet = CharSet.Unicode, EntryPoint = "GetSectorSizeFromHandle")]
		private static extern int GetSectorSizeFromHandle_Native(IntPtr service, SafeFileHandle hStore, out uint sectorCount);

		public static uint GetSectorSize(IntPtr service, SafeFileHandle storeHandle)
		{
			uint sectorCount = 0u;
			int sectorSizeFromHandle_Native = GetSectorSizeFromHandle_Native(service, storeHandle, out sectorCount);
			if (Win32Exports.FAILED(sectorSizeFromHandle_Native))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name} failed: {sectorSizeFromHandle_Native:x}.");
			}
			return sectorCount;
		}

		[DllImport("ImageStorageService.dll", CharSet = CharSet.Unicode, EntryPoint = "GetBlockAllocationBitmap")]
		private static extern int GetBlockAllocationBitmap_Native(IntPtr service, ImageStructures.STORE_ID storeId, string partitionName, uint blockSize, byte[] blockBitmapBuffer, uint bitmapBufferSize);

		public static void GetBlockAllocationBitmap(IntPtr service, ImageStructures.STORE_ID storeId, string partitionName, uint blockSize, byte[] blockBitmapBuffer)
		{
			int blockAllocationBitmap_Native = GetBlockAllocationBitmap_Native(service, storeId, partitionName, blockSize, blockBitmapBuffer, (uint)blockBitmapBuffer.Length);
			if (Win32Exports.FAILED(blockAllocationBitmap_Native))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name} failed: {blockAllocationBitmap_Native:x}.");
			}
		}

		[DllImport("ImageStorageService.dll", CharSet = CharSet.Unicode, EntryPoint = "WaitForPartitions")]
		private static extern int WaitForPartitions_Native(IntPtr service, ImageStructures.STORE_ID storeId, uint partitionCount, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] ImageStructures.PARTITION_ENTRY[] partitions);

		public static IntPtr WaitForPartitions(IntPtr service, ImageStructures.STORE_ID storeId, ImageStructures.PARTITION_ENTRY[] partitions)
		{
			IntPtr zero = IntPtr.Zero;
			int num = 0;
			num = WaitForPartitions_Native(service, storeId, (uint)partitions.Length, partitions);
			if (Win32Exports.FAILED(num))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name} failed with error code: {num:x}");
			}
			return zero;
		}

		[DllImport("ImageStorageService.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "CreateUsnJournal")]
		private static extern int CreateUsnJournal_Native(IntPtr serviceHandle, ImageStructures.STORE_ID storeId, string partitionName);

		public static void CreateUsnJournal(IntPtr serviceHandle, ImageStructures.STORE_ID storeId, string partitionName)
		{
			int num = CreateUsnJournal_Native(serviceHandle, storeId, partitionName);
			if (Win32Exports.FAILED(num))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}({partitionName} failed with error code: {num:x}");
			}
		}

		[DllImport("ImageStorageService.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "CreateJunction")]
		private static extern int CreateJunction_Native(IntPtr serviceHandle, ImageStructures.STORE_ID storeId, string sourceName, string targetPartition, string targetPath, [MarshalAs(UnmanagedType.U1)] bool useWellKnownGuids);

		public static void CreateJunction(IntPtr serviceHandle, ImageStructures.STORE_ID storeId, string sourceName, string targetPartition, string targetName, bool useWellKnownGuids = false)
		{
			int num = CreateJunction_Native(serviceHandle, storeId, sourceName, targetPartition, targetName, useWellKnownGuids);
			if (Win32Exports.FAILED(num))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name} failed with error code: {num:x}");
			}
		}
	}
}
