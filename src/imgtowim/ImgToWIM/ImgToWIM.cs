using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;
using Microsoft.WindowsPhone.Imaging;
using Microsoft.WindowsPhone.Imaging.WimInterop;

namespace ImgToWIM
{
	internal class ImgToWIM
	{
		private static string _tempFileDirectory = FileUtils.GetTempDirectory();

		private static string _wimFile;

		private static string _mainOSPath;

		private static string _EFIESP = "EFIESP";

		private static int _iEFIESPIndex = -1;

		private static ImageStorageManager _storageManager = null;

		private static List<string> _allPartitionPaths = new List<string>();

		private static List<string> _allPartitionNames = new List<string>();

		private static string _deviceLayoutFileName = "DeviceLayout.xml";

		private static bool _bDoingFFU = false;

		private static void Main(string[] args)
		{
			bool flag = false;
			FullFlashUpdateImage fullFlashUpdateImage = null;
			Environment.ExitCode = 0;
			if (args.Count() < 2)
			{
				Console.WriteLine("Must have both image file and output WIM specified.");
				DisplayUsage();
				Environment.ExitCode = 1;
				return;
			}
			string text = args[0];
			if (!File.Exists(text))
			{
				Console.WriteLine("The image file does not exist.");
				Console.WriteLine();
				DisplayUsage();
				Environment.ExitCode = 1;
				return;
			}
			if (string.Compare(Path.GetExtension(text), ".FFU", StringComparison.OrdinalIgnoreCase) == 0)
			{
				_bDoingFFU = true;
			}
			else if (string.Compare(Path.GetExtension(text), ".VHD", StringComparison.OrdinalIgnoreCase) != 0)
			{
				Console.WriteLine("Unrecognized file type.  Must be a .FFU or .VHD file.");
				Console.WriteLine();
				DisplayUsage();
				Environment.ExitCode = 1;
				return;
			}
			_wimFile = args[1];
			_wimFile = Path.GetFullPath(_wimFile);
			if (!Directory.Exists(Path.GetDirectoryName(_wimFile)))
			{
				Console.WriteLine("The wim file directory does not exists.  Use an existing directory.");
				Console.WriteLine();
				DisplayUsage();
				Environment.ExitCode = 1;
				return;
			}
			if (args.Count() > 2)
			{
				DisplayUsage();
				Environment.ExitCode = 1;
				return;
			}
			using (Mutex mutex = new Mutex(false, "Global\\VHDMutex_{585b0806-2d3b-4226-b259-9c8d3b237d5c}"))
			{
				try
				{
					mutex.WaitOne();
					try
					{
						Console.WriteLine("Reading the image file: " + text);
						if (_bDoingFFU)
						{
							fullFlashUpdateImage = new FullFlashUpdateImage();
							fullFlashUpdateImage.Initialize(text);
							_storageManager = new ImageStorageManager();
							_storageManager.MountFullFlashImage(fullFlashUpdateImage, false);
						}
						else
						{
							_storageManager = new ImageStorageManager();
							_storageManager.MountExistingVirtualHardDisk(text, true);
						}
						flag = true;
						GetPartitionPaths();
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.Message);
						Environment.ExitCode = 1;
					}
					if (Environment.ExitCode == 0)
					{
						try
						{
							CloneImageToWim();
						}
						catch (Exception ex2)
						{
							Console.WriteLine("Failed to capture image: " + ex2.Message);
							Environment.ExitCode = 1;
						}
					}
					if (flag)
					{
						if (_bDoingFFU)
						{
							_storageManager.DismountFullFlashImage(false);
						}
						else
						{
							_storageManager.DismountVirtualHardDisk();
						}
						flag = false;
					}
				}
				finally
				{
					mutex.ReleaseMutex();
				}
			}
		}

		private static void DisplayUsage()
		{
			Console.WriteLine("ImgToWIM Usage:");
			Console.WriteLine("ImgToWIM converts an FFU/VHD to WIM file.");
			Console.WriteLine("ImgToWIM <ImageFileName> <WimFileName>");
			Console.WriteLine("\tImageFileName- FFU/VHD to file from which to capture files");
			Console.WriteLine("\tWimFileName- The output WIM file");
			Console.WriteLine();
			Console.WriteLine("\tExamples:");
			Console.WriteLine("\t\tImgToWIM Flash.ffu Flash.wim");
			Console.WriteLine("\t\tImgToWIM Flash.vhd Flash.wim");
		}

		private static void CloneImageToWim()
		{
			WindowsImageContainer windowsImageContainer = null;
			string text = Path.Combine(_tempFileDirectory, "mountDir");
			try
			{
				Console.WriteLine();
				Console.WriteLine("Creating WIM at '{0}' ...", _wimFile);
				Console.WriteLine();
				windowsImageContainer = new WindowsImageContainer(_wimFile, WindowsImageContainer.CreateFileMode.CreateAlways, WindowsImageContainer.CreateFileAccess.Write, WindowsImageContainer.CreateFileCompression.WIM_COMPRESS_LZX);
				Console.WriteLine("Capturing '{0}'...", ImageConstants.MAINOS_PARTITION_NAME);
				windowsImageContainer.CaptureImage(_mainOSPath);
				WindowsImageContainer windowsImageContainer2 = windowsImageContainer;
				windowsImageContainer2.SetBootImage(windowsImageContainer2.ImageCount);
				LongPathDirectory.CreateDirectory(text);
				windowsImageContainer[0].Mount(text, false);
				string[] directories = Directory.GetDirectories(text, "*", SearchOption.TopDirectoryOnly);
				foreach (string text2 in directories)
				{
					if ((new FileInfo(text2).Attributes & FileAttributes.ReparsePoint) != 0)
					{
						LongPathDirectory.Delete(text2);
					}
				}
				bool saveChanges = true;
				windowsImageContainer[0].DismountImage(saveChanges);
				Console.WriteLine("WIM creation complete.");
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to create WIM: " + ex.Message);
			}
			finally
			{
				FileUtils.DeleteTree(text);
				if (windowsImageContainer != null)
				{
					windowsImageContainer.Dispose();
					windowsImageContainer = null;
				}
			}
		}

		private static void GetPartitionPaths()
		{
			IULogger logger = new IULogger();
			XsdValidator xsdValidator = null;
			InputPartition[] partitions = null;
			_mainOSPath = _storageManager.GetPartitionPath(ImageConstants.MAINOS_PARTITION_NAME);
			if (string.IsNullOrEmpty(_mainOSPath))
			{
				throw new Exception("Unable to find the MainOS path. Make sure you are trying to convert and image made with ImageApp.");
			}
			string text = Path.Combine(_mainOSPath, DevicePaths.ImageUpdatePath, _deviceLayoutFileName);
			if (!File.Exists(text))
			{
				throw new Exception("Unable to find DeviceLayout file and thus unable to extract metadata from VHD.");
			}
			xsdValidator = new XsdValidator();
			try
			{
				using (Stream xsdStream = ImageGeneratorParameters.GetDeviceLayoutXSD(text))
				{
					xsdValidator.ValidateXsd(xsdStream, text, logger);
				}
			}
			catch (XsdValidatorException innerException)
			{
				xsdValidator = null;
				throw new Exception("Unable to validate Device Layout XSD.", innerException);
			}
			TextReader textReader = new StreamReader(text);
			try
			{
				partitions = ((!ImageGeneratorParameters.IsDeviceLayoutV2(text)) ? ((DeviceLayoutInput)new XmlSerializer(typeof(DeviceLayoutInput)).Deserialize(textReader)).Partitions : ((DeviceLayoutInputv2)new XmlSerializer(typeof(DeviceLayoutInputv2)).Deserialize(textReader)).MainOSStore.Partitions);
			}
			catch (Exception innerException2)
			{
				throw new Exception("Unable to parse Device Layout XML.", innerException2);
			}
			finally
			{
				textReader.Close();
				xsdValidator = null;
				textReader = null;
			}
			PopulatePartitionList(partitions);
		}

		private static void PopulatePartitionList(InputPartition[] partitions)
		{
			char[] trimChars = new char[1] { '\\' };
			string text = "";
			int num = 0;
			foreach (InputPartition inputPartition in partitions)
			{
				if (string.Compare(inputPartition.Name, ImageConstants.MAINOS_PARTITION_NAME, StringComparison.OrdinalIgnoreCase) != 0)
				{
					try
					{
						text = _storageManager.GetPartitionPath(inputPartition.Name);
						text = text.TrimEnd(trimChars);
					}
					catch (ImageStorageException)
					{
						text = Path.Combine(_mainOSPath, inputPartition.Name);
					}
					_allPartitionPaths.Add(text);
					_allPartitionNames.Add(inputPartition.Name);
					num++;
					if (_iEFIESPIndex == -1 && string.Compare(inputPartition.Name, _EFIESP, StringComparison.OrdinalIgnoreCase) == 0)
					{
						_iEFIESPIndex = num;
					}
				}
			}
			if (_iEFIESPIndex == -1)
			{
				throw new Exception("The EFIESP partition could not be found in the image.  Make sure you are using a Windows Phone 8 image as the source.");
			}
		}
	}
}
