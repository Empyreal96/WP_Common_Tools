using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;
using Microsoft.WindowsPhone.Imaging;

namespace Microsoft.WindowsPhone.WPImage
{
	internal class DismountCommand : IWPImageCommand
	{
		public static readonly string PhysicalDriveOption = "physicalDrive";

		public static readonly string TargetImageOption = "imagePath";

		public static readonly string NoSignOption = "noSign";

		private string _mainOSMountedPath;

		private string _imagePath;

		private bool _signImage = true;

		private IULogger _logger = new IULogger();

		public string Name => "Dismount";

		public bool ParseArgs(string[] args)
		{
			if (args.Length < 2)
			{
				return false;
			}
			for (int i = 0; i < args.Length; i++)
			{
				string text = args[i].Substring(1);
				if (string.Compare(text, PhysicalDriveOption.Substring(1, 1), true, CultureInfo.InstalledUICulture) == 0 || string.Compare(text.Substring(0, 1), PhysicalDriveOption.Substring(0, 1), true, CultureInfo.InstalledUICulture) == 0)
				{
					if (i + 1 >= args.Length)
					{
						return false;
					}
					_mainOSMountedPath = args[i + 1];
					if (!_mainOSMountedPath.StartsWith("\\\\.\\PhysicalDrive", true, CultureInfo.InvariantCulture))
					{
						_mainOSMountedPath = "\\\\.\\PhysicalDrive" + _mainOSMountedPath;
					}
					i++;
				}
				else if (string.Compare(text, TargetImageOption, true, CultureInfo.InstalledUICulture) == 0 || string.Compare(text.Substring(0, 1), TargetImageOption.Substring(0, 1), true, CultureInfo.InstalledUICulture) == 0)
				{
					if (i + 1 >= args.Length)
					{
						return false;
					}
					_imagePath = args[i + 1];
					i++;
				}
				else
				{
					if (string.Compare(text, NoSignOption, true, CultureInfo.InstalledUICulture) != 0 && string.Compare(text.Substring(0, 1), NoSignOption.Substring(0, 1), true, CultureInfo.InstalledUICulture) != 0)
					{
						return false;
					}
					_signImage = false;
				}
			}
			if (string.IsNullOrEmpty(_mainOSMountedPath))
			{
				return false;
			}
			return true;
		}

		public void PrintUsage()
		{
			_logger.LogInfo("{0} {4} -{1} path [-{2} path [-{3}]]", "WPImage.exe", PhysicalDriveOption, TargetImageOption, NoSignOption, Name);
			_logger.LogInfo("  -{0}: path where the image is mounted: eg. \\\\.\\PhysicalDrive2", PhysicalDriveOption);
			_logger.LogInfo("  -{0}: save to this image file", TargetImageOption);
			_logger.LogInfo("  -{0}: do not sign the image file", NoSignOption);
			_logger.LogInfo("");
		}

		public void Run()
		{
			bool flag = true;
			string extension = Path.GetExtension(_imagePath);
			if (string.IsNullOrEmpty(extension))
			{
				Console.WriteLine("Unknown file extension.  Trying to dismount the image as a full flash image.");
			}
			else if (string.Compare(extension, ".vhd", true, CultureInfo.InvariantCulture) == 0)
			{
				Console.WriteLine("Dismounting an existing virtual disk image.");
				flag = false;
			}
			else if (string.Compare(extension, ".ffu", true, CultureInfo.InvariantCulture) != 0)
			{
				Console.WriteLine("Unknown file extension.  Trying to dismount the image as a full flash image.");
			}
			if (File.Exists(_imagePath))
			{
				try
				{
					File.Delete(_imagePath);
				}
				catch (IOException ex)
				{
					_logger.LogError("Unable to delete existing output image: " + ex.Message);
					return;
				}
			}
			ImageStorageManager imageStorageManager = new ImageStorageManager(_logger);
			try
			{
				List<ImageStorage> list = new List<ImageStorage>();
				_logger.LogInfo("Attaching to mounted image at {0}.", _mainOSMountedPath);
				ImageStorage imageStorage = imageStorageManager.AttachToMountedVirtualHardDisk(_mainOSMountedPath, false, true);
				string text = null;
				try
				{
					text = imageStorage.GetPartitionPath(ImageConstants.MAINOS_PARTITION_NAME);
				}
				catch (Exception ex2)
				{
					_logger.LogWarning("Unable to find MainOS partition: " + ex2.Message);
					throw;
				}
				string path = Path.Combine(text, "Windows\\ImageUpdate\\wpimage-storage.txt");
				string path2 = Path.Combine(text, "Windows\\ImageUpdate\\wpimage-info.txt");
				string path3 = Path.Combine(text, "Windows\\ImageUpdate\\wpimage-manifest.txt");
				uint storeHeaderVersion = 1u;
				if (File.Exists(path2))
				{
					using (StreamReader streamReader = new StreamReader(path2))
					{
						storeHeaderVersion = uint.Parse(streamReader.ReadLine());
					}
				}
				try
				{
					if (flag)
					{
						if (File.Exists(path))
						{
							using (StreamReader streamReader2 = new StreamReader(path))
							{
								string strA = streamReader2.ReadLine();
								if (string.Compare(strA, _mainOSMountedPath, true, CultureInfo.InvariantCulture) != 0)
								{
									_logger.LogError("");
									throw new Exception();
								}
								string text2 = null;
								while ((text2 = streamReader2.ReadLine()) != null)
								{
									ImageStorage imageStorage2 = null;
									imageStorage2 = ((string.Compare(strA, text2, true, CultureInfo.InvariantCulture) != 0) ? imageStorageManager.AttachToMountedVirtualHardDisk(text2, true, false) : imageStorage);
									list.Add(imageStorage2);
								}
							}
						}
						else
						{
							_logger.LogWarning("Unable to find wpimage-storage.txt in MainOS partition");
						}
					}
					else
					{
						list.Add(imageStorage);
					}
				}
				catch (Exception ex3)
				{
					_logger.LogWarning("Unable to find and attach to dependent VHD/X(s): " + ex3.Message);
					throw;
				}
				if (string.IsNullOrEmpty(_imagePath))
				{
					list.ForEach(delegate(ImageStorage s)
					{
						s.DismountVirtualHardDisk(true);
					});
					return;
				}
				_logger.LogInfo("Initializing target image object.");
				FullFlashUpdateImage fullFlashUpdateImage = imageStorageManager.CreateFullFlashObjectFromAttachedImage(list);
				imageStorageManager.VirtualHardDiskSectorSize = fullFlashUpdateImage.Stores[0].SectorSize;
				try
				{
					if (File.Exists(path3))
					{
						using (StreamReader streamReader3 = new StreamReader(path3))
						{
							fullFlashUpdateImage.OSVersion = streamReader3.ReadLine();
							fullFlashUpdateImage.AntiTheftVersion = streamReader3.ReadLine();
							string text3 = streamReader3.ReadLine();
							if (!string.IsNullOrEmpty(text3))
							{
								fullFlashUpdateImage.RulesVersion = text3;
							}
							text3 = streamReader3.ReadLine();
							if (!string.IsNullOrEmpty(text3))
							{
								fullFlashUpdateImage.RulesData = text3;
							}
						}
					}
				}
				catch (Exception ex4)
				{
					_logger.LogWarning("Unable to restore previous manifest: " + ex4.Message);
				}
				LongPathFile.Delete(path);
				LongPathFile.Delete(path2);
				LongPathFile.Delete(path3);
				_logger.LogInfo("Creating the image payload from {0}", _mainOSMountedPath);
				IPayloadWrapper payloadWrapper = GetPayloadWrapper(fullFlashUpdateImage, _imagePath, _signImage);
				imageStorageManager.DismountFullFlashImage(true, payloadWrapper, true, storeHeaderVersion);
				_logger.LogInfo("Success.");
				fullFlashUpdateImage = null;
			}
			catch (Exception innerException)
			{
				_logger.LogError("Failed to dismount the image: {0}", innerException.ToString());
				while (innerException.InnerException != null)
				{
					innerException = innerException.InnerException;
					_logger.LogError("\t{0}", innerException.ToString());
				}
			}
		}

		public static IPayloadWrapper GetPayloadWrapper(FullFlashUpdateImage image, string imagePath, bool signImage)
		{
			OutputWrapper outputWrapper = new OutputWrapper(imagePath);
			IPayloadWrapper innerWrapper = outputWrapper;
			if (signImage)
			{
				innerWrapper = new SigningWrapper(image, outputWrapper);
			}
			SecurityWrapper innerWrapper2 = new SecurityWrapper(image, innerWrapper);
			return new ManifestWrapper(image, innerWrapper2);
		}
	}
}
