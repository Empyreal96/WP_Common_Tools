using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon
{
	public class DrvStore : IDisposable
	{
		internal static class DriverStoreInterop
		{
			public delegate bool EnumFilesDelegate(IntPtr driverPackageHandle, IntPtr pDriverFile, IntPtr lParam);

			[DllImport("kernel32", SetLastError = true)]
			internal static extern IntPtr LoadLibrary(string lpFileName);

			[DllImport("kernel32.dll", SetLastError = true)]
			internal static extern bool FreeLibrary(IntPtr hModule);

			[DllImport("drvstore.dll", CharSet = CharSet.Unicode, EntryPoint = "DriverStoreOpenW", SetLastError = true)]
			internal static extern IntPtr DriverStoreOpen(string targetSystemPath, string targetBootDrive, DriverStoreOpenFlag Flags, IntPtr transactionHandle);

			[DllImport("drvstore.dll", SetLastError = true)]
			internal static extern bool DriverStoreClose(IntPtr driverStoreHandle);

			[DllImport("drvstore.dll", CharSet = CharSet.Unicode, EntryPoint = "DriverStoreImportW", SetLastError = true)]
			internal static extern uint DriverStoreImport(IntPtr driverStoreHandle, string driverPackageFileName, ProcessorArchitecture ProcessorArchitecture, string localeName, DriverStoreImportFlag flags, StringBuilder driverStoreFileName, int driverStoreFileNameSize);

			[DllImport("drvstore.dll", CharSet = CharSet.Unicode, EntryPoint = "DriverStoreOfflineAddDriverPackageW", SetLastError = true)]
			internal static extern uint DriverStoreOfflineAddDriverPackage(string DriverPackageInfPath, DriverStoreOfflineAddDriverPackageFlags Flags, IntPtr Reserved, ushort ProcessorArchitecture, string LocaleName, StringBuilder DestInfPath, ref int cchDestInfPath, string TargetSystemRoot, string TargetSystemDrive);

			[DllImport("drvstore.dll", CharSet = CharSet.Unicode, EntryPoint = "DriverStoreConfigureW", SetLastError = true)]
			internal static extern uint DriverStoreConfigure(IntPtr hDriverStore, string DriverStoreFilename, DriverStoreConfigureFlags Flags, string SourceFilter, string TargetFilter);

			[DllImport("drvstore.dll", CharSet = CharSet.Unicode, EntryPoint = "DriverStoreReflectCriticalW", SetLastError = true)]
			internal static extern uint DriverStoreReflectCritical(IntPtr driverStoreHandle, string driverStoreFileName, DriverStoreReflectCriticalFlag flag, string filterDeviceId);

			[DllImport("drvstore.dll", CharSet = CharSet.Unicode, EntryPoint = "DriverStoreReflectW", SetLastError = true)]
			internal static extern uint DriverStoreReflect(IntPtr driverStoreHandle, string driverStoreFileName, DriverStoreReflectFlag flag, string filterSectionNames);

			[DllImport("drvstore.dll", CharSet = CharSet.Unicode, EntryPoint = "DriverStorePublishW", SetLastError = true)]
			internal static extern uint DriverStorePublish(IntPtr driverStoreHandle, string driverStoreFileName, DriverStorePublishFlag flag, StringBuilder publishedFileName, int publishedFileNameSize, ref bool isPublishedFileNameChanged);

			[DllImport("drvstore.dll", CharSet = CharSet.Unicode, EntryPoint = "DriverStoreSetObjectPropertyW", SetLastError = true)]
			internal static extern bool DriverStoreSetObjectProperty(IntPtr driverStoreHandle, DriverStoreObjectType objectType, string objectName, ref DevPropKey propertyKey, DevPropType propertyType, ref uint propertyBuffer, int propertySize, DriverStoreSetObjectPropertyFlag flag);

			[DllImport("drvstore.dll", CharSet = CharSet.Unicode, SetLastError = true)]
			internal static extern bool DriverPackageEnumFilesW(IntPtr driverPackageHandle, IntPtr enumContext, DriverPackageEnumFilesFlag flags, EnumFilesDelegate callbackRoutine, IntPtr lParam);

			[DllImport("drvstore.dll", CharSet = CharSet.Unicode, EntryPoint = "DriverPackageOpenW", SetLastError = true)]
			internal static extern IntPtr DriverPackageOpen(string driverPackageFilename, ProcessorArchitecture processorArchitecture, string localeName, DriverPackageOpenFlag flags, IntPtr resolveContext);

			[DllImport("drvstore.dll", CharSet = CharSet.Unicode, SetLastError = true)]
			internal static extern void DriverPackageClose(IntPtr driverPackageHandle);
		}

		private const int MAX_PATH = 260;

		private static object syncRoot = new object();

		private IntPtr _hDrvStoreModule = IntPtr.Zero;

		private IntPtr _hDrvStore = IntPtr.Zero;

		private bool _isInitialized;

		private string _stagingRootDirectory;

		private string _stagingSystemDirectory;

		private string _targetBootDrive;

		private const string STR_DRVSTORE_DLL = "drvstore.dll";

		public const uint DriverDatabaseConfigOptionsOneCore = 51u;

		public string HivePath => Path.Combine(_stagingSystemDirectory, "System32", "config");

		public string ImportLogPath => Path.Combine(_stagingSystemDirectory, "INF", "setupapi.offline.log");

		public DrvStore(string stagingPath, string targetBootDrive)
		{
			_stagingRootDirectory = Environment.ExpandEnvironmentVariables(stagingPath);
			_stagingSystemDirectory = Path.Combine(_stagingRootDirectory, "windows");
			_targetBootDrive = targetBootDrive;
		}

		~DrvStore()
		{
			Dispose(false);
		}

		private void Initialize()
		{
			string directoryName = LongPath.GetDirectoryName(Assembly.GetEntryAssembly().Location);
			string text = Path.Combine(directoryName, "drvstore.dll");
			if (!LongPathFile.Exists(text))
			{
				throw new PkgGenException("Could not find {0} in folder {1}", "drvstore.dll", directoryName);
			}
			_hDrvStoreModule = DriverStoreInterop.LoadLibrary(text);
			if (_hDrvStoreModule == IntPtr.Zero)
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				LogUtil.Error("Loadlibrary failed on {0}", text);
				throw new Win32Exception(lastWin32Error);
			}
			_isInitialized = true;
		}

		public void Create()
		{
			if (!_isInitialized)
			{
				Initialize();
			}
			int num = 0;
			LogUtil.Diagnostic("Creating driver store at {0}", _stagingSystemDirectory);
			LongPathDirectory.CreateDirectory(_stagingSystemDirectory);
			if (_hDrvStore != IntPtr.Zero)
			{
				LogUtil.Diagnostic("Attempting to open a driver store that was not closed {0} ", _hDrvStore.ToString());
				Close();
			}
			_hDrvStore = DriverStoreInterop.DriverStoreOpen(_stagingSystemDirectory, _targetBootDrive, DriverStoreOpenFlag.Create, IntPtr.Zero);
			if (_hDrvStore == IntPtr.Zero)
			{
				num = Marshal.GetLastWin32Error();
				LogUtil.Error("DriverStoreOpen failed error 0x{0:X8}", num);
				throw new Win32Exception(num);
			}
			LogUtil.Diagnostic("DriverStoreOpen {0} ", _hDrvStore.ToString());
		}

		public void SetupConfigOptions(uint configOptions)
		{
			DevPropKey propertyKey = default(DevPropKey);
			propertyKey.fmtid = new Guid("8163eb00-142c-4f7a-94e1-a274cc47dbba");
			propertyKey.pid = 16u;
			LogUtil.Diagnostic("Setting DriverStore ConfigOptions to 0x{0:X}", configOptions);
			if (!DriverStoreInterop.DriverStoreSetObjectProperty(_hDrvStore, DriverStoreObjectType.DriverDatabase, "SYSTEM", ref propertyKey, DevPropType.DevPropTypeUint32, ref configOptions, 4, DriverStoreSetObjectPropertyFlag.None))
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				LogUtil.Error("DriverStoreSetObjectProperty failed error 0x{0:X8}", lastWin32Error);
				throw new Win32Exception(lastWin32Error);
			}
		}

		private bool IncludeFileCallback(IntPtr driverPackageHandle, IntPtr pDriverFile, IntPtr lParam)
		{
			Marshal.WriteInt32(lParam, 1);
			return false;
		}

		public bool DriverIncludesInfs(string infFile, CpuId cpuId)
		{
			int val = 0;
			IntPtr intPtr = Marshal.AllocHGlobal(4);
			IntPtr intPtr2 = DriverStoreInterop.DriverPackageOpen(infFile, GetProcessArchitectureFromCpuId(cpuId), null, DriverPackageOpenFlag.FilesOnly, IntPtr.Zero);
			if (intPtr2 == IntPtr.Zero)
			{
				LogUtil.Warning("Failed to determine if INF includes other INFs, assuming yes");
				return true;
			}
			Marshal.WriteInt32(intPtr, val);
			DriverStoreInterop.DriverPackageEnumFilesW(intPtr2, IntPtr.Zero, DriverPackageEnumFilesFlag.IncludeInfs, IncludeFileCallback, intPtr);
			val = Marshal.ReadInt32(intPtr);
			DriverStoreInterop.DriverPackageClose(intPtr2);
			Marshal.FreeHGlobal(intPtr);
			LogUtil.Diagnostic("INF includes other INFs: {0}", val);
			return val != 0;
		}

		private ProcessorArchitecture GetProcessArchitectureFromCpuId(CpuId cpuId)
		{
			switch (cpuId)
			{
			case CpuId.ARM:
				return ProcessorArchitecture.PROCESSOR_ARCHITECTURE_ARM;
			case CpuId.X86:
				return ProcessorArchitecture.PROCESSOR_ARCHITECTURE_INTEL;
			case CpuId.ARM64:
				return ProcessorArchitecture.PROCESSOR_ARCHITECTURE_ARM64;
			case CpuId.AMD64:
				return ProcessorArchitecture.PROCESSOR_ARCHITECTURE_AMD64;
			default:
				throw new PkgGenException("Unexpected CPU type '{0}'", cpuId);
			}
		}

		public void ImportDriver(string infPath, string[] referencePaths, string[] stagingSubdirs, CpuId cpuId)
		{
			LogUtil.Diagnostic("Importing driver {0} into store {1}", infPath, _stagingRootDirectory);
			if (_hDrvStore == IntPtr.Zero)
			{
				throw new InvalidOperationException("The driver store has not been created");
			}
			if (string.IsNullOrEmpty(infPath))
			{
				throw new ArgumentNullException("infPath");
			}
			string text = Path.Combine(_stagingRootDirectory, "import");
			LongPathDirectory.CreateDirectory(text);
			string driverPackageFileName = CopyToDirectory(infPath, text);
			if (referencePaths != null)
			{
				for (int i = 0; i < referencePaths.Length; i++)
				{
					string text2 = (string.IsNullOrEmpty(stagingSubdirs[i]) ? text : Path.Combine(text, stagingSubdirs[i]));
					LongPathDirectory.CreateDirectory(text2);
					CopyToDirectory(referencePaths[i], text2);
				}
			}
			StringBuilder stringBuilder = new StringBuilder(260);
			uint num = 0u;
			ProcessorArchitecture processArchitectureFromCpuId = GetProcessArchitectureFromCpuId(cpuId);
			DriverStoreImportFlag flags = DriverStoreImportFlag.SkipTempCopy | DriverStoreImportFlag.SkipExternalFileCheck | DriverStoreImportFlag.Inbox | DriverStoreImportFlag.SystemDefaultLocale;
			num = DriverStoreInterop.DriverStoreImport(_hDrvStore, driverPackageFileName, processArchitectureFromCpuId, null, flags, stringBuilder, stringBuilder.Capacity);
			if (num != 0)
			{
				LogUtil.Error("DriverStoreImport failed error 0x{0:X8}", num);
				throw new Win32Exception((int)num);
			}
			LogUtil.Diagnostic("Driverstore INF path: {0}", stringBuilder);
			LogUtil.Diagnostic("Publishing driver");
			StringBuilder stringBuilder2 = new StringBuilder(260);
			bool isPublishedFileNameChanged = false;
			num = DriverStoreInterop.DriverStorePublish(_hDrvStore, stringBuilder.ToString(), DriverStorePublishFlag.None, stringBuilder2, stringBuilder2.Capacity, ref isPublishedFileNameChanged);
			if (num != 0)
			{
				LogUtil.Error("DriverStorePublish failed error 0x{0:X8}", num);
				throw new Win32Exception((int)num);
			}
			LogUtil.Diagnostic("Published INF path: {0}", stringBuilder2);
			DriverStoreReflectCriticalFlag flag = DriverStoreReflectCriticalFlag.Force | DriverStoreReflectCriticalFlag.Configurations;
			num = DriverStoreInterop.DriverStoreReflectCritical(_hDrvStore, stringBuilder.ToString(), flag, null);
			if (num != 0)
			{
				LogUtil.Error("DriverStoreReflectCritical failed error 0x{0:X8}", num);
				throw new Win32Exception((int)num);
			}
		}

		private static string CopyToDirectory(string filePath, string destinationDirectory)
		{
			string text = Environment.ExpandEnvironmentVariables(filePath);
			LogUtil.Diagnostic("Copying {0} to {1}", text, destinationDirectory);
			if (!LongPathFile.Exists(text))
			{
				throw new PkgGenException("Can't find required file: {0}", text);
			}
			string text2 = Path.Combine(destinationDirectory, Path.GetFileName(text));
			LongPathFile.Copy(text, text2, true);
			LongPathFile.SetAttributes(text2, FileAttributes.Normal);
			return text2;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			lock (syncRoot)
			{
				Close();
				if (_hDrvStoreModule != IntPtr.Zero)
				{
					bool num = DriverStoreInterop.FreeLibrary(_hDrvStoreModule);
					_hDrvStoreModule = IntPtr.Zero;
					if (!num)
					{
						LogUtil.Warning("Unable to unload drvstore.dll");
					}
				}
			}
		}

		public void Close()
		{
			if (_hDrvStore != IntPtr.Zero)
			{
				LogUtil.Diagnostic("DriverStoreClose {0} ", _hDrvStore.ToString());
				if (!DriverStoreInterop.DriverStoreClose(_hDrvStore))
				{
					throw new PkgGenException($"Unable to close driver store");
				}
				_hDrvStore = IntPtr.Zero;
			}
		}
	}
}
