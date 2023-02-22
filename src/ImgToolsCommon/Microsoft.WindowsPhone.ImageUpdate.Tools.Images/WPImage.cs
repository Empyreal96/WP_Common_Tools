using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;
using Microsoft.WindowsPhone.Imaging;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Images
{
	public class WPImage : IDisposable
	{
		private ImageStorageManager _storageManager;

		private IULogger _logger;

		private List<WPPartition> _partitions = new List<WPPartition>();

		private WPStore _store;

		private bool _isFFU;

		private bool _isVHD;

		private bool _isWIM;

		public const string SystemVolumeInfo = "SYSTEM VOLUME INFORMATION";

		private MobileCoreImage _mcImage;

		private bool _alreadyDisposed;

		private WPMetadata _wpMetadata;

		private string _tempDirectoryPath = string.Empty;

		public List<string> DevicePlatformIDs;

		public string MainOSPath;

		public string MainOSMountPoint;

		public bool Win32Accessible;

		public MobileCoreImage MCImage => _mcImage;

		public WPMetadata Metadata => _wpMetadata;

		public bool IsFFU => _isFFU;

		public bool IsVHD => _isVHD;

		public bool IsWIM => _isWIM;

		public string TempDirectoryPath => _tempDirectoryPath;

		public int PartitionCount => _partitions.Count;

		public List<WPPartition> Partitions => _partitions;

		public WPStore Store => _store;

		public WPImage(IULogger logger)
		{
			_logger = logger;
			_store = new WPStore(this);
			_tempDirectoryPath = BuildPaths.GetImagingTempPath("");
			Directory.CreateDirectory(_tempDirectoryPath);
			_tempDirectoryPath = FileUtils.GetShortPathName(_tempDirectoryPath);
		}

		~WPImage()
		{
			Dispose(false);
		}

		[DllImport("kernel32.dll")]
		private static extern bool DeleteVolumeMountPoint(string lpszVolumeMountPoint);

		[DllImport("kernel32.dll")]
		private static extern bool SetVolumeMountPoint(string lpszVolumeMountPoint, string lpszVolumeName);

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool isDisposing)
		{
			if (_alreadyDisposed)
			{
				return;
			}
			if (_store != null)
			{
				_store.Dispose();
				_store = null;
			}
			if (_storageManager != null)
			{
				if (_isFFU)
				{
					_storageManager.DismountFullFlashImage(false);
				}
				else if (_isVHD)
				{
					_storageManager.DismountVirtualHardDisk(true, true);
				}
				_storageManager = null;
			}
			foreach (WPPartition partition in Partitions)
			{
				partition.Dispose();
			}
			_wpMetadata = null;
			if (!string.IsNullOrEmpty(MainOSMountPoint))
			{
				DeleteVolumeMountPoint(MainOSMountPoint);
				MainOSMountPoint = null;
			}
			if (_mcImage != null)
			{
				if (_mcImage.IsMounted)
				{
					_mcImage.Unmount();
				}
				_mcImage = null;
			}
			if (!string.IsNullOrEmpty(_tempDirectoryPath))
			{
				FileUtils.DeleteTree(_tempDirectoryPath);
			}
			_alreadyDisposed = true;
		}

		public void LoadImage(string fileName)
		{
			LoadImage(fileName, true, false);
		}

		public void LoadImage(string fileName, bool readOnly, bool randomizeGptIds)
		{
			if (!File.Exists(fileName))
			{
				throw new ImagesException("Tools.ImgCommon!LoadImage: File could not be found: '" + fileName + "'.");
			}
			switch (Path.GetExtension(fileName).ToUpper(CultureInfo.InvariantCulture))
			{
			case ".FFU":
				_isFFU = true;
				LoadFFU(fileName, randomizeGptIds);
				break;
			case ".VHD":
				_isVHD = true;
				LoadVHD(fileName, readOnly);
				break;
			case ".WIM":
				_isWIM = true;
				LoadWIM(fileName, readOnly);
				break;
			default:
				throw new ImagesException("Tools.ImgCommon!LoadImage: Unrecognized file type '" + Path.GetExtension(fileName).ToUpper(CultureInfo.InvariantCulture) + "' not supported.");
			}
		}

		private void GetMainOSPath()
		{
			if (_storageManager != null && string.IsNullOrEmpty(MainOSPath))
			{
				MainOSPath = _storageManager.GetPartitionPath(ImageConstants.MAINOS_PARTITION_NAME);
				if (MainOSPath.StartsWith("\\\\.\\", StringComparison.OrdinalIgnoreCase) && SetVolumeMountPoint(MainOSMountPoint, MainOSPath.Replace("\\\\.\\", "\\\\?\\", StringComparison.OrdinalIgnoreCase)))
				{
					MainOSPath = MainOSMountPoint;
				}
			}
		}

		private void LoadFFU(string fileName, bool randomizeGptIds)
		{
			try
			{
				_wpMetadata = new WPMetadata();
				_wpMetadata.Initialize(fileName);
			}
			catch (Exception ex)
			{
				throw new ImagesException("Tools.ImgCommon!LoadImage: Failed to Initialize WPMetadata with '" + fileName + "': " + ex.Message, ex.InnerException);
			}
			DevicePlatformIDs = _wpMetadata.DevicePlatformIDs.ToList();
			_store.Initialize(_wpMetadata.Stores[0]);
			try
			{
				IULogger iULogger = new IULogger();
				iULogger.ErrorLogger = null;
				iULogger.DebugLogger = null;
				iULogger.WarningLogger = null;
				iULogger.InformationLogger = null;
				_storageManager = new ImageStorageManager(iULogger);
				_storageManager.MountFullFlashImage(_wpMetadata, randomizeGptIds);
			}
			catch (Exception ex2)
			{
				throw new ImagesException("Tools.ImgCommon!LoadImage: Failed to Mount '" + fileName + "': " + ex2.Message, ex2.InnerException);
			}
			GetMainOSPath();
			try
			{
				foreach (FullFlashUpdateImage.FullFlashUpdatePartition partition in _wpMetadata.Stores[0].Partitions)
				{
					AddPartition(partition);
				}
				FindBinaryPartitions();
				AddWIMs();
			}
			catch (Exception ex3)
			{
				throw new ImagesException("Tools.ImgCommon!LoadImage: Failed while adding partitions: " + ex3.Message, ex3.InnerException);
			}
		}

		private void FindBinaryPartitions()
		{
			foreach (IPkgInfo pkg in Partitions.Find((WPPartition part) => part.Name.Equals(ImageConstants.MAINOS_PARTITION_NAME, StringComparison.OrdinalIgnoreCase)).Packages)
			{
				if (pkg.IsBinaryPartition)
				{
					Partitions.Find((WPPartition part) => part.Name.Equals(pkg.Partition, StringComparison.OrdinalIgnoreCase)).IsBinaryPartition = true;
				}
			}
		}

		private void LoadWIM(string fileName, bool readOnly)
		{
			MobileCoreImage mobileCoreImage = MobileCoreImage.Create(fileName);
			mobileCoreImage.Mount();
			_mcImage = mobileCoreImage;
			PopulatePartitionsFromMCImage();
		}

		private void LoadVHD(string fileName, bool readOnly)
		{
			bool flag = false;
			try
			{
				IULogger iULogger = new IULogger();
				iULogger.ErrorLogger = null;
				iULogger.DebugLogger = null;
				iULogger.WarningLogger = null;
				iULogger.InformationLogger = null;
				_storageManager = new ImageStorageManager(iULogger);
				_storageManager.MountExistingVirtualHardDisk(fileName, readOnly);
				GetMainOSPath();
				flag = true;
			}
			catch
			{
				_storageManager = null;
			}
			if (!flag)
			{
				MobileCoreImage mobileCoreImage = MobileCoreImage.Create(fileName);
				if (readOnly)
				{
					mobileCoreImage.MountReadOnly();
				}
				else
				{
					mobileCoreImage.Mount();
				}
				_mcImage = mobileCoreImage;
			}
			try
			{
				PopulatePartitionsFromVHD();
				PopulateStoreFromVHD();
			}
			catch (Exception ex)
			{
				throw new ImagesException("Tools.ImgCommon!LoadImage: Failed while adding partitions: " + ex.Message, ex.InnerException);
			}
		}

		private void AddPartition(FullFlashUpdateImage.FullFlashUpdatePartition partition)
		{
			WPPartition item = new WPPartition(this, partition, _storageManager, _logger);
			_partitions.Add(item);
		}

		private void AddWIM(string wimPath)
		{
			WPPartition item = new WPPartition(this, wimPath, _storageManager, _logger);
			_partitions.Add(item);
		}

		private void AddPartition(InputPartition partition)
		{
			WPPartition item = new WPPartition(this, partition, _storageManager, _logger);
			_partitions.Add(item);
		}

		public static DeviceLayoutInputv2 GetDeviceLayoutv2(ImageStorageManager storageManager)
		{
			DeviceLayoutInputv2 deviceLayoutInputv = new DeviceLayoutInputv2();
			XsdValidator xsdValidator = null;
			string partitionPath = storageManager.GetPartitionPath(ImageConstants.MAINOS_PARTITION_NAME);
			if (string.IsNullOrEmpty(partitionPath))
			{
				return null;
			}
			string text = Path.Combine(partitionPath, DevicePaths.DeviceLayoutFilePath);
			if (!File.Exists(text))
			{
				return null;
			}
			xsdValidator = new XsdValidator();
			try
			{
				using (Stream xsdStream = ImageGeneratorParameters.GetDeviceLayoutXSD(text))
				{
					xsdValidator.ValidateXsd(xsdStream, text, storageManager.Logger);
				}
			}
			catch (XsdValidatorException)
			{
				xsdValidator = null;
				return null;
			}
			TextReader textReader = new StreamReader(text);
			try
			{
				if (ImageGeneratorParameters.IsDeviceLayoutV2(text))
				{
					return (DeviceLayoutInputv2)new XmlSerializer(typeof(DeviceLayoutInputv2)).Deserialize(textReader);
				}
				return null;
			}
			catch
			{
				return null;
			}
			finally
			{
				textReader.Close();
				xsdValidator = null;
				textReader = null;
			}
		}

		[CLSCompliant(false)]
		public static DeviceLayoutInput GetDeviceLayout(ImageStorageManager storageManager)
		{
			DeviceLayoutInput deviceLayoutInput = new DeviceLayoutInput();
			XsdValidator xsdValidator = null;
			string partitionPath = storageManager.GetPartitionPath(ImageConstants.MAINOS_PARTITION_NAME);
			if (string.IsNullOrEmpty(partitionPath))
			{
				return null;
			}
			string text = Path.Combine(partitionPath, DevicePaths.DeviceLayoutFilePath);
			if (!File.Exists(text))
			{
				return null;
			}
			xsdValidator = new XsdValidator();
			try
			{
				using (Stream xsdStream = ImageGeneratorParameters.GetDeviceLayoutXSD(text))
				{
					xsdValidator.ValidateXsd(xsdStream, text, storageManager.Logger);
				}
			}
			catch (XsdValidatorException)
			{
				xsdValidator = null;
				return null;
			}
			TextReader textReader = new StreamReader(text);
			try
			{
				if (ImageGeneratorParameters.IsDeviceLayoutV2(text))
				{
					return null;
				}
				return (DeviceLayoutInput)new XmlSerializer(typeof(DeviceLayoutInput)).Deserialize(textReader);
			}
			catch
			{
				return null;
			}
			finally
			{
				textReader.Close();
				xsdValidator = null;
				textReader = null;
			}
		}

		public void PopulatePartitionsFromMCImage()
		{
			if (_mcImage == null)
			{
				return;
			}
			if (_mcImage.Partitions.Count() > 0)
			{
				_partitions = new List<WPPartition>();
			}
			foreach (ImagePartition partition in _mcImage.Partitions)
			{
				WPPartition item = new WPPartition(this, partition, _logger);
				_partitions.Add(item);
			}
		}

		[CLSCompliant(false)]
		public void PopulatePartitionsFromVHD()
		{
			if (_storageManager == null && _mcImage != null)
			{
				PopulatePartitionsFromMCImage();
				return;
			}
			DeviceLayoutInput deviceLayoutInput = null;
			DeviceLayoutInputv2 deviceLayoutInputv = null;
			if (deviceLayoutInput == null && deviceLayoutInputv == null)
			{
				throw new ImagesException("Tools.ImgCommon!PopulatePartitionsFromVHD: Unable to find DeviceLayout file and thus unable to extract metadata from VHD.");
			}
			if (deviceLayoutInputv != null)
			{
				Store.SectorSize = deviceLayoutInputv.SectorSize;
				InputPartition[] partitions = deviceLayoutInputv.MainOSStore.Partitions;
				foreach (InputPartition partition in partitions)
				{
					AddPartition(partition);
				}
			}
			else
			{
				Store.SectorSize = deviceLayoutInput.SectorSize;
				InputPartition[] partitions = deviceLayoutInput.Partitions;
				foreach (InputPartition partition2 in partitions)
				{
					AddPartition(partition2);
				}
			}
			AddWIMs();
		}

		private void AddWIMs()
		{
			string text = Path.Combine(_storageManager.GetPartitionPath(ImageConstants.MAINOS_PARTITION_NAME), DevicePaths.UpdateOSWIMFilePath);
			if (Directory.Exists(Path.GetDirectoryName(text)) && File.Exists(text))
			{
				AddWIM(text);
			}
			try
			{
				string text2 = Path.Combine(_storageManager.GetPartitionPath(ImageConstants.MMOS_PARTITION_NAME), DevicePaths.MMOSWIMFilePath);
				if (File.Exists(text2))
				{
					AddWIM(text2);
				}
			}
			catch
			{
			}
		}

		[CLSCompliant(false)]
		public void PopulateStoreFromVHD()
		{
			OEMDevicePlatformInput oEMDevicePlatformInput = null;
			XsdValidator xsdValidator = null;
			if (_storageManager == null)
			{
				return;
			}
			string text = Path.Combine(_storageManager.GetPartitionPath(ImageConstants.MAINOS_PARTITION_NAME), DevicePaths.OemDevicePlatformFilePath);
			if (!File.Exists(text))
			{
				throw new ImagesException("Tools.ImgCommon!PopulateStoreFromVHD: Unable to find OEM Device Platform file and thus unable to extract metadata from VHD.");
			}
			xsdValidator = new XsdValidator();
			try
			{
				using (Stream xsdStream = ImageGeneratorParameters.GetOEMDevicePlatformXSD())
				{
					xsdValidator.ValidateXsd(xsdStream, text, _logger);
				}
			}
			catch (XsdValidatorException inner)
			{
				xsdValidator = null;
				throw new ImagesException("Tools.ImgCommon!PopulateStoreFromVHD: Unable to validate OEM Device Platform XSD.", inner);
			}
			TextReader textReader = new StreamReader(text);
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(OEMDevicePlatformInput));
			try
			{
				oEMDevicePlatformInput = (OEMDevicePlatformInput)xmlSerializer.Deserialize(textReader);
			}
			catch (Exception inner2)
			{
				throw new ImagesException("Tools.ImgCommon!PopulateStoreFromVHD: Unable to parse OEM Device Platform XML.", inner2);
			}
			finally
			{
				textReader.Close();
				xsdValidator = null;
				textReader = null;
				xmlSerializer = null;
			}
			_store.Initialize(oEMDevicePlatformInput.MinSectorCount, _store.SectorSize);
			DevicePlatformIDs = oEMDevicePlatformInput.DevicePlatformIDs.ToList();
		}
	}
}
