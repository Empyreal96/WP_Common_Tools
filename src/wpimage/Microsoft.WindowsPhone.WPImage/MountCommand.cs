using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;
using Microsoft.WindowsPhone.Imaging;

namespace Microsoft.WindowsPhone.WPImage
{
	internal class MountCommand : IWPImageCommand
	{
		private string _path;

		private bool _waitForSystemVolume;

		private bool _randomizeGptIds;

		public string Name => "Mount";

		public bool ParseArgs(string[] args)
		{
			if (args.Length < 1)
			{
				return false;
			}
			_path = args[0];
			if (!File.Exists(_path))
			{
				Console.Error.WriteLine("The file {0} does not exist.", _path);
				return false;
			}
			for (int i = 1; i < args.Length; i++)
			{
				if (string.Compare(args[i], "waitForMountedSystemVolume", true, CultureInfo.InvariantCulture) == 0)
				{
					_waitForSystemVolume = true;
				}
				else if (string.Compare(args[i], "randomGptIds", true, CultureInfo.InvariantCulture) == 0)
				{
					_randomizeGptIds = true;
				}
			}
			return true;
		}

		public void PrintUsage()
		{
			Console.WriteLine("{0} {1} image_path", "WPImage.exe", Name);
			Console.WriteLine("  image_path should point to a Windows Phone 8 VHD or FFU");
		}

		public void Run()
		{
			bool flag = true;
			string extension = Path.GetExtension(_path);
			if (string.IsNullOrEmpty(extension))
			{
				Console.WriteLine("Unknown file extension.  Trying to mount the image as a full flash image.");
			}
			else if (string.Compare(extension, ".vhd", true, CultureInfo.InvariantCulture) == 0)
			{
				Console.WriteLine("Mounting an existing virtual disk image.");
				flag = false;
			}
			else if (string.Compare(extension, ".ffu", true, CultureInfo.InvariantCulture) != 0)
			{
				Console.WriteLine("Unknown file extension.  Trying to mount the image as a full flash image.");
			}
			IULogger logger = new IULogger
			{
				DebugLogger = WPImage.NullLog
			};
			FullFlashUpdateImage fullFlashUpdateImage = null;
			ImageStorageManager imageStorageManager = new ImageStorageManager(logger);
			string text = null;
			try
			{
				if (flag)
				{
					fullFlashUpdateImage = new FullFlashUpdateImage();
					Console.WriteLine("Initializing full flash update image {0}.", _path);
					fullFlashUpdateImage.Initialize(_path);
					Console.WriteLine("Mounting the full flash update image.");
					imageStorageManager.VirtualHardDiskSectorSize = fullFlashUpdateImage.Stores[0].SectorSize;
					uint storeHeaderVersion = imageStorageManager.MountFullFlashImage(fullFlashUpdateImage, _randomizeGptIds);
					if (_waitForSystemVolume)
					{
						foreach (FullFlashUpdateImage.FullFlashUpdatePartition partition in fullFlashUpdateImage.Stores.Single((FullFlashUpdateImage.FullFlashUpdateStore s) => s.IsMainOSStore).Partitions)
						{
							if (string.Compare(ImageConstants.SYSTEM_PARTITION_NAME, partition.Name, true, CultureInfo.InvariantCulture) == 0)
							{
								imageStorageManager.WaitForVolume(ImageConstants.SYSTEM_PARTITION_NAME);
								text = imageStorageManager.GetPartitionPath(ImageConstants.SYSTEM_PARTITION_NAME);
								break;
							}
						}
					}
					StoreMountStateToFiles(imageStorageManager, fullFlashUpdateImage, storeHeaderVersion);
				}
				else
				{
					imageStorageManager.MountExistingVirtualHardDisk(_path, false);
				}
				imageStorageManager.WaitForVolume(ImageConstants.MAINOS_PARTITION_NAME);
				string partitionPath = imageStorageManager.GetPartitionPath(ImageConstants.MAINOS_PARTITION_NAME);
				fullFlashUpdateImage.DisplayImagePlatformID(logger);
				Console.WriteLine("\tMain Mount Path: {0}", partitionPath);
				if (!string.IsNullOrEmpty(text))
				{
					Console.WriteLine("\tSystem Mount Path: {0}", text);
				}
				Console.WriteLine("\tPhysical Disk Name: {0}", imageStorageManager.MainOSStorage.GetDiskName());
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to mount the image: {0}", ex.Message);
			}
		}

		private void StoreMountStateToFiles(ImageStorageManager storageManager, FullFlashUpdateImage fullFlashImage, uint storeHeaderVersion)
		{
			string text = null;
			try
			{
				text = storageManager.GetPartitionPath(ImageConstants.MAINOS_PARTITION_NAME);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Unable to find MainOS partition: " + ex.Message);
				throw;
			}
			using (StreamWriter streamWriter = new StreamWriter(Path.Combine(text, "Windows\\ImageUpdate\\wpimage-storage.txt")))
			{
				streamWriter.WriteLine(storageManager.MainOSStorage.GetDiskName());
				foreach (FullFlashUpdateImage.FullFlashUpdateStore store in fullFlashImage.Stores)
				{
					ImageStorage imageStorage = storageManager.GetImageStorage(store);
					streamWriter.WriteLine(imageStorage.GetDiskName());
				}
			}
			using (StreamWriter streamWriter2 = new StreamWriter(Path.Combine(text, "Windows\\ImageUpdate\\wpimage-info.txt")))
			{
				streamWriter2.WriteLine(storeHeaderVersion);
			}
			try
			{
				string[] contents = new string[4] { fullFlashImage.OSVersion, fullFlashImage.AntiTheftVersion, fullFlashImage.RulesVersion, fullFlashImage.RulesData };
				File.WriteAllLines(Path.Combine(text, "Windows\\ImageUpdate\\wpimage-manifest.txt"), contents);
			}
			catch (Exception ex2)
			{
				Console.WriteLine("Non-fatal: Failed to preserve manifest data: {0}", ex2.Message);
			}
		}
	}
}
