using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.WindowsPhone.Imaging
{
	public static class BcdElementDataTypes
	{
		public static readonly BcdElementDataType DefaultObject = new BcdElementDataType(ElementClass.Application, ElementFormat.Object, 3u);

		public static readonly BcdElementDataType ResumeObject = new BcdElementDataType(ElementClass.Application, ElementFormat.Object, 6u);

		public static readonly BcdElementDataType DisplayOrder = new BcdElementDataType(ElementClass.Application, ElementFormat.ObjectList, 1u);

		public static readonly BcdElementDataType BootSequence = new BcdElementDataType(ElementClass.Application, ElementFormat.ObjectList, 2u);

		public static readonly BcdElementDataType ToolsDisplayOrder = new BcdElementDataType(ElementClass.Application, ElementFormat.ObjectList, 16u);

		public static readonly BcdElementDataType TimeoutValue = new BcdElementDataType(ElementClass.Application, ElementFormat.Integer, 4u);

		public static readonly BcdElementDataType NXPolicy = new BcdElementDataType(ElementClass.Application, ElementFormat.Integer, 32u);

		public static readonly BcdElementDataType PAEPolicy = new BcdElementDataType(ElementClass.Application, ElementFormat.Integer, 33u);

		public static readonly BcdElementDataType DebuggingEnabled = new BcdElementDataType(ElementClass.Application, ElementFormat.Boolean, 6u);

		public static readonly BcdElementDataType DisplayBootMenu = new BcdElementDataType(ElementClass.Application, ElementFormat.Boolean, 32u);

		public static readonly BcdElementDataType WinPEImage = new BcdElementDataType(ElementClass.Application, ElementFormat.Boolean, 34u);

		public static readonly BcdElementDataType RemoveMemory = new BcdElementDataType(ElementClass.Application, ElementFormat.Integer, 49u);

		public static readonly BcdElementDataType KernelDebuggerEnabled = new BcdElementDataType(ElementClass.Application, ElementFormat.Boolean, 160u);

		public static readonly BcdElementDataType KernelEmsEnabled = new BcdElementDataType(ElementClass.Application, ElementFormat.Boolean, 176u);

		public static readonly BcdElementDataType SystemRoot = new BcdElementDataType(ElementClass.Application, ElementFormat.String, 2u);

		public static readonly BcdElementDataType FilePath = new BcdElementDataType(ElementClass.Application, ElementFormat.String, 35u);

		public static readonly BcdElementDataType OsLoaderType = new BcdElementDataType(ElementClass.Application, ElementFormat.Device, 1u);

		public static readonly BcdElementDataType BcdDevice = new BcdElementDataType(ElementClass.Application, ElementFormat.Device, 34u);

		public static readonly BcdElementDataType BootMenuPolicy = new BcdElementDataType(ElementClass.Application, ElementFormat.Integer, 194u);

		public static readonly BcdElementDataType DetectKernelAndHal = new BcdElementDataType(ElementClass.Application, ElementFormat.Boolean, 16u);

		public static readonly BcdElementDataType PersistBootSequence = new BcdElementDataType(ElementClass.Application, ElementFormat.Boolean, 49u);

		public static readonly BcdElementDataType FfuUpdateMode = new BcdElementDataType(ElementClass.Application, ElementFormat.Boolean, 513u);

		public static readonly BcdElementDataType ForceFfu = new BcdElementDataType(ElementClass.Application, ElementFormat.Boolean, 515u);

		public static readonly BcdElementDataType CheckPlatformId = new BcdElementDataType(ElementClass.Application, ElementFormat.Boolean, 516u);

		public static readonly BcdElementDataType DisableCheckPlatformId = new BcdElementDataType(ElementClass.Application, ElementFormat.Boolean, 517u);

		public static readonly BcdElementDataType EnableUFPMode = new BcdElementDataType(ElementClass.Application, ElementFormat.Boolean, 519u);

		public static readonly BcdElementDataType UFPMincryplHashingSupport = new BcdElementDataType(ElementClass.Application, ElementFormat.Boolean, 520u);

		public static readonly BcdElementDataType CustomActionsList = new BcdElementDataType(ElementClass.Application, ElementFormat.IntegerList, 48u);

		public static readonly BcdElementDataType DebugTransportPath = new BcdElementDataType(ElementClass.Application, ElementFormat.String, 19u);

		public static readonly BcdElementDataType HypervisorDebuggerType = new BcdElementDataType(ElementClass.Application, ElementFormat.Integer, 243u);

		public static readonly BcdElementDataType HypervisorDebuggerPortNumber = new BcdElementDataType(ElementClass.Application, ElementFormat.Integer, 244u);

		public static readonly BcdElementDataType HypervisorDebuggerBaudRate = new BcdElementDataType(ElementClass.Application, ElementFormat.Integer, 245u);

		public static readonly BcdElementDataType MemoryCaptureModeAddress = new BcdElementDataType(ElementClass.Application, ElementFormat.Integer, 1280u);

		public static readonly BcdElementDataType WpDmpSettingsFile = new BcdElementDataType(ElementClass.Application, ElementFormat.String, 1281u);

		public static readonly BcdElementDataType WpDmpLogFile = new BcdElementDataType(ElementClass.Application, ElementFormat.String, 1282u);

		public static readonly BcdElementDataType OemChargingBootThreshold = new BcdElementDataType(ElementClass.Application, ElementFormat.Integer, 1296u);

		public static readonly BcdElementDataType OemChargingModeThreshold = new BcdElementDataType(ElementClass.Application, ElementFormat.Integer, 1297u);

		public static readonly BcdElementDataType OemChargingModeEnabled = new BcdElementDataType(ElementClass.Application, ElementFormat.Boolean, 1298u);

		public static readonly BcdElementDataType BootFlowVariableGlobal = new BcdElementDataType(ElementClass.Application, ElementFormat.Integer, 2730u);

		public static readonly BcdElementDataType ProcessCustomActionsFirst = new BcdElementDataType(ElementClass.Application, ElementFormat.Boolean, 40u);

		public static readonly BcdElementDataType BootStatusPolicy = new BcdElementDataType(ElementClass.Application, ElementFormat.Integer, 224u);

		public static readonly BcdElementDataType AllowUserToResetPhone = new BcdElementDataType(ElementClass.Application, ElementFormat.Boolean, 518u);

		public static readonly BcdElementDataType ManufacturingMode = new BcdElementDataType(ElementClass.Application, ElementFormat.String, 320u);

		public static readonly BcdElementDataType MSEBootDebugPolicy = new BcdElementDataType(ElementClass.Application, ElementFormat.Boolean, 325u);

		public static readonly BcdElementDataType MSEBootOrderClean = new BcdElementDataType(ElementClass.Application, ElementFormat.Boolean, 326u);

		public static readonly BcdElementDataType MSEDeviceId = new BcdElementDataType(ElementClass.Application, ElementFormat.Boolean, 327u);

		public static readonly BcdElementDataType MSEFfuLoader = new BcdElementDataType(ElementClass.Application, ElementFormat.Boolean, 328u);

		public static readonly BcdElementDataType MSEIuLoader = new BcdElementDataType(ElementClass.Application, ElementFormat.Boolean, 329u);

		public static readonly BcdElementDataType MSEMassStorage = new BcdElementDataType(ElementClass.Application, ElementFormat.Boolean, 330u);

		public static readonly BcdElementDataType MSERpmbProvisioning = new BcdElementDataType(ElementClass.Application, ElementFormat.Boolean, 331u);

		public static readonly BcdElementDataType MSESecureBootPolicy = new BcdElementDataType(ElementClass.Application, ElementFormat.Boolean, 332u);

		public static readonly BcdElementDataType MSEStartCharge = new BcdElementDataType(ElementClass.Application, ElementFormat.Boolean, 333u);

		public static readonly BcdElementDataType MSEResetTPM = new BcdElementDataType(ElementClass.Application, ElementFormat.Boolean, 334u);

		public static readonly BcdElementDataType QuietBootEnable = new BcdElementDataType(ElementClass.Application, ElementFormat.Boolean, 65u);

		public static readonly Dictionary<BcdElementDataType, string> ApplicationTypes = new Dictionary<BcdElementDataType, string>
		{
			{ DefaultObject, "Default Object" },
			{ ResumeObject, "Resume Object" },
			{ DisplayOrder, "Display Order" },
			{ BootSequence, "Boot Sequence" },
			{ ToolsDisplayOrder, "Tools Display Order" },
			{ TimeoutValue, "Timeout Value" },
			{ NXPolicy, "NX Policy" },
			{ PAEPolicy, "PAE Policy" },
			{ DebuggingEnabled, "Debugging Enabled" },
			{ DisplayBootMenu, "Display Boot Menu" },
			{ WinPEImage, "WinPE Image" },
			{ RemoveMemory, "Remove Memory" },
			{ KernelDebuggerEnabled, "Kernel Debugger Enabled" },
			{ KernelEmsEnabled, "Kernel EMS Enabled" },
			{ SystemRoot, "System Root" },
			{ FilePath, "File Path" },
			{ OsLoaderType, "OS Loader Type" },
			{ BcdDevice, "BCD Device" },
			{ BootMenuPolicy, "Boot Menu Policy" },
			{ DetectKernelAndHal, "Detect Kernel and HAL" },
			{ PersistBootSequence, "Persist Boot Sequence" },
			{ FfuUpdateMode, "FFU Update Mode" },
			{ ForceFfu, "Force FFU" },
			{ CheckPlatformId, "Enable Platform ID Check" },
			{ DisableCheckPlatformId, "Disable Platform ID Check" },
			{ EnableUFPMode, "Enable UFP Mode" },
			{ UFPMincryplHashingSupport, "Use Mincrypl for Hashing" },
			{ CustomActionsList, "Custom Action List" },
			{ DebugTransportPath, "Debug Transport Path" },
			{ HypervisorDebuggerType, "Hypervisor Debugger Type" },
			{ HypervisorDebuggerPortNumber, "Hypervisor Debugger Port Number" },
			{ HypervisorDebuggerBaudRate, "Hypervisor Debugger Baud Rate" },
			{ MemoryCaptureModeAddress, "Memory Capture Mode Address" },
			{ WpDmpSettingsFile, "WpDmp Settings File" },
			{ WpDmpLogFile, "WpDmp Log File" },
			{ OemChargingBootThreshold, "OEM Charging Boot Threshold" },
			{ OemChargingModeThreshold, "OEM Charging Mode Threshold" },
			{ OemChargingModeEnabled, "OEM Charging Mode Enabled" },
			{ BootFlowVariableGlobal, "BootFlowAPI Global" },
			{ ProcessCustomActionsFirst, "Process Custom Actions First" },
			{ BootStatusPolicy, "Boot Status Policy" },
			{ AllowUserToResetPhone, "Allow User To Reset Phone" },
			{ ManufacturingMode, "Manufacturing Mode" },
			{ MSEBootDebugPolicy, "Enable Boot Debug Policy" },
			{ MSEBootOrderClean, "Enable Boot Order Clean" },
			{ MSEDeviceId, "Enable Device Id" },
			{ MSEFfuLoader, "Enable FFU Loader" },
			{ MSEIuLoader, "Enable IU Loader" },
			{ MSEMassStorage, "Enable Mass Storage" },
			{ MSERpmbProvisioning, "Enable RPMB Provisioning" },
			{ MSESecureBootPolicy, "Enable Secure Boot Policy" },
			{ MSEStartCharge, "Enable Start Charge" },
			{ MSEResetTPM, "Enable Reset TPM" },
			{ QuietBootEnable, "Quiet Boot Enable" }
		};

		public static readonly BcdElementDataType OsLoaderDevice = new BcdElementDataType(ElementClass.Library, ElementFormat.Device, 1u);

		public static readonly BcdElementDataType ApplicationPath = new BcdElementDataType(ElementClass.Library, ElementFormat.String, 2u);

		public static readonly BcdElementDataType Description = new BcdElementDataType(ElementClass.Library, ElementFormat.String, 4u);

		public static readonly BcdElementDataType PreferredLocale = new BcdElementDataType(ElementClass.Library, ElementFormat.String, 5u);

		public static readonly BcdElementDataType AutoRecoveryEnabled = new BcdElementDataType(ElementClass.Library, ElementFormat.Boolean, 9u);

		public static readonly BcdElementDataType DebuggerEnabled = new BcdElementDataType(ElementClass.Library, ElementFormat.Boolean, 16u);

		public static readonly BcdElementDataType EmsEnabled = new BcdElementDataType(ElementClass.Library, ElementFormat.Boolean, 32u);

		public static readonly BcdElementDataType DisplayAdvanceOptions = new BcdElementDataType(ElementClass.Library, ElementFormat.Boolean, 64u);

		public static readonly BcdElementDataType DisplayOptionsEdit = new BcdElementDataType(ElementClass.Library, ElementFormat.Boolean, 65u);

		public static readonly BcdElementDataType DisableIntegrityChecks = new BcdElementDataType(ElementClass.Library, ElementFormat.Boolean, 72u);

		public static readonly BcdElementDataType AllowPreReleaseSignatures = new BcdElementDataType(ElementClass.Library, ElementFormat.Boolean, 73u);

		public static readonly BcdElementDataType ConsoleExtendedInput = new BcdElementDataType(ElementClass.Library, ElementFormat.Boolean, 80u);

		public static readonly BcdElementDataType Inherit = new BcdElementDataType(ElementClass.Library, ElementFormat.ObjectList, 6u);

		public static readonly BcdElementDataType RecoverySequence = new BcdElementDataType(ElementClass.Library, ElementFormat.ObjectList, 8u);

		public static readonly BcdElementDataType DebuggerType = new BcdElementDataType(ElementClass.Library, ElementFormat.Integer, 17u);

		public static readonly BcdElementDataType DebuggerPortAddress = new BcdElementDataType(ElementClass.Library, ElementFormat.Integer, 18u);

		public static readonly BcdElementDataType PortNumber = new BcdElementDataType(ElementClass.Library, ElementFormat.Integer, 19u);

		public static readonly BcdElementDataType DebuggerUsbTargetName = new BcdElementDataType(ElementClass.Library, ElementFormat.String, 22u);

		public static readonly BcdElementDataType DebuggerBusParameters = new BcdElementDataType(ElementClass.Library, ElementFormat.String, 25u);

		public static readonly BcdElementDataType DebuggerIgnoreUsermodeExceptions = new BcdElementDataType(ElementClass.Library, ElementFormat.Boolean, 23u);

		public static readonly BcdElementDataType DebuggerStartPolicy = new BcdElementDataType(ElementClass.Library, ElementFormat.Integer, 24u);

		public static readonly BcdElementDataType DebuggerNetworkKey = new BcdElementDataType(ElementClass.Library, ElementFormat.String, 29u);

		public static readonly BcdElementDataType DebuggerNetworkHostIp = new BcdElementDataType(ElementClass.Library, ElementFormat.Integer, 26u);

		public static readonly BcdElementDataType DebuggerNetworkPort = new BcdElementDataType(ElementClass.Library, ElementFormat.Integer, 27u);

		public static readonly BcdElementDataType DebuggerNetworkDhcp = new BcdElementDataType(ElementClass.Library, ElementFormat.Boolean, 28u);

		public static readonly BcdElementDataType DebuggerBaudRate = new BcdElementDataType(ElementClass.Library, ElementFormat.Integer, 20u);

		public static readonly BcdElementDataType ForceNoKeyboard = new BcdElementDataType(ElementClass.Library, ElementFormat.Boolean, 114u);

		public static readonly BcdElementDataType BootUxFadeDisable = new BcdElementDataType(ElementClass.Library, ElementFormat.Boolean, 106u);

		public static readonly BcdElementDataType BootUxLogoTransitionEnable = new BcdElementDataType(ElementClass.Library, ElementFormat.Boolean, 122u);

		public static readonly BcdElementDataType BootUxLogoTransitionTime = new BcdElementDataType(ElementClass.Library, ElementFormat.Integer, 121u);

		public static readonly BcdElementDataType BootUxProgressAnimationDisable = new BcdElementDataType(ElementClass.Library, ElementFormat.Boolean, 105u);

		public static readonly BcdElementDataType BootUxTextDisable = new BcdElementDataType(ElementClass.Library, ElementFormat.Boolean, 104u);

		public static readonly BcdElementDataType BootUxErrorScreen = new BcdElementDataType(ElementClass.Library, ElementFormat.Integer, 125u);

		public static readonly BcdElementDataType BsdFilepath = new BcdElementDataType(ElementClass.Library, ElementFormat.String, 68u);

		public static readonly BcdElementDataType BsdPreservePreviousEntries = new BcdElementDataType(ElementClass.Library, ElementFormat.Boolean, 69u);

		public static readonly BcdElementDataType LoadOptions = new BcdElementDataType(ElementClass.Library, ElementFormat.String, 48u);

		public static readonly BcdElementDataType GraphicsModeDisable = new BcdElementDataType(ElementClass.Library, ElementFormat.Boolean, 70u);

		public static readonly Dictionary<BcdElementDataType, string> LibraryTypes = new Dictionary<BcdElementDataType, string>
		{
			{ OsLoaderDevice, "Boot Device" },
			{ ApplicationPath, "Application Path" },
			{ Description, "Description" },
			{ PreferredLocale, "Preferred Locale" },
			{ AutoRecoveryEnabled, "Auto-recovery Enabled" },
			{ DebuggerEnabled, "Debugger Enabled" },
			{ EmsEnabled, "EMS Enabled" },
			{ DisplayAdvanceOptions, "Display Advanced Options" },
			{ DisplayOptionsEdit, "Display Option Edit" },
			{ DisableIntegrityChecks, "Disable Integrity Checks" },
			{ AllowPreReleaseSignatures, "Allow Pre-release Signatures" },
			{ ConsoleExtendedInput, "Console Extended Input" },
			{ Inherit, "Inherit" },
			{ RecoverySequence, "Recovery Sequence" },
			{ DebuggerType, "Debugger Type" },
			{ DebuggerPortAddress, "Debugger Port Address" },
			{ PortNumber, "Port Number" },
			{ DebuggerUsbTargetName, "Debugger USB Target Name" },
			{ DebuggerBusParameters, "Debugger Bus Parameters" },
			{ DebuggerIgnoreUsermodeExceptions, "Debugger Ignore Usermode Exceptions" },
			{ DebuggerStartPolicy, "Debugger Start Policy" },
			{ DebuggerNetworkKey, "Debugger Network Key" },
			{ DebuggerNetworkHostIp, "Debugger Network Host IP" },
			{ DebuggerNetworkPort, "Debugger Network Port" },
			{ DebuggerNetworkDhcp, "Debugger Network DHCP" },
			{ DebuggerBaudRate, "Debugger Baud Rate" },
			{ ForceNoKeyboard, "Force No Keyboard" },
			{ BootUxFadeDisable, "Boot UX Fade Disable" },
			{ BootUxLogoTransitionEnable, "Boot UX Logo Transition Enable" },
			{ BootUxLogoTransitionTime, "Boot UX Logo Transition Time" },
			{ BootUxProgressAnimationDisable, "Boot UX Progress Animation Disable" },
			{ BootUxTextDisable, "Boot UX Text Disable" },
			{ BootUxErrorScreen, "Boot UX Error Screen" },
			{ BsdFilepath, "BSD Filepath" },
			{ BsdPreservePreviousEntries, "BSD Preserve Previous Entries" },
			{ LoadOptions, "Load Options" },
			{ GraphicsModeDisable, "Graphics Mode Disable" }
		};

		public static readonly BcdElementDataType RamDiskSdiPath = new BcdElementDataType(ElementClass.Device, ElementFormat.String, 4u);

		public static readonly BcdElementDataType RamDiskSdiDevice = new BcdElementDataType(ElementClass.Device, ElementFormat.Device, 3u);

		public static readonly Dictionary<BcdElementDataType, string> DeviceTypes = new Dictionary<BcdElementDataType, string>
		{
			{ RamDiskSdiPath, "Ramdisk SDI Path" },
			{ RamDiskSdiDevice, "Ramdisk SDI Device" }
		};

		public static readonly BcdElementDataType FlashingAction = new BcdElementDataType(ElementClass.Oem, ElementFormat.ObjectList, 1u);

		public static readonly BcdElementDataType ResetMyPhoneAction = new BcdElementDataType(ElementClass.Oem, ElementFormat.ObjectList, 2u);

		public static readonly BcdElementDataType DeveloperMenuAction = new BcdElementDataType(ElementClass.Oem, ElementFormat.ObjectList, 3u);

		public static readonly Dictionary<BcdElementDataType, string> OEMTypes = new Dictionary<BcdElementDataType, string>
		{
			{ FlashingAction, "Flashing Action" },
			{ ResetMyPhoneAction, "Reset My Phone Action" },
			{ DeveloperMenuAction, "Developer Menu Action" }
		};

		public static BcdElementDataType GetWellKnownDataType(string dataTypeName)
		{
			BcdElementDataType bcdElementDataType = null;
			foreach (BcdElementDataType key in LibraryTypes.Keys)
			{
				if (string.Compare(LibraryTypes[key], dataTypeName, true, CultureInfo.InvariantCulture) == 0)
				{
					bcdElementDataType = key;
					break;
				}
			}
			if (bcdElementDataType == null)
			{
				foreach (BcdElementDataType key2 in ApplicationTypes.Keys)
				{
					if (string.Compare(ApplicationTypes[key2], dataTypeName, true, CultureInfo.InvariantCulture) == 0)
					{
						bcdElementDataType = key2;
						break;
					}
				}
			}
			if (bcdElementDataType == null)
			{
				foreach (BcdElementDataType key3 in DeviceTypes.Keys)
				{
					if (string.Compare(DeviceTypes[key3], dataTypeName, true, CultureInfo.InvariantCulture) == 0)
					{
						bcdElementDataType = key3;
						break;
					}
				}
			}
			if (bcdElementDataType == null)
			{
				foreach (BcdElementDataType key4 in OEMTypes.Keys)
				{
					if (string.Compare(OEMTypes[key4], dataTypeName, true, CultureInfo.InvariantCulture) == 0)
					{
						return key4;
					}
				}
				return bcdElementDataType;
			}
			return bcdElementDataType;
		}
	}
}
