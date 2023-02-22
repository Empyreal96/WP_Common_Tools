using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;
using Microsoft.WindowsPhone.Imaging;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Images
{
	public class WPPartition : FullFlashUpdateImage.FullFlashUpdatePartition
	{
		private ImageStorageManager _storageManager;

		private IULogger _logger;

		private string _path = string.Empty;

		private List<IPkgInfo> _packages = new List<IPkgInfo>();

		private byte _mbrPartitionType;

		private Guid _gptPartitionType;

		private bool _isBinaryPartition;

		private WPImage _image;

		private bool _bInvalidPartition;

		private bool _isWIM;

		private string _sourceWIM = string.Empty;

		private long _wimFileSize;

		private long _wimFileContentSize;

		private string _wimMountPoint = string.Empty;

		private string _wimPath = string.Empty;

		private bool _win32Accessible;

		private bool _attemptedToMakeWin32Accessible;

		private string _mountPoint;

		public bool Win32Accessible;

		private const int S_OK = 0;

		private const int WimNoCommit = 0;

		public string MountPoint
		{
			get
			{
				if (string.IsNullOrEmpty(_mountPoint))
				{
					string text = Path.Combine(_image.TempDirectoryPath, base.Name);
					if (Directory.Exists(text))
					{
						Directory.Delete(text);
					}
					Directory.CreateDirectory(text);
					_mountPoint = text + "\\";
				}
				return _mountPoint;
			}
		}

		public bool IsWim => _isWIM;

		public string WimFile => _sourceWIM;

		public string PartitionTypeLabel
		{
			get
			{
				if (!IsWim)
				{
					return "Partition";
				}
				return "WIM";
			}
		}

		public long WimFileSize => _wimFileSize;

		public long WimFileContentSize => _wimFileContentSize;

		public bool IsBinaryPartition
		{
			get
			{
				return _isBinaryPartition;
			}
			set
			{
				_isBinaryPartition = value;
			}
		}

		public bool InvalidPartition => _bInvalidPartition;

		public string PartitionPath => _path;

		public int PackageCount => _packages.Count;

		[CLSCompliant(false)]
		public List<IPkgInfo> Packages => _packages;

		public WPPartition(WPImage image, string wimPath, ImageStorageManager storageManager, IULogger logger)
		{
			_storageManager = storageManager;
			_logger = logger;
			_image = image;
			base.Name = Path.GetFileNameWithoutExtension(wimPath);
			base.AttachDriveLetter = false;
			base.Bootable = true;
			base.FileSystem = "NTFS";
			base.Hidden = false;
			base.PartitionType = "None";
			base.PrimaryPartition = "None";
			base.ReadOnly = true;
			base.RequiredToFlash = false;
			base.UseAllSpace = false;
			base.ByteAlignment = 0u;
			base.SectorsInUse = 0u;
			base.TotalSectors = 0u;
			base.SectorAlignment = 0u;
			_isWIM = true;
			_sourceWIM = wimPath;
			InitializeWIM();
		}

		~WPPartition()
		{
		}

		private void InitializeWIM()
		{
			_wimMountPoint = Path.Combine(_image.TempDirectoryPath, Path.GetFileNameWithoutExtension(_sourceWIM) + ".mnt");
			_wimPath = Path.Combine(_image.TempDirectoryPath, Path.GetFileName(_sourceWIM));
			File.Copy(_sourceWIM, _wimPath);
			Directory.CreateDirectory(_wimMountPoint);
			MountWim(_wimPath, _wimMountPoint);
			_path = _wimMountPoint;
			FileInfo fileInfo = new FileInfo(_sourceWIM);
			_wimFileSize = fileInfo.Length;
			_wimFileContentSize = GetDirectoryFileContentSize(_path);
			LoadPackages();
		}

		private static long GetDirectoryFileContentSize(string rootDir)
		{
			long num = 0L;
			DirectoryInfo directoryInfo = new DirectoryInfo(rootDir);
			try
			{
				foreach (FileInfo item in directoryInfo.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly))
				{
					num += item.Length;
				}
			}
			catch (UnauthorizedAccessException)
			{
				return num;
			}
			DirectoryInfo[] directories = directoryInfo.GetDirectories();
			foreach (DirectoryInfo directoryInfo2 in directories)
			{
				if (string.Compare("SYSTEM VOLUME INFORMATION", Path.GetFileName(directoryInfo2.Name), true, CultureInfo.InvariantCulture) != 0)
				{
					num += GetDirectoryFileContentSize(directoryInfo2.FullName);
				}
			}
			return num;
		}

		public WPPartition(WPImage image, InputPartition partition, ImageStorageManager storageManager, IULogger logger)
		{
			_storageManager = storageManager;
			_logger = logger;
			_image = image;
			base.Name = partition.Name;
			base.AttachDriveLetter = partition.AttachDriveLetter;
			base.Bootable = partition.Bootable;
			base.FileSystem = partition.FileSystem;
			base.Hidden = partition.Hidden;
			base.PartitionType = partition.Type;
			base.PrimaryPartition = partition.PrimaryPartition;
			base.ReadOnly = partition.ReadOnly;
			base.RequiredToFlash = partition.RequiredToFlash;
			base.TotalSectors = partition.TotalSectors;
			base.UseAllSpace = partition.UseAllSpace;
			base.ByteAlignment = partition.ByteAlignment;
			Win32Accessible = image.Win32Accessible;
			try
			{
				ulong freeBytesOnVolume = _storageManager.GetFreeBytesOnVolume(partition.Name);
				base.SectorsInUse = partition.TotalSectors - (uint)freeBytesOnVolume / image.Store.SectorSize;
				if (freeBytesOnVolume % image.Store.SectorSize != 0)
				{
					base.SectorsInUse--;
				}
			}
			catch
			{
				base.SectorsInUse = 0u;
			}
			base.SectorAlignment = 0u;
			Initialize();
		}

		public WPPartition(WPImage image, FullFlashUpdateImage.FullFlashUpdatePartition partition, ImageStorageManager storageManager, IULogger logger)
		{
			_storageManager = storageManager;
			_logger = logger;
			_image = image;
			base.Name = partition.Name;
			base.AttachDriveLetter = partition.AttachDriveLetter;
			base.Bootable = partition.Bootable;
			base.FileSystem = partition.FileSystem;
			base.Hidden = partition.Hidden;
			base.PartitionType = partition.PartitionType;
			base.PrimaryPartition = partition.PrimaryPartition;
			base.ReadOnly = partition.ReadOnly;
			base.RequiredToFlash = partition.RequiredToFlash;
			base.SectorsInUse = partition.SectorsInUse;
			base.TotalSectors = partition.TotalSectors;
			base.UseAllSpace = partition.UseAllSpace;
			base.ByteAlignment = partition.ByteAlignment;
			base.SectorAlignment = partition.SectorAlignment;
			Win32Accessible = image.Win32Accessible;
			Initialize();
		}

		public WPPartition(WPImage image, ImagePartition partition, IULogger logger)
		{
			_storageManager = null;
			_logger = logger;
			_image = image;
			_path = partition.Root;
			_wimFileContentSize = GetDirectoryFileContentSize(_path);
			if (partition.MountedDriveInfo != null)
			{
				base.Name = partition.MountedDriveInfo.VolumeLabel;
				base.FileSystem = partition.MountedDriveInfo.DriveFormat;
				base.PartitionType = partition.MountedDriveInfo.DriveType.ToString();
				base.SectorsInUse = (uint)(partition.MountedDriveInfo.TotalSize - partition.MountedDriveInfo.TotalFreeSpace);
				base.TotalSectors = (uint)partition.MountedDriveInfo.TotalSize;
			}
			else
			{
				base.Name = partition.Name;
				base.SectorsInUse = (uint)_wimFileContentSize;
				base.FileSystem = "NTFS";
				base.PartitionType = "WIM";
			}
			base.AttachDriveLetter = false;
			base.Bootable = true;
			base.Hidden = false;
			base.PrimaryPartition = "None";
			base.ReadOnly = true;
			base.RequiredToFlash = false;
			base.UseAllSpace = false;
			base.ByteAlignment = 0u;
			base.SectorAlignment = 0u;
			_isWIM = true;
			_sourceWIM = _image.MCImage.ImagePath;
		}

		private void Initialize()
		{
			if (_storageManager.MainOSStorage.StoreId.StoreType == ImageConstants.PartitionTypeMbr)
			{
				_mbrPartitionType = _storageManager.MainOSStorage.GetPartitionTypeMbr(base.Name);
			}
			else
			{
				_gptPartitionType = _storageManager.MainOSStorage.GetPartitionTypeGpt(base.Name);
			}
			try
			{
				_path = _storageManager.GetPartitionPath(base.Name);
				if (Win32Accessible)
				{
					MakeWin32Accessible();
				}
			}
			catch (Exception ex)
			{
				throw new ImagesException("Failed call to GetPartitionPath for partition '" + base.Name + "' with error: " + ex.Message, ex.InnerException);
			}
			LoadPackages();
		}

		public void Dispose()
		{
			if (IsWim)
			{
				DismountWim(_wimPath, _wimMountPoint);
			}
		}

		private void CheckWin32Accessible()
		{
			_win32Accessible = !PartitionPath.StartsWith("\\\\.\\", StringComparison.OrdinalIgnoreCase);
		}

		private void MakeWin32Accessible()
		{
			if (_win32Accessible || _attemptedToMakeWin32Accessible)
			{
				return;
			}
			CheckWin32Accessible();
			if (!_win32Accessible)
			{
				if (base.Name.Equals(ImageConstants.MAINOS_PARTITION_NAME, StringComparison.OrdinalIgnoreCase))
				{
					_path = _image.MainOSPath;
				}
				else
				{
					_path = Path.Combine(_image.MainOSPath, base.Name);
				}
			}
			_attemptedToMakeWin32Accessible = true;
		}

		public bool HasRegistryHive(SystemRegistryHiveFiles hiveType)
		{
			return File.Exists(GetRegistryHivePath(hiveType));
		}

		public string GetRegistryHiveDevicePath(SystemRegistryHiveFiles hiveType)
		{
			bool isUefiBoot = false;
			if (base.Name.ToUpper(CultureInfo.InvariantCulture) == "EFIESP")
			{
				if (_storageManager.MainOSStorage.StoreId.StoreType == ImageConstants.PartitionTypeGpt)
				{
					isUefiBoot = true;
				}
				return DevicePaths.GetRegistryHiveFilePath(hiveType, isUefiBoot);
			}
			return DevicePaths.GetRegistryHiveFilePath(hiveType);
		}

		public string GetRegistryHivePath(SystemRegistryHiveFiles hiveType)
		{
			return Path.Combine(PartitionPath, GetRegistryHiveDevicePath(hiveType));
		}

		private void LoadPackages()
		{
			char[] trimChars = new char[1] { '\\' };
			string path = Path.Combine(PartitionPath, PkgConstants.c_strDsmDeviceFolder.TrimStart(trimChars));
			List<string> list = new List<string>();
			if (Directory.Exists(path))
			{
				list.AddRange(Directory.EnumerateFiles(path, PkgConstants.c_strDsmSearchPattern));
			}
			string path2 = Path.Combine(PartitionPath, PkgConstants.c_strMumDeviceFolder.TrimStart(trimChars));
			if (Directory.Exists(path2))
			{
				list.AddRange(Directory.EnumerateFiles(path2, PkgConstants.c_strMumSearchPattern));
			}
			foreach (string item2 in list)
			{
				try
				{
					IPkgInfo item = Package.LoadInstalledPackage(item2, _path);
					_packages.Add(item);
				}
				catch (Exception ex)
				{
					_logger.LogError("Tools.ImgCommon!LoadPackages: Failed to load package dsm\\mum file '{0}' in Partition '{1}'  with error: {2} ", item2, base.Name, ex.Message);
				}
			}
		}

		public void CopyAsBinary(string destinationFile)
		{
			FileStream fileStream = new FileStream(destinationFile, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
			_storageManager.LockAndDismountVolume(base.Name);
			SafeVolumeHandle safeVolumeHandle = new SafeVolumeHandle(_storageManager.MainOSStorage, base.Name);
			VirtualMemoryPtr virtualMemoryPtr = new VirtualMemoryPtr(1048576u);
			ulong num = _storageManager.GetPartitionSize(base.Name) * _image.Store.SectorSize;
			ulong num2 = 0uL;
			try
			{
				uint bytesRead;
				for (; num2 < num; num2 += bytesRead)
				{
					bytesRead = 0u;
					uint bytesWritten = 0u;
					uint bytesToRead = (uint)Math.Min(num - num2, 1048576uL);
					Win32Exports.ReadFile(safeVolumeHandle.VolumeHandle, virtualMemoryPtr, bytesToRead, out bytesRead);
					Win32Exports.WriteFile(fileStream.SafeFileHandle, virtualMemoryPtr, bytesRead, out bytesWritten);
				}
			}
			catch (Exception ex)
			{
				throw new ImagesException("Tools.ImgCommon!CopyAsBinary: Failed while writing binary partition " + base.Name + ": " + ex.Message, ex.InnerException);
			}
			finally
			{
				virtualMemoryPtr.Close();
				safeVolumeHandle.Close();
				fileStream.Close();
				safeVolumeHandle = null;
				fileStream = null;
			}
		}

		private static bool FAILED(int hr)
		{
			return hr < 0;
		}

		private bool MountWim(string wimPath, string mountPoint)
		{
			if (FAILED(MountWim(wimPath, mountPoint, Path.GetDirectoryName(wimPath))))
			{
				return false;
			}
			return true;
		}

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "IU_MountWim")]
		private static extern int MountWim([MarshalAs(UnmanagedType.LPWStr)] string WimPath, [MarshalAs(UnmanagedType.LPWStr)] string MountPath, [MarshalAs(UnmanagedType.LPWStr)] string TemporaryPath);

		private bool DismountWim(string wimPath, string mountPoint)
		{
			if (FAILED(DismountWim(wimPath, mountPoint, 0)))
			{
				return false;
			}
			return true;
		}

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "IU_DismountWim")]
		private static extern int DismountWim([MarshalAs(UnmanagedType.LPWStr)] string WimPath, [MarshalAs(UnmanagedType.LPWStr)] string MountPath, int CommitMode);
	}
}
