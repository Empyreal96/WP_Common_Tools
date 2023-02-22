using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using Microsoft.WindowsPhone.CompDB;
using Microsoft.WindowsPhone.FeatureAPI;
using Microsoft.WindowsPhone.ImageUpdate.Customization;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;
using Microsoft.WindowsPhone.Imaging.WimInterop;

namespace Microsoft.WindowsPhone.Imaging
{
	public class Imaging
	{
		private IULogger _logger;

		private string _oemInputFile = string.Empty;

		private string _oemCustomizationXML = string.Empty;

		private string _oemCustomizationPPKG = string.Empty;

		private VersionInfo _oemCustomizationVersion;

		private string _msPackagesRoot = string.Empty;

		private string _updateInputFile = string.Empty;

		private string _updateInputFileGenerated = string.Empty;

		private string _outputFile = string.Empty;

		private bool _bDoingFFU = true;

		private bool _bDoingUpdate;

		private string _outputType = string.Empty;

		private string _catalogFile = string.Empty;

		private string _UOSOutputDestination = string.Empty;

		private string _PackageListFile = string.Empty;

		private string _UpdateHistoryDestination = string.Empty;

		private string _tempDirectoryPath = string.Empty;

		private string _updateStagingRoot = string.Empty;

		private DeviceLayoutValidator _deviceLayoutValidator = new DeviceLayoutValidator();

		private ReleaseType _releaseType;

		private List<IPkgInfo> _msCoreFMPackages = new List<IPkgInfo>();

		private Dictionary<string, IPkgInfo> _packageInfoList = new Dictionary<string, IPkgInfo>(StringComparer.OrdinalIgnoreCase);

		private string _osVersion;

		private const string c_AntiTheftMinVersion = "1.1";

		private List<KeyValuePair<string, string>> _dataAssetFileList = new List<KeyValuePair<string, string>>();

		private readonly ImagingTelemetryLogger _telemetryLogger = ImagingTelemetryLogger.Instance;

		private ImageStorageManager _storageManager;

		private ImageStorageManager _storageManagerStaging;

		private ImageStorageManager _storageManagerCommit;

		private FullFlashUpdateImage _ffuImage;

		private ImageSigner _imgSigner;

		private UpdateOSInput _updateInput;

		private OEMInput _oemInput;

		private string _updateHistoryFile = string.Empty;

		private string _processedFilesDir = string.Empty;

		private string _oemDevicePlatformPackagePath = string.Empty;

		private string _oemDevicePlatformDevicePath = Path.Combine("\\" + DevicePaths.ImageUpdatePath, "OEMDevicePlatform.xml");

		private string _deviceLayoutPackagePath = string.Empty;

		private string _deviceLayoutDevicePath = Path.Combine("\\" + DevicePaths.ImageUpdatePath, "DeviceLayout.xml");

		private ImageGeneratorParameters _parameters;

		private bool _hasDPPPartition;

		private bool _isOneCore;

		public bool FormatDPP;

		public bool StrictSettingPolicies;

		public bool SkipImaging;

		public bool SkipUpdateMain;

		public CpuId CPUId;

		public string BSPProductName;

		private const string _processedFilesSubdir = "ProcessedFiles";

		private const string _deviceLayout = "DeviceLayout.xml";

		private const string _oemDevicePlatform = "OEMDevicePlatform.xml";

		private const string _imgupdFilesSubdir = "Windows\\ImageUpdate";

		private const int _ImageAppRetryCount = 80;

		private const int _DisplayMessageOnCount = 6;

		private TimeSpan MutexTimeout = new TimeSpan(0, 0, 0, 15);

		private Mutex _imageAppMutex;

		private DateTime _dtStartTime;

		private DateTime _dtEndTime;

		private Stopwatch _swReqXMLProcessingTime = new Stopwatch();

		private Stopwatch _swCreateFFUTime = new Stopwatch();

		private Stopwatch _swWritingFFUTime = new Stopwatch();

		private Stopwatch _swStorageStackTime = new Stopwatch();

		private Stopwatch _swMountImageTime = new Stopwatch();

		private Stopwatch _swDismountImageTime = new Stopwatch();

		private static Stopwatch _swMutexTime = new Stopwatch();

		private TimeSpan _tsCompDBTime;

		private TimeSpan _tsCompDBAnswersTime;

		private static readonly object _lock = new object();

		private const uint c_MinimumUserStoreSize = 1717986918u;

		private BSPCompDB _bspCompDB = new BSPCompDB();

		private DeviceCompDB _deviceCompDB = new DeviceCompDB();

		private string _bspDBFile;

		private string _deviceDBFile;

		private List<FMConditionalFeature> _condFeatures = new List<FMConditionalFeature>();

		private string _buildInfo;

		private string _buildID;

		public Imaging(IULogger logger)
		{
			_logger = logger;
		}

		~Imaging()
		{
		}

		private void SetPaths()
		{
			try
			{
				_outputFile = Path.GetFullPath(_outputFile);
			}
			catch (Exception innerException)
			{
				throw new ImageCommonException("Imaging!SetPaths: OutputFile path and name are too long.", innerException);
			}
			string directoryName = Path.GetDirectoryName(_outputFile);
			LongPathDirectory.CreateDirectory(directoryName);
			directoryName = FileUtils.GetShortPathName(directoryName);
			_processedFilesDir = Path.Combine(directoryName, "ProcessedFiles");
			if (Directory.Exists(_processedFilesDir))
			{
				FileUtils.CleanDirectory(_processedFilesDir);
			}
			else
			{
				LongPathDirectory.CreateDirectory(_processedFilesDir);
			}
			_tempDirectoryPath = BuildPaths.GetImagingTempPath(directoryName) + Process.GetCurrentProcess().Id;
			_updateStagingRoot = Path.Combine(_tempDirectoryPath, "USERS\\System\\AppData\\Local\\UpdateStagingRoot");
			LongPathDirectory.CreateDirectory(_tempDirectoryPath);
			_tempDirectoryPath = FileUtils.GetShortPathName(_tempDirectoryPath);
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(_outputFile);
			if (fileNameWithoutExtension.Length == 0)
			{
				throw new ImageCommonException("Imaging!SetPaths: The Output File cannot be empty when extension is removed.");
			}
			if (_outputFile.EndsWith(".FFU", StringComparison.OrdinalIgnoreCase))
			{
				_bDoingFFU = true;
				_outputType = "FFU";
				if (CPUId == CpuId.Invalid)
				{
					_logger.LogInfo("Imaging: Generating FFU thus setting CPU Type to 'arm'.");
					CPUId = CpuId.ARM;
				}
			}
			else
			{
				if (!_outputFile.EndsWith(".VHD", StringComparison.OrdinalIgnoreCase))
				{
					throw new ImageCommonException("Imaging!SetPaths: The OutputFile must end with either '.FFU' or '.VHD'.");
				}
				_bDoingFFU = false;
				_outputType = "VHD";
				if (CPUId == CpuId.Invalid)
				{
					_logger.LogInfo("Imaging: Generating VHD thus setting CPU Type to 'x86'.");
					CPUId = CpuId.X86;
				}
			}
			if (_bDoingFFU)
			{
				_catalogFile = Path.Combine(directoryName, fileNameWithoutExtension + ".CAT");
			}
			if (!string.IsNullOrEmpty(_oemInputFile))
			{
				if (_oemInputFile.Length > 260)
				{
					_oemInputFile = FileUtils.GetShortPathName(_oemInputFile);
				}
				if (string.IsNullOrEmpty(_msPackagesRoot))
				{
					throw new ImageCommonException("Imaging!SetPaths: The OEMInputXML requires a path to MSPackagesRoot.");
				}
			}
			_bspDBFile = Path.Combine(directoryName, fileNameWithoutExtension + ".BSPDB.xml");
			_deviceDBFile = Path.Combine(directoryName, fileNameWithoutExtension + ".DeviceDB.xml");
			_bspCompDB.BuildArch = CPUId.ToString();
			_deviceCompDB.BuildArch = CPUId.ToString();
			_updateInputFileGenerated = Path.Combine(directoryName, fileNameWithoutExtension + ".UpdateInput.xml");
			if (_updateInputFileGenerated.Length > 260)
			{
				_updateInputFileGenerated = FileUtils.GetShortPathName(_updateInputFileGenerated);
			}
			_UOSOutputDestination = Path.Combine(directoryName, fileNameWithoutExtension + "." + DevicePaths.UpdateOutputFile);
			if (_UOSOutputDestination.Length > 260)
			{
				_UOSOutputDestination = FileUtils.GetShortPathName(_UOSOutputDestination);
			}
			_UpdateHistoryDestination = Path.Combine(directoryName, fileNameWithoutExtension + "." + DevicePaths.UpdateHistoryFile);
			if (_UpdateHistoryDestination.Length > 260)
			{
				_UpdateHistoryDestination = FileUtils.GetShortPathName(_UpdateHistoryDestination);
			}
			_PackageListFile = Path.Combine(directoryName, fileNameWithoutExtension + ".PackageList.xml");
			if (_PackageListFile.Length > 260)
			{
				_PackageListFile = FileUtils.GetShortPathName(_PackageListFile);
			}
			int val = Math.Max(_UOSOutputDestination.Length, _UpdateHistoryDestination.Length);
			val = Math.Max(_catalogFile.Length, val);
			val = Math.Max(_outputFile.Length, val);
			if (val >= 260)
			{
				throw new ImageCommonException("Imaging!SetPaths: The Output File and path '" + _outputFile + "' and cannot be longer than " + (260 - (val - 260)) + " characters.");
			}
		}

		public void UpdateExistingImage(string imageFile, string updateInputXML, bool randomizeGptIds)
		{
			_outputFile = imageFile;
			_updateInputFile = updateInputXML;
			_bDoingUpdate = true;
			ProcessImage(randomizeGptIds);
		}

		public void BuildNewImage(string outputFile, string oemInputXML, string msPackageRoot, string oemCustomizationXML, string oemCustomizationPPKG, string oemVersion, bool randomizeGptIds)
		{
			_outputFile = outputFile;
			_oemInputFile = oemInputXML;
			_msPackagesRoot = Path.GetFullPath(msPackageRoot);
			_oemCustomizationXML = oemCustomizationXML;
			_oemCustomizationPPKG = oemCustomizationPPKG;
			if (!string.IsNullOrEmpty(_oemCustomizationXML) || !string.IsNullOrEmpty(_oemCustomizationPPKG))
			{
				if (string.IsNullOrEmpty(oemVersion))
				{
					_logger.LogError("Imaging!BuildNewImage: The OEMVersion must be set if OEMCustomizationXML\\OEMCustomizationPPKG is provided.");
					throw new ImageCommonException("Imaging!BuildNewImage: The OEMVersion must be set if OEMCustomizationXML\\OEMCustomizationPPKG is provided.");
				}
				if (!VersionInfo.TryParse(oemVersion, out _oemCustomizationVersion))
				{
					_logger.LogError("Imaging!BuildNewImage: Provided OEMVersion that was not a valid version string.");
					throw new ImageCommonException("Imaging!BuildNewImage: Provided OEMVersion that was not a valid version string.");
				}
			}
			ProcessImage(randomizeGptIds);
		}

		private void ProcessImage(bool randomizeGptIds)
		{
			try
			{
				ProcessPrivilege.Adjust(PrivilegeNames.RestorePrivilege, true);
				ProcessPrivilege.Adjust(PrivilegeNames.SecurityPrivilege, true);
				_dtStartTime = DateTime.Now;
				SetPaths();
				Guid guid = Guid.NewGuid();
				_telemetryLogger.LogString("ProcessImageStarted", guid, _bDoingUpdate.ToString(CultureInfo.InvariantCulture));
				try
				{
					string location = GetType().Assembly.Location;
					FileInfo fileInfo = new FileInfo(location);
					_telemetryLogger.LogString("BinaryInfo", guid, FileVersionInfo.GetVersionInfo(location).ProductVersion, fileInfo.CreationTime.ToString("yyMMdd-HHmm", CultureInfo.InvariantCulture), fileInfo.LastWriteTime.ToString("yyMMdd-HHmm", CultureInfo.InvariantCulture));
				}
				catch
				{
				}
				try
				{
					if (!SkipUpdateMain)
					{
						if (_bDoingUpdate)
						{
							_logger.LogInfo("Imaging: Reading the update input file...");
							_updateInput = ValidateInput();
							_updateInput.WriteToFile(_updateInputFileGenerated);
						}
						else
						{
							_logger.LogInfo("Imaging: Reading the OEM Input XML file...");
							ValidateInput(ref _oemInput);
							if (_oemInput.Edition == null)
							{
								throw new ImageCommonException("Imaging!ProcessImage: The Product entry in the OEMInput '" + _oemInput.Product + "' is not recognized.");
							}
							_isOneCore = !_oemInput.Edition.IsProduct("Windows Phone") && !_oemInput.Edition.IsProduct("Phone Manufacturing OS") && !_oemInput.Edition.IsProduct("Factory OS") && !_oemInput.Edition.IsProduct("Phone Andromeda OS");
							try
							{
								_releaseType = (ReleaseType)Enum.Parse(typeof(ReleaseType), _oemInput.ReleaseType);
							}
							catch (PackageException)
							{
								throw new ImageCommonException("Imaging!ProcessImage: The ReleaseType '" + _oemInput.ReleaseType + "' in the OEM Input file is not valid.  Please use 'Production' or 'Test'.");
							}
							if (_oemInput.Edition.ReleaseType == ReleaseType.Test && _releaseType == ReleaseType.Production)
							{
								throw new ImageCommonException("Imaging!ProcessImage: The Product entry in the OEMInput '" + _oemInput.Product + "' is a ReleaseType=Test Product and cannot be used to create ReleaseType=Production images as specified in the OEMInput.");
							}
							GenerateInputFile(null);
							GenerateCustomizationContent();
							if (_releaseType != ReleaseType.Production && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("IMAGING_PACKAGES_OVERRIDE")))
							{
								GenerateInputFile(Directory.GetFiles(Environment.GetEnvironmentVariable("IMAGING_PACKAGES_OVERRIDE")).ToList());
							}
							ValidateProductionImage();
						}
					}
					IList<string> partitionsTargeted = _packageInfoList.Select((KeyValuePair<string, IPkgInfo> p) => p.Value.Partition).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
					_swStorageStackTime.Start();
					_storageManagerCommit = new ImageStorageManager(_logger, partitionsTargeted);
					_storageManagerCommit.RandomizeDiskIds = randomizeGptIds;
					_storageManagerCommit.RandomizePartitionIDs = randomizeGptIds;
					_swStorageStackTime.Stop();
					_storageManager = _storageManagerCommit;
					if (SkipImaging)
					{
						_logger.LogInfo("Imaging: OEM Customizations processing complete. Skipping Imaging...");
						return;
					}
					if (_bDoingUpdate)
					{
						LoadImage(randomizeGptIds);
					}
					else if (!string.IsNullOrEmpty(_oemInputFile))
					{
						_deviceLayoutValidator.Initialize(_msCoreFMPackages.FirstOrDefault(), _oemInput, _logger, _tempDirectoryPath);
						ReadDeviceLayout(guid);
						InitializeMinFreeSectors();
						_swStorageStackTime.Start();
						FullFlashUpdateImage.IsGPTPartitionType(_parameters.MainOSStore.Partitions[0].Type);
						_storageManagerStaging = new ImageStorageManager(_logger, partitionsTargeted);
						_storageManagerStaging.IsDesktopImage = false;
						_storageManagerStaging.RandomizeDiskIds = true;
						_storageManagerStaging.RandomizePartitionIDs = true;
						_swStorageStackTime.Stop();
						_storageManagerStaging.VirtualHardDiskSectorSize = _parameters.VirtualHardDiskSectorSize;
						_storageManager = _storageManagerStaging;
						CreateImage(null);
						_tempDirectoryPath = _storageManager.GetPartitionPath(ImageConstants.DATA_PARTITION_NAME) + Process.GetCurrentProcess().Id;
						LongPathDirectory.CreateDirectory(_tempDirectoryPath);
						_tempDirectoryPath = FileUtils.GetShortPathName(_tempDirectoryPath);
						_updateStagingRoot = Path.Combine(_tempDirectoryPath, "USERS\\System\\AppData\\Local\\UpdateStagingRoot");
					}
					else if (!FormatDPP)
					{
						throw new ImageCommonException("Imaging!Run: No Input XML file specified.");
					}
					try
					{
						_hasDPPPartition = !string.IsNullOrEmpty(_storageManager.GetPartitionPath(ImageConstants.DPP_PARTITION_NAME));
					}
					catch
					{
						_hasDPPPartition = false;
					}
					if (FormatDPP && !_hasDPPPartition)
					{
						throw new ImageCommonException("Imaging!ProcessImage: The OEM Input XML specifies FormatDPP but the DeviceLayout does not contain a DPP partition.");
					}
					if (!SkipUpdateMain)
					{
						if (!_bDoingUpdate)
						{
							Environment.SetEnvironmentVariable("WINDOWS_WCP_INSKUASSEMBLY", "1");
						}
						if (_updateInput.PackageFiles.Where((string x) => x.IndexOf(".mobilecore.", StringComparison.OrdinalIgnoreCase) > 0).Count() > 0 && CPUId != CpuId.ARM64 && CPUId != CpuId.AMD64)
						{
							Environment.SetEnvironmentVariable("VALIDATE_REGISTRY_COLLISIONS", "1");
							_logger.LogInfo("Imaging: Registry collision validation is enabled.");
						}
						StageImage();
						if (!_bDoingUpdate)
						{
							_logger.LogInfo("Imaging: Preparing to create new image '{0}'...", _outputFile);
							ReadDeviceLayout();
							ProcessMinFreeSectors();
							_storageManagerCommit.VirtualHardDiskSectorSize = _parameters.VirtualHardDiskSectorSize;
							_storageManager = _storageManagerCommit;
							CreateImage(_outputFile);
							EnforcePartitionRestrictions();
							PrePopulateCommitVolumes();
						}
						CommitImage();
						_telemetryLogger.LogString("ImageStoreCount", guid, _ffuImage.StoreCount.ToString(CultureInfo.InvariantCulture));
						try
						{
							StringBuilder stringBuilder = new StringBuilder();
							foreach (string devicePlatformID in _ffuImage.DevicePlatformIDs)
							{
								stringBuilder.Append(devicePlatformID);
								stringBuilder.Append(";");
							}
							_telemetryLogger.LogString("PlatformIds", guid, stringBuilder.ToString());
						}
						catch
						{
						}
						Environment.SetEnvironmentVariable("VALIDATE_REGISTRY_COLLISIONS", null);
						Environment.SetEnvironmentVariable("WINDOWS_WCP_INSKUASSEMBLY", null);
					}
					if (!_bDoingUpdate)
					{
						ProcessBSPProductNameAndVersion();
						LoadDataAssets();
						ValidateMinFreeSectors();
						CopyPristineHivesForFactoryReset();
					}
					FinalizeImage();
					_logger.LogInfo("Imaging: Image processing complete.");
					Environment.ExitCode = 0;
				}
				catch (ImageCommonException ex2)
				{
					_logger.LogError("{0}", ex2.Message);
					_telemetryLogger.LogString("ImagingFailed", guid, ex2.Message);
					if (ex2.InnerException != null)
					{
						string text = ex2.InnerException.ToString();
						_logger.LogError("\t{0}", text);
						_telemetryLogger.LogString("ImagingException", guid, text);
					}
					Environment.ExitCode = 1;
				}
				catch (Exception ex3)
				{
					_logger.LogError("{0}", ex3.ToString());
					_telemetryLogger.LogString("ImagingFailed", guid, ex3.ToString());
					if (ex3.InnerException != null)
					{
						string text2 = ex3.InnerException.ToString();
						_logger.LogError("\t{0}", text2);
						_telemetryLogger.LogString("ImagingUnhandledException", guid, text2);
					}
					_logger.LogError("An unhandled exception was thrown: {0}", ex3.ToString());
					Environment.ExitCode = 3;
				}
				finally
				{
					if (Environment.ExitCode != 0)
					{
						try
						{
							CleanupHandler(null, null);
						}
						catch (Exception ex4)
						{
							LogUtil.Diagnostic("Ignoring exception during cleanup: " + ex4.Message);
						}
						string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(_outputFile);
						_logger.LogInfo("Imaging: See {0}.cbs.log and {0}.csi.log for details.", fileNameWithoutExtension);
					}
					bool deleteFile = true;
					CleanupStorageManager(_storageManagerStaging, deleteFile);
					if (!string.IsNullOrEmpty(_tempDirectoryPath))
					{
						FileUtils.DeleteTree(_tempDirectoryPath);
					}
					_dtEndTime = DateTime.Now;
					TimeSpan timeSpan = _dtEndTime - _dtStartTime;
					_logger.LogInfo("Imaging: Performance Results:");
					_logger.LogInfo("\tTotal Run Time:\t" + timeSpan);
					_telemetryLogger.LogString("PerformanceTotalTime", guid, timeSpan.ToString());
					if (_swCreateFFUTime.Elapsed != TimeSpan.Zero)
					{
						TimeSpan timeSpan2 = _swReqXMLProcessingTime.Elapsed + _swCreateFFUTime.Elapsed;
						_logger.LogInfo("\tImage Creation Time:\t{0}", timeSpan2);
						_telemetryLogger.LogString("PerformanceImageCreationTime", guid, timeSpan2.ToString());
						_logger.LogDebug("\t\tReading\\Parsing XML Time:\t" + _swReqXMLProcessingTime);
						_logger.LogDebug("\t\tCreate metadata Time:\t" + _swCreateFFUTime.Elapsed);
					}
					TimeSpan timeSpan3 = _swStorageStackTime.Elapsed + _swDismountImageTime.Elapsed + _swMountImageTime.Elapsed;
					_logger.LogInfo("\tStorage Stack Time:\t{0}", timeSpan3);
					_telemetryLogger.LogString("PerformanceStorageStackTime", guid, timeSpan3.ToString());
					_logger.LogDebug("\t\tImage Mount Time:\t" + _swMountImageTime.Elapsed);
					_telemetryLogger.LogString("PerformanceImageMountTime", guid, _swMountImageTime.Elapsed.ToString());
					_logger.LogDebug("\t\tImage Dismount Time:\t" + _swDismountImageTime.Elapsed);
					_telemetryLogger.LogString("PerformanceImageDismountTime", guid, _swDismountImageTime.Elapsed.ToString());
					if (_tsCompDBAnswersTime != TimeSpan.Zero)
					{
						_logger.LogInfo("\tCompDB Answer gathering Time:\t{0}", _tsCompDBAnswersTime);
					}
					if (_tsCompDBTime != TimeSpan.Zero)
					{
						_logger.LogInfo("\tCompDB Total Time:\t{0}", _tsCompDBTime);
					}
					if (_swMutexTime.Elapsed != TimeSpan.Zero)
					{
						_logger.LogInfo("\tWaiting on other Imaging instance Time:\t{0}", _swMutexTime.Elapsed);
						_telemetryLogger.LogString("PerformanceMutexTime", guid, _swMutexTime.Elapsed.ToString());
					}
				}
			}
			finally
			{
				ProcessPrivilege.Adjust(PrivilegeNames.RestorePrivilege, false);
				ProcessPrivilege.Adjust(PrivilegeNames.SecurityPrivilege, false);
			}
		}

		private void PrePopulateCommitVolumes()
		{
			Stopwatch stopwatch = new Stopwatch();
			_logger.LogInfo("Imaging: pre-populating commit volumes");
			stopwatch.Start();
			foreach (InputStore store in _parameters.Stores)
			{
				foreach (InputPartition item in store.Partitions.Where((InputPartition x) => string.Equals(x.FileSystem, "FAT", StringComparison.InvariantCultureIgnoreCase) || string.Equals(x.FileSystem, "NTFS", StringComparison.InvariantCultureIgnoreCase)))
				{
					PrePopulateCommitVolume(item);
				}
			}
			stopwatch.Stop();
			_logger.LogInfo($"Imaging: pre-populating commit volumes completed in {stopwatch.Elapsed.ToString()}...");
		}

		private void PrePopulateCommitVolume(InputPartition partition)
		{
			bool fUseXCopy = true;
			if (_storageManager.MainOSStorage.StoreId.StoreType == ImageConstants.PartitionTypeGpt)
			{
				if (Guid.Parse(partition.Type) != ImageConstants.PARTITION_BASIC_DATA_GUID)
				{
					if (!string.Equals(partition.FileSystem, "FAT", StringComparison.InvariantCultureIgnoreCase))
					{
						throw new ImageCommonException($"Partition {partition.Name} has a non-FAT filesystem {partition.FileSystem}, but lives on a hidden volume, so we can't copy its metadata properly");
					}
					fUseXCopy = false;
				}
			}
			else if (_storageManager.MainOSStorage.StoreId.StoreType != ImageConstants.PartitionTypeMbr)
			{
				throw new ImagingException("Unsupported store type: " + _storageManager.MainOSStorage.StoreId.StoreType);
			}
			_logger.LogInfo("Imaging: Pre-populating component store for '{0}'...", partition.Name);
			string partitionPath = _storageManagerStaging.GetPartitionPath(partition.Name);
			string partitionPath2 = _storageManagerCommit.GetPartitionPath(partition.Name);
			CopyDirectoryTree(Path.Combine(partitionPath, "Windows\\winsxs"), Path.Combine(partitionPath2, "Windows\\winsxs"), fUseXCopy);
			CopyDirectoryTree(Path.Combine(partitionPath, "Windows\\servicing"), Path.Combine(partitionPath2, "Windows\\servicing"), fUseXCopy);
			bool overwrite = true;
			LongPathFile.Copy(Path.Combine(partitionPath, "Windows\\system32\\config\\COMPONENTS"), Path.Combine(partitionPath2, "Windows\\system32\\config\\COMPONENTS"), overwrite);
			LongPathFile.Copy(Path.Combine(partitionPath, "Windows\\system32\\config\\SOFTWARE"), Path.Combine(partitionPath2, "Windows\\system32\\config\\SOFTWARE"), overwrite);
		}

		private void CopyDirectoryTree(string sourcePath, string targetPath, bool fUseXCopy)
		{
			int num = 0;
			if (fUseXCopy)
			{
				try
				{
					num = CommonUtils.RunProcess("xcopy.exe", $"/y /b /q /k /o /e /h \"{sourcePath}\" \"{targetPath}\"");
				}
				catch (Exception innerException)
				{
					throw new ImageCommonException("xcopy.exe failed unexpectedly", innerException);
				}
				if (num != 0)
				{
					throw new ImageCommonException($"xcopy.exe finished with non-zero exit code {num}");
				}
			}
			else
			{
				num = CopyAllFiles(sourcePath, targetPath, true, false);
				if (num != 0)
				{
					throw new ImageCommonException($"CopyAllFiles failed with non-zero exit code {num}");
				}
			}
		}

		private string GetMainOSPath()
		{
			return _storageManager.GetPartitionPath(ImageConstants.MAINOS_PARTITION_NAME);
		}

		private string GetDataPath()
		{
			return _storageManager.GetPartitionPath(ImageConstants.DATA_PARTITION_NAME);
		}

		private string GetUpdateOsWimPath()
		{
			return Path.Combine(GetMainOSPath(), DevicePaths.UpdateOSWIMFilePath);
		}

		private string GetPendingUpdateOSWimPath()
		{
			return Path.Combine(_updateStagingRoot, "Pending_UpdateOS.wim");
		}

		private void ValidateInput(ref OEMInput xmlInput)
		{
			try
			{
				OEMInput.ValidateInput(ref xmlInput, _oemInputFile, _logger, _msPackagesRoot, CPUId.ToString());
			}
			catch (Exception ex)
			{
				throw new ImageCommonException("Imaging!ValidateInput: Failed to load OEM Input XML file " + _oemInputFile + ":" + ex.Message, ex);
			}
			xmlInput.WriteToFile(Path.Combine(_processedFilesDir, "OEMInput.xml"));
			EnsureAllFilesExist(xmlInput.PackageFiles, "OEM input");
		}

		private UpdateOSInput ValidateInput()
		{
			UpdateOSInput updateOSInput = null;
			try
			{
				updateOSInput = UpdateOSInput.ValidateInput(_updateInputFile, _logger);
			}
			catch (Exception ex)
			{
				throw new ImageCommonException("Imaging!ValidateInput: Failed to load update input file file " + _updateInputFile + ":" + ex.Message, ex);
			}
			if (updateOSInput != null)
			{
				EnsureAllFilesExist(updateOSInput.PackageFiles, "update input");
			}
			return updateOSInput;
		}

		private void ValidateInput(ref FeatureManifest xmlInput, string fmFile)
		{
			try
			{
				FeatureManifest.ValidateAndLoad(ref xmlInput, fmFile, _logger);
			}
			catch (Exception ex)
			{
				throw new ImageCommonException("Imaging!ValidateInput: Failed to load Feature Manifest XML file " + fmFile + ":" + ex.Message, ex);
			}
		}

		private void EnsureAllFilesExist(List<string> filesToCheck, string inputType)
		{
			if (filesToCheck == null)
			{
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			foreach (string item in filesToCheck.Where((string x) => !File.Exists(x)))
			{
				stringBuilder.AppendLine("\t'" + item + "'");
			}
			if (stringBuilder.Length == 0)
			{
				return;
			}
			throw new ImageCommonException("Imaging!EnsureAllFilesExist: The " + inputType + " contains the package file(s) that do not exist:" + Environment.NewLine + stringBuilder.ToString());
		}

		private void GenerateInputFile(List<string> packages)
		{
			_updateInput = new UpdateOSInput();
			_updateInput.Description = ((_oemInput != null) ? _oemInput.Description : "Imaging generated update input file");
			_updateInput.DateTime = DateTime.Now.ToString();
			_updateInput.PackageFiles = new List<string>();
			if (packages != null)
			{
				_updateInput.PackageFiles.AddRange(packages);
			}
			else if (_oemInput != null)
			{
				ProcessFMs();
				if (_oemInput.PackageFiles != null)
				{
					_updateInput.PackageFiles.AddRange(_oemInput.PackageFiles);
				}
				if (!string.IsNullOrEmpty(_oemInput.FormatDPP) && !bool.TryParse(_oemInput.FormatDPP, out FormatDPP))
				{
					throw new ImageCommonException("Imaging!GenerateInputFile: The OEM Input XML specifies FormatDPP with an invalid value.  Value must be 'true' or 'false'.");
				}
			}
			_updateInput.WriteToFile(_updateInputFileGenerated);
			LoadPackages();
		}

		private void GenerateCustomizationContent()
		{
			if (string.IsNullOrWhiteSpace(_oemCustomizationXML) && string.IsNullOrWhiteSpace(_oemCustomizationPPKG))
			{
				return;
			}
			bool flag = false;
			Customizations customizationInput = new Customizations
			{
				CustomizationXMLFilePath = _oemCustomizationXML,
				CustomizationPPKGFilePath = _oemCustomizationPPKG,
				ImageCpuType = CPUId,
				ImageBuildType = ((!_oemInput.BuildType.Equals("chk")) ? BuildType.Retail : BuildType.Checked),
				ImageDeviceName = _oemInput.Device,
				ImageVersion = _oemCustomizationVersion,
				ImagePackages = _packageInfoList.Values.ToList(),
				OutputDirectory = Path.GetDirectoryName(_outputFile)
			};
			Customizations.StrictSettingPolicies = StrictSettingPolicies;
			CustomContent customContent = CustomContentGenerator.GenerateCustomContent(customizationInput);
			foreach (CustomizationError item in customContent.CustomizationErrors.Where((CustomizationError x) => x.Severity == CustomizationErrorSeverity.Error))
			{
				flag = true;
				if (item.FilesInvolved != null)
				{
					_logger.LogError("{0} ({1})", item.Message, string.Join(", ", item.FilesInvolved.Select((IDefinedIn x) => x.DefinedInFile)));
				}
				else
				{
					_logger.LogError(item.Message);
				}
			}
			foreach (CustomizationError item2 in customContent.CustomizationErrors.Where((CustomizationError x) => x.Severity == CustomizationErrorSeverity.Warning))
			{
				if (item2.FilesInvolved != null)
				{
					_logger.LogWarning("{0} ({1})", item2.Message, string.Join(", ", item2.FilesInvolved.Select((IDefinedIn x) => x.DefinedInFile)));
				}
				else
				{
					_logger.LogWarning(item2.Message);
				}
			}
			if (flag)
			{
				throw new ImageCommonException("Imaging: Customization package generation failed.");
			}
			_dataAssetFileList.AddRange(customContent.DataContent);
			foreach (string packagePath in customContent.PackagePaths)
			{
				_logger.LogInfo("Imaging: Including Image Customization Package: {0}", Path.GetFileName(packagePath));
				IPkgInfo value = Package.LoadFromCab(packagePath);
				_updateInput.PackageFiles.Add(packagePath);
				_packageInfoList.Add(packagePath, value);
			}
			_updateInput.WriteToFile(_updateInputFileGenerated);
		}

		private void ValidateProductionImage()
		{
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder();
			bool flag = false;
			if (_bDoingUpdate || _releaseType != ReleaseType.Production)
			{
				return;
			}
			string text = Path.Combine(_tempDirectoryPath, "temp.cat");
			stringBuilder.Clear();
			stringBuilder2.Clear();
			if (!string.Equals(_oemInput.BuildType, OEMInput.BuildType_FRE, StringComparison.OrdinalIgnoreCase))
			{
				throw new ImageCommonException("Imaging!ValidateProductionImage: The BuildType '" + _oemInput.BuildType + "' in the OEM Input file is not valid.  Please use 'fre' for Retail images.");
			}
			if (_oemInput.PackageFiles != null && _oemInput.PackageFiles.Count() > 0)
			{
				throw new ImageCommonException("Imaging!ValidateProductionImage: The Retail images cannot use the PackageFiles section of the OEMInput.");
			}
			foreach (IPkgInfo value in _packageInfoList.Values)
			{
				if (value.ReleaseType != _releaseType || value.BuildType != BuildType.Retail)
				{
					stringBuilder.AppendLine("\t'" + value.Name + "'");
				}
				IEnumerable<IFileEntry> source = value.Files.Where((IFileEntry file) => file.FileType == FileType.Catalog);
				if (source.Count() == 0)
				{
					_logger.LogWarning("This package has no queryable catalog: " + value.Name);
					continue;
				}
				IFileEntry fileEntry = source.First();
				bool overwriteExistingFiles = true;
				value.ExtractFile(fileEntry.DevicePath, text, overwriteExistingFiles);
				if (!ImageSigner.HasSignature(text, value.OwnerType == OwnerType.Microsoft))
				{
					stringBuilder2.AppendLine(string.Concat("\t'", value.Name, "': (", value.OwnerType, ")"));
				}
				LongPathFile.Delete(text);
			}
			if (stringBuilder.Length != 0)
			{
				if (_bDoingFFU)
				{
					flag = true;
					_logger.LogError("Imaging: The OEM Input XML combined with the Feature Manifest contains the following non-production package(s) while the OEM Input specifies this is a 'Production' image:" + Environment.NewLine + stringBuilder.ToString());
				}
				else
				{
					_logger.LogInfo("Imaging: The OEM Input XML combined with the Feature Manifest contains the following non-production package(s) while the OEM Input specifies this is a 'Production' image:" + Environment.NewLine + stringBuilder.ToString());
				}
			}
			if (stringBuilder2.Length != 0)
			{
				if (_bDoingFFU)
				{
					flag = true;
					_logger.LogError("Imaging: The OEM Input XML combined with the Feature Manifest contains the following improperly signed package(s) while the OEM Input specifies this is a 'Production' image:" + Environment.NewLine + stringBuilder2.ToString());
				}
				else
				{
					_logger.LogInfo("Imaging: The OEM Input XML combined with the Feature Manifest contains the following improperly signed package(s) while the OEM Input specifies this is a 'Production' image:" + Environment.NewLine + stringBuilder2.ToString());
				}
			}
			if (!flag)
			{
				return;
			}
			throw new ImageCommonException("Imaging: Production image validation failed.");
		}

		private void ProcessFMs()
		{
			if (_oemInput.AdditionalFMs != null)
			{
				foreach (string additionalFM in _oemInput.AdditionalFMs)
				{
					_logger.LogInfo("Imaging: Reading the FM XML file '" + additionalFM + "'...");
					ProcessFMEntries(additionalFM);
				}
			}
			_logger.LogInfo("Imaging: Reading the Feature Manifest XML file...");
			string text = _oemInput.BuildType;
			if (string.IsNullOrWhiteSpace(text))
			{
				text = Environment.GetEnvironmentVariable("BuildType");
			}
			foreach (EditionPackage coreFeatureManifestPackage in _oemInput.Edition.CoreFeatureManifestPackages)
			{
				string packagePath = coreFeatureManifestPackage.GetPackagePath(_msPackagesRoot, CPUId.ToString(), text);
				packagePath = _oemInput.ProcessOEMInputVariables(packagePath);
				if (!File.Exists(packagePath))
				{
					packagePath = Path.ChangeExtension(packagePath, PkgConstants.c_strCBSPackageExtension);
					if (!File.Exists(packagePath))
					{
						throw new ImageCommonException("Imaging!ProcessFMs: Failed to find the Feature Manifest package '" + Path.ChangeExtension(packagePath, "") + "*'.  Unable to create image.");
					}
				}
				IPkgInfo pkgInfo;
				try
				{
					pkgInfo = Package.LoadFromCab(packagePath);
					_msCoreFMPackages.Add(pkgInfo);
					_osVersion = pkgInfo.Version.ToString();
				}
				catch (IUException ex)
				{
					throw new ImageCommonException("Imaging!ProcessFMs: Failed to load the Feature Manifest package '" + packagePath + "' due to the following error: " + ex.Message, ex);
				}
				if (pkgInfo.OwnerType != OwnerType.Microsoft)
				{
					throw new ImageCommonException("Imaging!ProcessFMs: The Feature Manifest package '" + packagePath + "' must be a Microsoft owned and signed package.");
				}
				string fMDevicePath = coreFeatureManifestPackage.FMDevicePath;
				IFileEntry fileEntry = pkgInfo.FindFile(fMDevicePath);
				if (fileEntry == null)
				{
					throw new ImageCommonException("Imaging!ProcessFMs: Failed to find the Feature Manifest xml '" + fMDevicePath + "'.");
				}
				string text2 = _tempDirectoryPath + fileEntry.DevicePath;
				pkgInfo.ExtractFile(fileEntry.DevicePath, text2, true);
				ProcessFMEntries(text2, true);
				LongPathFile.Delete(text2);
			}
		}

		private FeatureManifest ProcessFMEntries(string fmFileXML, bool coreFM = false)
		{
			FeatureManifest xmlInput = null;
			fmFileXML = Environment.ExpandEnvironmentVariables(fmFileXML);
			ValidateInput(ref xmlInput, fmFileXML);
			xmlInput.OemInput = _oemInput;
			if (coreFM)
			{
				if (string.IsNullOrEmpty(_buildID))
				{
					_buildID = xmlInput.BuildID;
				}
				if (string.IsNullOrEmpty(_buildInfo))
				{
					_buildInfo = xmlInput.BuildInfo;
				}
			}
			if (xmlInput.Features != null && xmlInput.Features.MSConditionalFeatures != null)
			{
				foreach (FMConditionalFeature mSConditionalFeature in xmlInput.Features.MSConditionalFeatures)
				{
					if (mSConditionalFeature.GetAllConditions().FirstOrDefault((Condition cond) => cond.Type == Condition.ConditionType.Registry) != null)
					{
						_condFeatures.Add(mSConditionalFeature);
					}
				}
			}
			FeatureManifest featureManifest = new FeatureManifest(xmlInput);
			featureManifest.ProcessVariables();
			string fileName = Path.GetFileName(fmFileXML);
			featureManifest.WriteToFile(Path.Combine(_processedFilesDir, fileName));
			List<FeatureManifest.FMPkgInfo> list = new List<FeatureManifest.FMPkgInfo>();
			list = xmlInput.GetFilteredPackagesByGroups();
			List<string> list2 = list.Select((FeatureManifest.FMPkgInfo pkg) => pkg.PackagePath).Distinct().ToList();
			EnsureAllFilesExist(list2, "OEM input + feature manifest");
			ProcessCompDBPackages(list, xmlInput, fileName);
			_updateInput.PackageFiles.AddRange(list2);
			if (string.IsNullOrEmpty(_oemDevicePlatformPackagePath))
			{
				string oEMDevicePlatformPackage = xmlInput.GetOEMDevicePlatformPackage(_oemInput.Device);
				if (!string.IsNullOrEmpty(oEMDevicePlatformPackage) && GetPackageFile(oEMDevicePlatformPackage, _oemDevicePlatformDevicePath) != null)
				{
					_oemDevicePlatformPackagePath = oEMDevicePlatformPackage;
				}
				if (string.IsNullOrEmpty(_oemDevicePlatformPackagePath) && list.Any((FeatureManifest.FMPkgInfo pkg) => pkg.FMGroup == FeatureManifest.PackageGroups.DEVICE))
				{
					foreach (FeatureManifest.FMPkgInfo item in list.FindAll((FeatureManifest.FMPkgInfo pkg) => pkg.FMGroup == FeatureManifest.PackageGroups.DEVICE && pkg.Partition.Equals(ImageConstants.MAINOS_PARTITION_NAME, StringComparison.OrdinalIgnoreCase)))
					{
						if (GetPackageFile(item.PackagePath, _oemDevicePlatformDevicePath) != null)
						{
							_oemDevicePlatformPackagePath = item.PackagePath;
							break;
						}
					}
				}
			}
			string deviceLayoutPackage = xmlInput.GetDeviceLayoutPackage(_oemInput.SOC);
			if (!string.IsNullOrEmpty(deviceLayoutPackage))
			{
				if (GetPackageFile(deviceLayoutPackage, _deviceLayoutDevicePath) != null)
				{
					if (!string.IsNullOrEmpty(_deviceLayoutPackagePath))
					{
						throw new ImageCommonException("Imaging!ProcessFMEntries: The OEM Input XML combined with the Feature Manifest files contains more than one definition for the Device Layout package.");
					}
					_deviceLayoutPackagePath = deviceLayoutPackage;
				}
				if (string.IsNullOrEmpty(_deviceLayoutPackagePath) && list.Any((FeatureManifest.FMPkgInfo pkg) => pkg.FMGroup == FeatureManifest.PackageGroups.SOC))
				{
					foreach (FeatureManifest.FMPkgInfo item2 in list.FindAll((FeatureManifest.FMPkgInfo pkg) => pkg.FMGroup == FeatureManifest.PackageGroups.SOC && pkg.Partition.Equals(ImageConstants.MAINOS_PARTITION_NAME, StringComparison.OrdinalIgnoreCase)))
					{
						if (GetPackageFile(item2.PackagePath, _deviceLayoutDevicePath) != null)
						{
							_deviceLayoutPackagePath = item2.PackagePath;
							return xmlInput;
						}
					}
					return xmlInput;
				}
			}
			return xmlInput;
		}

		private void ProcessCompDBPackages(List<FeatureManifest.FMPkgInfo> packages, FeatureManifest fm, string fmFilename)
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			OwnerType ownerType = ((fm.OwnerType == OwnerType.Microsoft) ? fm.OwnerType : OwnerType.OEM);
			List<CompDBPackageInfo> list = new List<CompDBPackageInfo>();
			List<CompDBFeature> list2 = new List<CompDBFeature>();
			foreach (FeatureManifest.FMPkgInfo item3 in packages.Where((FeatureManifest.FMPkgInfo pkg) => pkg.FMGroup == FeatureManifest.PackageGroups.OEMDEVICEPLATFORM))
			{
				item3.FMGroup = FeatureManifest.PackageGroups.DEVICE;
			}
			foreach (FeatureManifest.FMPkgInfo item4 in packages.Where((FeatureManifest.FMPkgInfo pkg) => pkg.FMGroup == FeatureManifest.PackageGroups.DEVICELAYOUT))
			{
				item4.FMGroup = FeatureManifest.PackageGroups.SOC;
			}
			Func<FeatureManifest.FMPkgInfo, bool> func = default(Func<FeatureManifest.FMPkgInfo, bool>);
			foreach (string feature in packages.Select((FeatureManifest.FMPkgInfo fminfo) => fminfo.FeatureID).Distinct())
			{
				CompDBFeature compDBFeature = new CompDBFeature(feature, fm.ID, CompDBFeature.CompDBFeatureTypes.MobileFeature, fm.OwnerType.ToString());
				Func<FeatureManifest.FMPkgInfo, bool> func2 = func;
				if (func2 == null)
				{
					func2 = (func = (FeatureManifest.FMPkgInfo pkg) => pkg.FeatureID.Equals(feature, StringComparison.OrdinalIgnoreCase));
				}
				foreach (FeatureManifest.FMPkgInfo item5 in packages.Where(func2))
				{
					CompDBFeaturePackage item = new CompDBFeaturePackage(item5.ID, item5.FeatureIdentifierPackage);
					compDBFeature.Packages.Add(item);
					CompDBPackageInfo item2 = new CompDBPackageInfo(item5, fm, fmFilename, _msPackagesRoot, _deviceCompDB, true, false);
					list.Add(item2);
				}
				list2.Add(compDBFeature);
			}
			list = list.Distinct().ToList();
			_deviceCompDB.Features.AddRange(list2);
			_deviceCompDB.Packages.AddRange(list.Select((CompDBPackageInfo pkg) => new CompDBPackageInfo(pkg).ClearPackageHashes()));
			_deviceCompDB.Languages = _oemInput.SupportedLanguages.UserInterface.Select((string lang) => new CompDBLanguage(lang)).ToList();
			_deviceCompDB.Resolutions = _oemInput.Resolutions.Select((string res) => new CompDBResolution(res)).ToList();
			if (ownerType == OwnerType.OEM)
			{
				_bspCompDB.Features.AddRange(list2);
				List<CompDBPackageInfo> source = new List<CompDBPackageInfo>(list);
				source = source.Select((CompDBPackageInfo pkg) => pkg.SetParentDB(_bspCompDB)).ToList();
				_bspCompDB.Packages.AddRange(source);
			}
			stopwatch.Stop();
			_tsCompDBTime += stopwatch.Elapsed;
		}

		private List<Hashtable> GetRegistryTable(List<string> packages)
		{
			List<Hashtable> list = new List<Hashtable>();
			foreach (string package in packages)
			{
				try
				{
					Hashtable hashtable = Package.LoadRegistry(package);
					if (hashtable.Count != 0)
					{
						list.Add(hashtable);
					}
				}
				catch
				{
				}
			}
			return list;
		}

		private void WriteCompDBs()
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			Stopwatch stopwatch2 = new Stopwatch();
			_deviceCompDB.ReleaseType = (_bspCompDB.ReleaseType = _releaseType);
			_deviceCompDB.Product = (_bspCompDB.Product = _oemInput.Edition.InternalProductDir);
			_deviceCompDB.BuildInfo = (_bspCompDB.BuildInfo = _buildInfo);
			Guid buildID = (string.IsNullOrEmpty(_buildID) ? Guid.NewGuid() : new Guid(_buildID));
			_deviceCompDB.BuildID = (_bspCompDB.BuildID = buildID);
			stopwatch2.Start();
			List<string> packages = (from pkg in _packageInfoList
				where pkg.Value.OwnerType != OwnerType.Microsoft
				select pkg into pkg2
				select pkg2.Key).ToList();
			List<Hashtable> list = new List<Hashtable>(GetRegistryTable(packages));
			List<Hashtable> list2 = new List<Hashtable>();
			if (_condFeatures.Count() > 0)
			{
				List<string> packages2 = (from pkg in _packageInfoList
					where pkg.Value.OwnerType == OwnerType.Microsoft
					select pkg into pkg2
					select pkg2.Key).ToList();
				list2 = GetRegistryTable(packages2);
				list.AddRange(list2);
			}
			if (_condFeatures.Count() > 0)
			{
				DeviceConditionAnswers deviceConditionAnswers = new DeviceConditionAnswers(_logger);
				deviceConditionAnswers.PopulateConditionAnswers(_condFeatures, list);
				if (deviceConditionAnswers.Conditions != null)
				{
					_deviceCompDB.ConditionAnswers = deviceConditionAnswers;
				}
			}
			stopwatch2.Stop();
			_bspCompDB.OSVersion = (_deviceCompDB.OSVersion = _osVersion);
			_bspCompDB.WriteToFile(_bspDBFile, true);
			_deviceCompDB.WriteToFile(_deviceDBFile);
			_tsCompDBTime += stopwatch.Elapsed;
			_tsCompDBAnswersTime += stopwatch2.Elapsed;
		}

		private void LoadImage(bool randomizeGptIds)
		{
			try
			{
				AcquireMutex(_logger);
				if (_bDoingFFU)
				{
					try
					{
						_logger.LogInfo("Imaging: Loading exsiting FFU file '{0}'...", _outputFile);
						_ffuImage = new FullFlashUpdateImage();
						_ffuImage.Initialize(_outputFile);
						_imgSigner = new ImageSigner();
						_imgSigner.Initialize(_ffuImage, _catalogFile, _logger);
					}
					catch (Exception innerException)
					{
						throw new ImageCommonException("Imaging!LoadImage: Failed while loading the FFU image " + _outputFile + " with :", innerException);
					}
					try
					{
						_logger.LogInfo("Imaging: Verifying the existing FFU image...");
						_imgSigner.VerifyCatalog();
					}
					catch (Exception innerException2)
					{
						throw new ImageCommonException("Imaging!LoadImage: The FFU " + _outputFile + " has been tampered with outside ImageApp and is no longer usable.", innerException2);
					}
					try
					{
						_logger.LogInfo("Imaging: Mounting FFU file '{0}'...", _outputFile);
						_swMountImageTime.Start();
						_storageManager.MountFullFlashImage(_ffuImage, randomizeGptIds);
						_swMountImageTime.Stop();
					}
					catch (Exception innerException3)
					{
						throw new ImageCommonException("Imaging!LoadImage: Failed to mount FFU '" + _outputFile + "' :", innerException3);
					}
				}
				else
				{
					try
					{
						_logger.LogInfo("Imaging: Loading existing VHD file '{0}'...", _outputFile);
						_swMountImageTime.Start();
						bool readOnly = false;
						_storageManager.MountExistingVirtualHardDisk(_outputFile, readOnly);
						_swMountImageTime.Stop();
					}
					catch (Exception innerException4)
					{
						throw new ImageCommonException("Imaging!LoadImage: Failed to mount VHD '" + _outputFile + "' :", innerException4);
					}
				}
				_logger.LogInfo("Imaging: {0} file '{1}' loaded.", _outputType, _outputFile);
			}
			finally
			{
				ReleaseMutex();
			}
		}

		private void ReadDeviceLayout(Guid? telemetrySessionId = null)
		{
			string empty = string.Empty;
			string empty2 = string.Empty;
			_parameters = new ImageGeneratorParameters();
			string text = SaveToTempXMLFiles();
			_logger.LogInfo("Imaging: Found required files '{0}' and '{1}'...", "DeviceLayout.xml", "OEMDevicePlatform.xml");
			empty = Path.Combine(text, "DeviceLayout.xml");
			if (telemetrySessionId.HasValue)
			{
				_telemetryLogger.LogString("IsDeviceLayoutV2", telemetrySessionId.Value, ImageGeneratorParameters.IsDeviceLayoutV2(empty).ToString(CultureInfo.InvariantCulture));
			}
			empty2 = Path.Combine(text, "OEMDevicePlatform.xml");
			_swReqXMLProcessingTime.Start();
			_parameters.Initialize(_logger);
			_logger.LogInfo("Imaging: Processing Device Layout XML files...");
			_parameters.ProcessInputXML(empty, empty2);
			_swReqXMLProcessingTime.Stop();
			FileUtils.DeleteTree(text);
		}

		private void CreateImage(string imagePath)
		{
			AcquireMutex(_logger);
			try
			{
				ImageGenerator imageGenerator = new ImageGenerator();
				_swCreateFFUTime.Start();
				imageGenerator.Initialize(_parameters, _logger);
				_logger.LogInfo("Imaging: Creating initial image...");
				_ffuImage = imageGenerator.CreateFFU();
				_swCreateFFUTime.Stop();
				_ffuImage.OSVersion = _osVersion;
				_ffuImage.AntiTheftVersion = "1.1";
				_logger.LogInfo("Imaging: Mounting {0}, {1}, {2} ...", _outputType, _ffuImage, imagePath);
				_swMountImageTime.Start();
				if (_bDoingFFU)
				{
					_storageManager.CreateFullFlashImage(_ffuImage);
				}
				else
				{
					bool preparePartitions = true;
					_storageManager.CreateVirtualHardDisk(_ffuImage.Stores[0], imagePath, ImageConstants.PartitionTypeMbr, preparePartitions);
				}
				string mainOSPath = GetMainOSPath();
				foreach (InputStore store in _parameters.Stores)
				{
					foreach (InputPartition item in store.Partitions.Where((InputPartition x) => string.Equals(x.FileSystem, "FAT", StringComparison.InvariantCultureIgnoreCase) || string.Equals(x.FileSystem, "NTFS", StringComparison.InvariantCultureIgnoreCase)))
					{
						_logger.LogInfo("Imaging: Creating Windows layout for partition '{0}'...", item.Name);
						CreateWindowsInPartition(_storageManager.GetPartitionPath(item.Name), string.Compare(item.Name, ImageConstants.SYSTEM_PARTITION_NAME, StringComparison.OrdinalIgnoreCase) == 0);
						if (!item.Compressed)
						{
							continue;
						}
						if (!string.Equals(item.FileSystem, "NTFS", StringComparison.InvariantCultureIgnoreCase))
						{
							throw new ImageCommonException("Partition " + item.Name + " is marked compressed, but its filesystem isn't NTFS. Compressed is only supported on NTFS partitions. Please fix the OEMDeviceLayout (FileSystem) and/or OEMDevicePlatform (UncompressedPartitions) to only specify compression on NTFS partitions.");
						}
						_logger.LogInfo("Imaging: Attaching WOF to partition '{0}'...", item.Name);
						try
						{
							_storageManager.AttachWOFToVolume(item.Name);
						}
						catch
						{
							if (_releaseType == ReleaseType.Production)
							{
								_logger.LogError("Moblie image compression is on by default. This image is configured for compression, but is likely running < Windows 10 without the WOFADK compression driver installed. Please see the documentation regarding image compression.");
								throw;
							}
							_logger.LogWarning("Imaging: Unable to attach WOF to partition '{0}', continuing without compression due to non-production image...", item.Name);
							continue;
						}
						_logger.LogInfo("Imaging: Marking partition '{0}' as Compact...", item.Name);
						string text = Path.Combine(_storageManager.GetPartitionPath(item.Name), "Windows\\system32\\config\\SYSTEM");
						using (ORRegistryKey oRRegistryKey = ORRegistryKey.OpenHive(text))
						{
							using (ORRegistryKey oRRegistryKey2 = oRRegistryKey.CreateSubKey("Setup"))
							{
								oRRegistryKey2.SetValue("Compact", 1);
								oRRegistryKey.SaveHive(text);
							}
						}
					}
				}
				string text2 = Path.Combine(mainOSPath, "Windows\\ImageUpdate", "OEMInput.xml");
				LongPathDirectory.CreateDirectory(Path.GetDirectoryName(text2));
				LongPathFile.Copy(_oemInputFile, text2, true);
				_swMountImageTime.Stop();
			}
			catch (ImageCommonException)
			{
				throw;
			}
			catch (Exception ex2)
			{
				throw new ImageCommonException("Imaging!CreateImage: Failed to create " + _outputType + ": " + ex2.Message, ex2);
			}
			finally
			{
				ReleaseMutex();
			}
		}

		private void CreateWindowsInPartition(string root, bool fEFIESP)
		{
			int num = 0;
			UIntPtr InitCookie = UIntPtr.Zero;
			UIntPtr pMalloc = UIntPtr.Zero;
			UIntPtr zero = UIntPtr.Zero;
			UIntPtr zero2 = UIntPtr.Zero;
			uint num2 = 0x2Au | (_isOneCore ? 64u : 0u);
			uint pdwDisposition = 0u;
			uint num3 = 0u;
			if (fEFIESP)
			{
				num2 |= 0x10u;
				if (new List<string> { "VM", "VM64", "VMGen1", "C3PC" }.Contains(_oemInput.Device, StringComparer.OrdinalIgnoreCase))
				{
					num2 |= 0x80u;
				}
			}
			switch (CPUId)
			{
			case CpuId.X86:
				num3 = 0u;
				break;
			case CpuId.AMD64:
				num3 = 9u;
				if ((num2 & 0x40) == 0)
				{
					num2 &= 0xFFFFFFF7u;
				}
				break;
			case CpuId.ARM:
				num3 = 5u;
				break;
			case CpuId.ARM64:
				num3 = 12u;
				if ((num2 & 0x40) == 0)
				{
					num2 &= 0xFFFFFFF7u;
				}
				break;
			default:
				throw new ImageCommonException($"Unsupported CPUId {(uint)CPUId} encountered.");
			}
			UpdateMain.OFFLINE_STORE_CREATION_PARAMETERS pParameters = default(UpdateMain.OFFLINE_STORE_CREATION_PARAMETERS);
			pParameters.cbSize = (UIntPtr)(ulong)Marshal.SizeOf((object)pParameters);
			pParameters.dwFlags = 0u;
			pParameters.ulProcessorArchitecture = num3;
			pParameters.pszHostSystemDrivePath = root;
			string szSystemDrive = "c:";
			if (UpdateMain.FAILED(num = UpdateMain.WcpInitialize(out InitCookie)))
			{
				throw new ImageCommonException("Imaging!UpdateImage: Failed call WcpInitialize with error code: " + string.Format("{0} (0x{0:X})", num));
			}
			if (UpdateMain.FAILED(num = UpdateMain.CoGetMalloc(1u, out pMalloc)))
			{
				throw new ImageCommonException("Imaging!UpdateImage: Failed call CoGetMalloc with error code: " + string.Format("{0} (0x{0:X})", num));
			}
			if (UpdateMain.FAILED(num = UpdateMain.SetIsolationIMalloc(pMalloc)))
			{
				throw new ImageCommonException("Imaging!UpdateImage: Failed call SetIsolationIMalloc with error code: " + string.Format("{0} (0x{0:X})", num));
			}
			if (UpdateMain.FAILED(num = UpdateMain.CreateNewWindows(num2, szSystemDrive, ref pParameters, zero, out pdwDisposition)))
			{
				throw new ImageCommonException("Imaging!UpdateImage: Failed call CreateNewWindows with error code: " + string.Format("{0} (0x{0:X})", num));
			}
			UpdateMain.WcpShutdown(InitCookie);
		}

		private void StageImage()
		{
			int num = 0;
			using (UpdateMain updateMain = new UpdateMain())
			{
				try
				{
					if (!_bDoingUpdate)
					{
						LongPathDirectory.CreateDirectory(Path.GetDirectoryName(GetUpdateOsWimPath()));
						using (WindowsImageContainer windowsImageContainer = new WindowsImageContainer(GetUpdateOsWimPath(), WindowsImageContainer.CreateFileMode.CreateAlways, WindowsImageContainer.CreateFileAccess.Write, WindowsImageContainer.CreateFileCompression.WIM_COMPRESS_LZX))
						{
							string text = Path.Combine(_tempDirectoryPath, "UpdateOSWim");
							LongPathDirectory.CreateDirectory(text);
							FileUtils.CleanDirectory(text);
							CreateWindowsInPartition(text, false);
							windowsImageContainer.CaptureImage(text);
							windowsImageContainer.SetBootImage(windowsImageContainer.ImageCount);
							FileUtils.DeleteTree(text);
						}
					}
					ImageStructures.STORE_ID[] array = new ImageStructures.STORE_ID[_storageManager.Storages.Count];
					for (int i = 0; i < _storageManager.Storages.Count; i++)
					{
						array[i] = _storageManager.Storages[i].StoreId;
					}
					if (UpdateMain.FAILED(num = updateMain.Initialize(array.Length, array, _updateInputFileGenerated, _tempDirectoryPath, ErrorLogger, WarningLogger, InformationLogger, DebugLogger)))
					{
						throw new ImageCommonException("Imaging!UpdateImage: Failed to Initialize UpdateDLL::UpdateMain with error code: " + string.Format("{0} (0x{0:X})", num));
					}
					_logger.LogInfo("Imaging: Staging the image...");
					if (UpdateMain.FAILED(num = updateMain.PrepareUpdate()))
					{
						throw new ImageCommonException("Imaging!UpdateImage: Failed call to UpdateDLL::PrepareUpdate with error code: " + string.Format("{0} (0x{0:X})", num));
					}
				}
				catch (ImageCommonException)
				{
					throw;
				}
				catch (Exception ex2)
				{
					throw new ImageCommonException("Imaging!UpdateImage: Failed to stage " + _outputType + ": " + ex2.Message, ex2);
				}
			}
		}

		private void CommitImage()
		{
			int num = 0;
			using (UpdateMain updateMain = new UpdateMain())
			{
				DateTime now = DateTime.Now;
				bool overwrite = true;
				string pendingUpdateOSWimPath = GetPendingUpdateOSWimPath();
				if (LongPathFile.Exists(GetPendingUpdateOSWimPath()))
				{
					string updateOsWimPath = GetUpdateOsWimPath();
					LongPathDirectory.CreateDirectory(Path.GetDirectoryName(updateOsWimPath));
					LongPathFile.Copy(pendingUpdateOSWimPath, updateOsWimPath, overwrite);
				}
				ImageStructures.STORE_ID[] array = new ImageStructures.STORE_ID[_storageManager.Storages.Count];
				for (int i = 0; i < _storageManager.Storages.Count; i++)
				{
					array[i] = _storageManager.Storages[i].StoreId;
				}
				if (UpdateMain.FAILED(num = updateMain.Initialize(array.Length, array, _updateInputFileGenerated, _tempDirectoryPath, ErrorLogger, WarningLogger, InformationLogger, DebugLogger)))
				{
					throw new ImageCommonException("Imaging!UpdateImage: Failed to Initialize UpdateDLL::UpdateMain with error code: " + string.Format("{0} (0x{0:X})", num));
				}
				_logger.LogInfo("Imaging: Committing the image...");
				if (UpdateMain.FAILED(num = updateMain.ExecuteUpdate()))
				{
					throw new ImageCommonException("Imaging!UpdateImage: Failed call to UpdateDLL::ExecuteUpdate with error code: " + string.Format("{0} (0x{0:X})", num));
				}
				_logger.LogInfo("Imaging: Update completed...");
				string path = Path.Combine(GetMainOSPath(), "Users\\default\\ntuser.dat");
				DirectorySecurity accessControl = Directory.GetAccessControl(path);
				accessControl.SetAccessRuleProtection(false, true);
				Directory.SetAccessControl(path, accessControl);
				_logger.LogInfo("Imaging: Saving Update XML files...");
				LongPathFile.Copy(Path.Combine(GetDataPath(), DevicePaths.UpdateOutputFilePath), _UOSOutputDestination, overwrite);
				_updateHistoryFile = Path.Combine(GetMainOSPath(), DevicePaths.UpdateHistoryFilePath);
				LongPathFile.Copy(_updateHistoryFile, _UpdateHistoryDestination, overwrite);
				GetPackageListFromUpdateHistory();
			}
		}

		private void ProcessBSPProductNameAndVersion()
		{
			if (_bDoingUpdate)
			{
				return;
			}
			string mainOSPath = GetMainOSPath();
			string text = "";
			string text2 = BuildCompDB.GetProductNamePrefix(_oemInput.Product);
			if (!string.IsNullOrEmpty(BSPProductName))
			{
				text2 += BSPProductName;
			}
			string text3 = Path.Combine(mainOSPath, "Windows\\system32\\config\\SOFTWARE");
			string tempFile = FileUtils.GetTempFile(_tempDirectoryPath);
			LongPathFile.Copy(text3, tempFile);
			using (ORRegistryKey oRRegistryKey = ORRegistryKey.OpenHive(tempFile))
			{
				using (ORRegistryKey oRRegistryKey2 = oRRegistryKey.CreateSubKey("Microsoft\\Windows NT\\CurrentVersion\\Update\\TargetingInfo\\Overrides\\BSP"))
				{
					if (!string.IsNullOrEmpty(BSPProductName))
					{
						_logger.LogInfo("Imaging: Writing BSP Product Name to the registry Overrides: '{0}'", text2);
						oRRegistryKey2.SetValue("Name", text2);
						oRRegistryKey.SaveHive(text3);
					}
					try
					{
						text = oRRegistryKey2.GetStringValue("Version");
					}
					catch
					{
						text = "";
					}
				}
			}
			LongPathFile.Delete(tempFile);
			if (string.IsNullOrEmpty(BSPProductName) || string.IsNullOrEmpty(text))
			{
				text3 = Path.Combine(mainOSPath, "Windows\\system32\\config\\SYSTEM");
				using (ORRegistryKey oRRegistryKey3 = ORRegistryKey.OpenHive(text3))
				{
					using (ORRegistryKey oRRegistryKey4 = oRRegistryKey3.CreateSubKey("Platform\\DeviceTargetingInfo"))
					{
						if (string.IsNullOrEmpty(BSPProductName))
						{
							try
							{
								string stringValue = oRRegistryKey4.GetStringValue("PhoneManufacturer");
								string stringValue2 = oRRegistryKey4.GetStringValue("PhoneHardwareVariant");
								text2 = text2 + stringValue + "." + stringValue2;
							}
							catch
							{
							}
						}
						if (string.IsNullOrEmpty(text))
						{
							try
							{
								text = oRRegistryKey4.GetStringValue("PhoneFirmwareRevision");
							}
							catch
							{
							}
						}
					}
				}
			}
			_bspCompDB.BSPVersion = text;
			_bspCompDB.BSPProductName = text2;
		}

		private void GetPackageListFromUpdateHistory()
		{
			if (!File.Exists(_UpdateHistoryDestination))
			{
				throw new ImageCommonException("Imaging!GetPackageListFromUpdateHistory: Unable to find History file: " + _UpdateHistoryDestination);
			}
			UpdateHistory.ValidateUpdateHistory(_UpdateHistoryDestination, _logger).GetPackageList().WriteToFile(_PackageListFile);
		}

		private void LoadDataAssets()
		{
			_logger.LogInfo("Imaging: Copying Data Partition Assets...");
			foreach (KeyValuePair<string, string> dataAssetFile in _dataAssetFileList)
			{
				string text = Path.Combine(GetDataPath(), dataAssetFile.Value);
				LongPathDirectory.CreateDirectory(Path.GetDirectoryName(text));
				LongPathFile.Copy(dataAssetFile.Key, text);
			}
			_logger.LogInfo("Imaging:   {0} Data Asset Files Copied.", _dataAssetFileList.Count);
			if (_oemInput != null && _oemInput.UserStoreMapData != null)
			{
				if (!Directory.Exists(_oemInput.UserStoreMapData.SourceDir))
				{
					throw new ImageCommonException("Imaging!LoadMapData: The source directory for the User Store map data does not exist: " + _oemInput.UserStoreMapData.SourceDir);
				}
				char[] trimChars = new char[1] { '\\' };
				string destination = Path.Combine(GetDataPath(), _oemInput.UserStoreMapData.UserStoreDir.TrimStart(trimChars));
				FileUtils.CopyDirectory(_oemInput.UserStoreMapData.SourceDir, destination);
			}
		}

		private void FinalizeImage()
		{
			_logger.LogInfo("Imaging: Finalizing the {0} image...", _outputType);
			try
			{
				AcquireMutex(_logger);
				if (_hasDPPPartition)
				{
					_storageManager.WaitForVolume(ImageConstants.DPP_PARTITION_NAME);
				}
				_storageManager.WaitForVolume(ImageConstants.MAINOS_PARTITION_NAME);
				if (FormatDPP)
				{
					_logger.LogInfo("Imaging: Formatting the DPP partition.");
					_swStorageStackTime.Start();
					InputPartition inputPartition = null;
					foreach (InputStore store in _parameters.Stores)
					{
						inputPartition = store.Partitions.FirstOrDefault((InputPartition x) => string.Equals(x.Name, ImageConstants.DPP_PARTITION_NAME, StringComparison.InvariantCultureIgnoreCase));
						if (inputPartition != null)
						{
							break;
						}
					}
					if (inputPartition == null)
					{
						throw new ImageCommonException("Imaging!FinalizeImage: DPP partition is not present.");
					}
					if (string.IsNullOrEmpty(inputPartition.FileSystem))
					{
						inputPartition.FileSystem = "FAT";
					}
					_storageManager.FormatPartition(inputPartition.Name, inputPartition.FileSystem, inputPartition.ClusterSize);
					if (_storageManager.MainOSStorage.StoreId.StoreType == ImageConstants.PartitionTypeGpt)
					{
						_storageManager.SetPartitionType(inputPartition.Name, ImageConstants.PARTITION_BASIC_DATA_GUID);
					}
					else
					{
						_storageManager.SetPartitionType(inputPartition.Name, 11);
					}
					_swStorageStackTime.Stop();
				}
				WriteCompDBs();
				if (_bDoingFFU)
				{
					UpdateUsedSectors();
					_ffuImage.Description = GetUpdateDescription(_updateHistoryFile);
					OutputWrapper innerWrapper = new OutputWrapper(_outputFile);
					SecurityWrapper securityWrapper = new SecurityWrapper(_ffuImage, innerWrapper);
					ManifestWrapper payloadWrapper = new ManifestWrapper(_ffuImage, securityWrapper);
					_swDismountImageTime.Start();
					uint storeHeaderVersion = ((_parameters == null) ? ((!ImageGeneratorParameters.IsDeviceLayoutV2(Path.Combine(GetMainOSPath(), DevicePaths.ImageUpdatePath, "DeviceLayout.xml"))) ? 1u : 2u) : _parameters.DeviceLayoutVersion);
					bool saveChanges = true;
					bool deleteFile = true;
					_storageManager.DismountFullFlashImage(saveChanges, payloadWrapper, deleteFile, storeHeaderVersion);
					_swDismountImageTime.Stop();
					LongPathFile.WriteAllBytes(Path.ChangeExtension(_outputFile, ".cat"), securityWrapper.CatalogData);
				}
				else
				{
					_swDismountImageTime.Start();
					_storageManager.DismountVirtualHardDisk(false, false, true);
					_swDismountImageTime.Stop();
				}
			}
			catch (ImageCommonException)
			{
				throw;
			}
			catch (Exception ex2)
			{
				throw new ImageCommonException("Imaging!FinalizeImage: Failed to finalize the " + _outputType + ": " + ex2.Message, ex2);
			}
			finally
			{
				ReleaseMutex();
			}
		}

		private void UpdateUsedSectors()
		{
			if (_ffuImage == null)
			{
				return;
			}
			_storageManager.FlushVolumesForDismount();
			foreach (FullFlashUpdateImage.FullFlashUpdateStore store in _ffuImage.Stores)
			{
				foreach (FullFlashUpdateImage.FullFlashUpdatePartition partition in store.Partitions)
				{
					if (!_storageManager.PartitionIsMountedRaw(partition.Name))
					{
						try
						{
							_storageManager.WaitForVolume(partition.Name);
							ulong freeBytesOnVolume = _storageManager.GetFreeBytesOnVolume(partition.Name);
							partition.SectorsInUse = partition.TotalSectors - (uint)Math.Ceiling((double)freeBytesOnVolume / (double)store.SectorSize);
						}
						catch (Exception ex)
						{
							throw new ImageCommonException("Imaging!UpdateUsedSectors: Failed to calculate free space on partition '" + partition.Name + "' : " + ex.Message, ex);
						}
					}
				}
			}
		}

		private void ErrorLogger(string errorStr)
		{
			_logger.LogError("{0}", errorStr);
		}

		private void WarningLogger(string warnStr)
		{
			_logger.LogWarning("{0}", warnStr);
		}

		private void InformationLogger(string infoStr)
		{
			_logger.LogInfo("{0}", infoStr);
		}

		private void DebugLogger(string debugStr)
		{
			_logger.LogDebug("{0}", debugStr);
		}

		private void LoadPackages()
		{
			StringBuilder stringBuilder = new StringBuilder();
			_packageInfoList.Clear();
			foreach (string packageFile in _updateInput.PackageFiles)
			{
				try
				{
					IPkgInfo value = Package.LoadFromCab(packageFile);
					_packageInfoList.Add(packageFile, value);
				}
				catch (IUException ex)
				{
					stringBuilder.AppendLine("Imaging!LoadPackages: Unable to load package '" + packageFile + "' due to the following error: " + ex.ToString());
				}
			}
			if (stringBuilder.Length != 0)
			{
				throw new ImageCommonException(stringBuilder.ToString());
			}
		}

		private IFileEntry GetPackageFile(string packagePath, string deviceFile)
		{
			IPkgInfo pkgInfo = null;
			try
			{
				pkgInfo = Package.LoadFromCab(packagePath);
			}
			catch (PackageException ex)
			{
				throw new PackageException(ex, "Imaging!GetPackageFile: Failed to load '" + packagePath + "' package: " + ex.Message);
			}
			return GetPackageFile(pkgInfo, deviceFile);
		}

		private IFileEntry GetPackageFile(IPkgInfo pkgInfo, string deviceFile)
		{
			IFileEntry result = null;
			if (pkgInfo != null)
			{
				result = pkgInfo.FindFile(deviceFile);
			}
			return result;
		}

		private string SaveToTempXMLFiles()
		{
			string text = Path.Combine(_tempDirectoryPath, "XMLConfig");
			bool flag = false;
			bool flag2 = false;
			LongPathDirectory.CreateDirectory(text);
			if (_releaseType != ReleaseType.Production && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("IMAGING_DEVICELAYOUT_PACKAGE")))
			{
				_deviceLayoutPackagePath = Environment.GetEnvironmentVariable("IMAGING_DEVICELAYOUT_PACKAGE");
			}
			if (_releaseType != ReleaseType.Production && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("IMAGING_DEVICEPLATFORM_PACKAGE")))
			{
				_oemDevicePlatformPackagePath = Environment.GetEnvironmentVariable("IMAGING_DEVICEPLATFORM_PACKAGE");
			}
			if (string.IsNullOrEmpty(_oemDevicePlatformPackagePath))
			{
				throw new PackageException("Imaging!SaveToTempXMLFiles: Unable to create image without OEMDevicePlatform.xml.  No OEMDevicePlatform package could be found in the Additional FMs.");
			}
			if (!File.Exists(_oemDevicePlatformPackagePath))
			{
				throw new PackageException("Imaging!SaveToTempXMLFiles: Unable to create image without OEMDevicePlatform.xml.  The specified package cound not be found: '" + _oemDevicePlatformPackagePath + "'");
			}
			IPkgInfo pkgInfo;
			try
			{
				pkgInfo = Package.LoadFromCab(_oemDevicePlatformPackagePath);
			}
			catch (PackageException ex)
			{
				throw new PackageException(ex, "Imaging!SaveToTempXMLFiles: Failed to load '" + _oemDevicePlatformPackagePath + "' package: " + ex.Message);
			}
			if (!_isOneCore && pkgInfo.OwnerType != OwnerType.OEM)
			{
				throw new PackageException("Imaging!SaveToTempXMLFiles: The OEMDevicePlatform.xml must be contained in and OEM owned package.  '" + _oemDevicePlatformPackagePath + "' package specified is not.");
			}
			IFileEntry packageFile = GetPackageFile(pkgInfo, _oemDevicePlatformDevicePath);
			if (packageFile != null)
			{
				pkgInfo.ExtractFile(packageFile.DevicePath, Path.Combine(text, "OEMDevicePlatform.xml"), true);
				flag2 = true;
			}
			if (string.IsNullOrEmpty(_deviceLayoutPackagePath) || !File.Exists(_deviceLayoutPackagePath))
			{
				throw new PackageException("Imaging!SaveToTempXMLFiles: Unable to create image without DeviceLayout.xml.  The specified device '" + _oemInput.Device + "' does not have an associated DeviceLayout in the Feature Manifest.");
			}
			IPkgInfo pkgInfo2;
			try
			{
				pkgInfo2 = Package.LoadFromCab(_deviceLayoutPackagePath);
			}
			catch (PackageException ex2)
			{
				throw new PackageException(ex2, "Imaging!SaveToTempXMLFiles: Failed to load '" + _deviceLayoutPackagePath + "' package: " + ex2.Message);
			}
			packageFile = GetPackageFile(pkgInfo2, _deviceLayoutDevicePath);
			if (packageFile != null)
			{
				string text2 = Path.Combine(text, "DeviceLayout.xml");
				pkgInfo2.ExtractFile(packageFile.DevicePath, text2, true);
				_deviceLayoutValidator.ValidateDeviceLayout(_msCoreFMPackages.FirstOrDefault(), pkgInfo2, _deviceLayoutPackagePath, text2);
				flag = true;
			}
			if (!flag || !flag2)
			{
				string text3 = "ImageApp: Unable to create image without file(s):";
				FileUtils.DeleteTree(text);
				text = null;
				if (!flag)
				{
					text3 = text3 + Environment.NewLine + "DeviceLayout.xml";
				}
				if (!flag2)
				{
					text3 = text3 + Environment.NewLine + "OEMDevicePlatform.xml";
				}
				throw new ImageCommonException(text3);
			}
			return text;
		}

		private void CleanupStorageManager(ImageStorageManager storageManager, bool deleteFile)
		{
			if (storageManager != null)
			{
				if (_bDoingFFU)
				{
					storageManager.DismountFullFlashImage(false);
				}
				else
				{
					storageManager.DismountVirtualHardDisk(deleteFile, deleteFile);
				}
			}
		}

		public void CleanupHandler(object sender, ConsoleCancelEventArgs args)
		{
			try
			{
				_swStorageStackTime.Start();
				bool deleteFile = true;
				CleanupStorageManager(_storageManagerStaging, deleteFile);
				deleteFile = !_bDoingUpdate;
				CleanupStorageManager(_storageManagerCommit, deleteFile);
				_swStorageStackTime.Stop();
				if (!string.IsNullOrEmpty(_tempDirectoryPath))
				{
					FileUtils.DeleteTree(_tempDirectoryPath);
				}
				Environment.SetEnvironmentVariable("WINDOWS_WCP_INSKUASSEMBLY", null);
				Environment.SetEnvironmentVariable("COMPONENT_BASED_SERVICING_LOGFILE", null);
				Environment.SetEnvironmentVariable("WINDOWS_TRACING_FLAGS", null);
				Environment.SetEnvironmentVariable("WINDOWS_TRACING_LOGFILE", null);
			}
			catch (Exception ex)
			{
				LogUtil.Diagnostic("Ignoring exception during cleanup: " + ex.ToString());
			}
			if (args != null)
			{
				Environment.Exit(1);
			}
		}

		private string GetUpdateDescription(string updateHistoryFile)
		{
			string text = string.Empty;
			UpdateHistory updateHistory = null;
			try
			{
				updateHistory = UpdateHistory.ValidateUpdateHistory(updateHistoryFile, _logger);
			}
			catch
			{
			}
			if (updateHistory != null && updateHistory.UpdateEvents != null)
			{
				foreach (UpdateEvent updateEvent in updateHistory.UpdateEvents)
				{
					if (!string.IsNullOrWhiteSpace(updateEvent.Summary))
					{
						text += updateEvent.Summary.Replace("\n", "\r\n", StringComparison.OrdinalIgnoreCase);
						text += Environment.NewLine;
					}
					else if (!string.IsNullOrWhiteSpace(updateEvent.UpdateResults.Description))
					{
						text += updateEvent.UpdateResults.Description.Replace("\n", "\r\n", StringComparison.OrdinalIgnoreCase);
						text += Environment.NewLine;
					}
					else
					{
						text = text + "Update on: " + updateEvent.DateTime;
						text += Environment.NewLine;
					}
				}
				return text;
			}
			return text;
		}

		public void AcquireMutex(IULogger logger)
		{
			_imageAppMutex = new Mutex(false, "Global\\VHDMutex_{585b0806-2d3b-4226-b259-9c8d3b237d5c}");
			if (_imageAppMutex.WaitOne(0))
			{
				return;
			}
			logger.LogInfo("Imaging - Another imaging tool is currently running.  Waiting for it to complete before continuing....");
			_swMutexTime.Start();
			bool flag = false;
			try
			{
				for (int i = 0; i < 80; i++)
				{
					if (!_imageAppMutex.WaitOne(MutexTimeout))
					{
						if ((i + 1) % 6 == 0)
						{
							logger.LogInfo("Imaging - Still waiting for other imaging tools to complete. Current wait time: {0}:{1} minute(s)", _swMutexTime.Elapsed.Minutes, _swMutexTime.Elapsed.Seconds);
						}
						continue;
					}
					flag = true;
					logger.LogInfo("Imaging - Mutex acquired.");
					break;
				}
			}
			catch (AbandonedMutexException)
			{
				flag = true;
			}
			_swMutexTime.Stop();
			if (!flag)
			{
				_imageAppMutex = null;
				throw new ImageCommonException("Imaging: Failed to acquire VHD Mutex (timeout)");
			}
		}

		public void ReleaseMutex()
		{
			if (_imageAppMutex != null)
			{
				_imageAppMutex.ReleaseMutex();
				_imageAppMutex = null;
			}
		}

		private void ValidateMinFreeSectors()
		{
			if (_parameters == null)
			{
				return;
			}
			foreach (InputPartition item in _parameters.MainOSStore.Partitions.Where((InputPartition x) => x.MinFreeSectors != 0))
			{
				_logger.LogInfo("Imaging: Validating MinFreeSectors for partition '{0}'...", item.Name);
				_storageManager.WaitForVolume(item.Name);
				uint num = (uint)(_storageManager.GetFreeBytesOnVolume(item.Name) / _parameters.SectorSize);
				uint num2 = (uint)Math.Abs((int)(item.MinFreeSectors - num));
				string format = string.Format("Imaging!ValidateMinFreeSectors: Partition '{0}' requested {1} minimum free sectors, {2} actual free sectors were found (difference of {3} sectors ({4} MB)).", item.Name, item.MinFreeSectors, num, num2, (num2 * _parameters.SectorSize / 1024u / 1024u).ToString("F"));
				if (num < item.MinFreeSectors)
				{
					if (ReleaseType.Production == (ReleaseType)Enum.Parse(typeof(ReleaseType), _oemInput.ReleaseType))
					{
						_logger.LogError(format);
					}
					else
					{
						_logger.LogWarning(format);
					}
				}
				else
				{
					_logger.LogInfo(format);
				}
			}
		}

		private ulong GetFileSystemOverhead(InputPartition partition, string stagedRoot)
		{
			ulong num = 0uL;
			_storageManager.WaitForVolume(partition.Name);
			if (Directory.Exists(stagedRoot))
			{
				uint num2 = (uint)LongPathDirectory.GetDirectories(stagedRoot, "*", SearchOption.AllDirectories).Length;
				num += num2 * partition.ClusterSize;
			}
			if (string.Equals(partition.FileSystem, "NTFS", StringComparison.InvariantCultureIgnoreCase))
			{
				_storageManager.CreateUsnJournal(partition.Name);
			}
			ulong num3 = _storageManager.GetPartitionSize(partition.Name) * _parameters.SectorSize;
			num3 -= _storageManager.GetFreeBytesOnVolume(partition.Name);
			return num + num3;
		}

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		private static extern int IU_GetDirectorySize(string folder, bool recursive, uint clusterSize, out ulong folderSize);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern uint IU_GetClusterSize(string folder);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
		private static extern int CopyAllFiles(string source, string dest, bool recursive, bool mirror);

		public static ulong AlignUp(ulong value, ulong boundary)
		{
			ulong num = value + boundary - 1;
			return num - num % boundary;
		}

		private void ProcessMinFreeSectors()
		{
			if (_parameters == null)
			{
				throw new ImageCommonException("Imaging!ProcessMinFreeSectors: Incorrectly called before reading the DeviceLayout package.");
			}
			_logger.LogInfo("Imaging: Processing MinFreeSectors...");
			foreach (InputPartition item in _parameters.MainOSStore.Partitions.Where((InputPartition x) => x.MinFreeSectors != 0))
			{
				_storageManager.WaitForVolume(item.Name);
				string text = Path.Combine(_updateStagingRoot, item.Name);
				ulong folderSize = 0uL;
				ulong num = 0uL;
				uint num2 = item.ClusterSize;
				uint sectorSize = _parameters.SectorSize;
				bool num3 = item.Name.Equals(ImageConstants.MAINOS_PARTITION_NAME, StringComparison.OrdinalIgnoreCase);
				if (num2 == 0)
				{
					num2 = (item.ClusterSize = IU_GetClusterSize(_storageManager.GetPartitionPath(item.Name)));
				}
				if (Directory.Exists(text))
				{
					LongPathFile.Delete(Path.Combine(text, "ReservedSpace"));
					if (UpdateMain.FAILED(IU_GetDirectorySize(text, true, num2, out folderSize)))
					{
						throw new ImageCommonException($"Failed to get directory size for staged folder '{text}'");
					}
				}
				string pendingUpdateOSWimPath = GetPendingUpdateOSWimPath();
				if (num3 && LongPathFile.Exists(pendingUpdateOSWimPath))
				{
					uint num5 = (uint)new FileInfo(pendingUpdateOSWimPath).Length;
					folderSize += AlignUp(num5, num2);
				}
				num = GetFileSystemOverhead(item, text);
				uint num6 = (uint)Math.Ceiling((double)(AlignUp(folderSize, num2) + AlignUp(num, num2)) / (double)_parameters.SectorSize);
				uint num7 = num2 / sectorSize;
				num6 = (uint)AlignUp(num6, num7);
				item.MinFreeSectors = (uint)AlignUp(item.MinFreeSectors, num7);
				item.GeneratedFileOverheadSectors = (uint)AlignUp(item.GeneratedFileOverheadSectors, num7);
				item.TotalSectors = num6 + item.MinFreeSectors + item.GeneratedFileOverheadSectors;
				if (num3)
				{
					item.TotalSectors += (uint)((double)item.TotalSectors * 0.04);
				}
				item.TotalSectors = (uint)AlignUp(item.TotalSectors, MBToSectors(1uL));
				_logger.LogInfo($"\tResized partition '{item.Name}' to {item.TotalSectors} sectors ({(ulong)((long)item.TotalSectors * (long)sectorSize) / 1024uL / 1024uL} MB, {(ulong)((double)item.TotalSectors * (double)sectorSize / (double)num2)} clusters)");
			}
		}

		private void EnforcePartitionRestrictions()
		{
			InputPartition inputPartition = _parameters.MainOSStore.Partitions.FirstOrDefault((InputPartition x) => x.Name.Equals(ImageConstants.MAINOS_PARTITION_NAME, StringComparison.OrdinalIgnoreCase));
			if (_parameters.MinSectorCount <= 5368709120uL / (ulong)_parameters.SectorSize && !inputPartition.Compressed)
			{
				throw new ImagingException("The MainOS partition is not marked as compressed, but the platform has less than 5 GB of space.  Please enable compression.");
			}
			uint num = 1717986918u;
			if (_oemInput.Edition.MinimumUserStoreSize != 0)
			{
				num = _oemInput.Edition.MinimumUserStoreSize;
			}
			ulong partitionSize = _storageManager.GetPartitionSize(ImageConstants.DATA_PARTITION_NAME);
			if (partitionSize < num / _parameters.SectorSize && ReleaseType.Production == (ReleaseType)Enum.Parse(typeof(ReleaseType), _oemInput.ReleaseType))
			{
				throw new ImageCommonException($"The user store should be at least {num} bytes in size, but is only {partitionSize * _parameters.SectorSize}.  Please reduce the size of other partitions in the image or increase the MinSectorCount to ensure users will have a sufficient amount of space.");
			}
			IEnumerable<InputPartition> source = _parameters.MainOSStore.Partitions.Where((InputPartition x) => x.RequiresCompression && x.ClusterSize > 4096);
			if (source.Count() > 0)
			{
				string arg = "{ " + string.Join(", ", source.Select((InputPartition x) => x.Name).ToArray()) + " }";
				throw new ImageCommonException($"Partitions {arg} require compression, but have invalid (non-4k) cluster sizes.  Please change the layout to use 4k sectors or remove the compression requirement.");
			}
			IEnumerable<InputPartition> source2 = _parameters.MainOSStore.Partitions.Where((InputPartition x) => x.RequiresCompression && !x.Compressed);
			if (source2.Count() > 0)
			{
				string arg2 = "{ " + string.Join(", ", source2.Select((InputPartition x) => x.Name).ToArray()) + " }";
				throw new ImagingException($"Partitions {arg2} require compression, but are not marked as compressed.  Please compress those partitions that require it, or switch layouts to remove compression requirements.");
			}
		}

		private uint MBToSectors(ulong RequestedMB)
		{
			return (uint)(RequestedMB * 1048576 / _parameters.SectorSize);
		}

		private void InitializeMinFreeSectors()
		{
			if (_parameters == null)
			{
				throw new ImageCommonException("Imaging!ProcessMinFreeSectors: Incorrectly called before reading the DeviceLayout package.");
			}
			_parameters.MinSectorCount = MBToSectors(102400uL);
			foreach (InputPartition item in _parameters.MainOSStore.Partitions.Where((InputPartition x) => x.MinFreeSectors != 0))
			{
				item.TotalSectors = (uint)Math.Ceiling((double)(item.MinFreeSectors + item.GeneratedFileOverheadSectors) * 3.5);
				if (!string.Equals(item.Name, ImageConstants.MAINOS_PARTITION_NAME, StringComparison.OrdinalIgnoreCase))
				{
					item.TotalSectors += MBToSectors(1500uL);
				}
			}
		}

		private void CopyPristineHivesForFactoryReset()
		{
			_logger.LogInfo("ImageApp: Copying pristine hives for factory reset...");
			List<string> list = new List<string> { "SOFTWARE", "SYSTEM", "DRIVERS", "SAM", "SECURITY", "..\\..\\..\\USERS\\DEFAULT\\NTUSER.DAT", "DEFAULT", "COMPONENTS" };
			string path = "windows\\system32\\config";
			string sourceDir = Path.Combine(GetMainOSPath(), path);
			string cabPath = Path.Combine(GetDataPath(), "Windows\\ImageUpdate\\ImagingHives\\ImagingHives.cab");
			foreach (string item in list)
			{
				string text = Path.Combine(sourceDir, item);
				int num = RegValidator.ValidateRegistryHive(text);
				if (num != 0)
				{
					throw new IUException("Registry hive validation failed for path '{0}', err '0x{1:X8}'", text, num);
				}
			}
			CabArchiver cab = new CabArchiver();
			list.ForEach(delegate(string x)
			{
				cab.AddFile(Path.GetFileName(x), Path.Combine(sourceDir, x));
			});
			cab.Save(cabPath, CompressionType.FastLZX);
		}
	}
}
