using System.Text.RegularExpressions;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public static class ConstantStrings
	{
		public const string Extension = ".XML";

		public const string WinPhoneRoot = "_WINPHONEROOT";

		public const string Namespace = "WP_Policy";

		public const string NodePathHelper = "/WP_Policy:";

		public const string NodePathRoot = "/WP_Policy:PhoneSecurityPolicy";

		public const string NodePathMacro = "//WP_Policy:Macros/WP_Policy:Macro";

		public const string NodePathCapabilities = "//WP_Policy:Capabilities";

		public const string NodePathComponents = "//WP_Policy:Components";

		public const string NodePathAuthorization = "//WP_Policy:Authorization";

		public const string ServiceOwnProcess = "Win32OwnProcess";

		public const string ServiceSharedProcess = "Win32ShareProcess";

		public const string NodeService = "./WP_Policy:Service[@Type='Win32OwnProcess' or @Type='Win32ShareProcess']";

		public const string NodeFullTrust = "./WP_Policy:FullTrust";

		public const string NoAttributeValue = "No";

		public const string MacroPrefixCharacters = "$(";

		public const string MacroPostfixCharacters = ")";

		public const string DefaultAccountName000 = "DA0";

		public const string InstallationFolderPath = "\\PROGRAMS\\{0}\\(*)";

		public const string ElementInstallationFolder = "InstallationFolder";

		public const string InstallationFolderRight = "0x001200A9";

		public const string OwnerRight = "OW";

		public const string ChamberProfileDataDefaultFolder = "ChamberProfileDataDefaultFolder";

		public const string ChamberProfileDataShellContentFolder = "ChamberProfileDataShellContentFolder";

		public const string ChamberProfileDataMediaFolder = "ChamberProfileDataMediaFolder";

		public const string ChamberProfileDataPlatformDataFolder = "ChamberProfileDataPlatformDataFolder";

		public const string ChamberProfileDataLiveTilesFolder = "ChamberProfileDataLiveTilesFolder";

		public const string ApplicationDefaultDataFolderPath = "\\DATA\\{0}\\{1}\\(*)";

		public const string ApplicationShellContentFolderPath = "\\DATA\\{0}\\{1}\\Local\\Shared\\ShellContent\\(*)";

		public const string ApplicationMediaFolderPath = "\\DATA\\{0}\\{1}\\Local\\Shared\\Media\\(*)";

		public const string ApplicationPlatformDataFolderPath = "\\DATA\\{0}\\{1}\\PlatformData\\(*)";

		public const string ApplicationLiveTilesFolderPath = "\\DATA\\{0}\\{1}\\PlatformData\\LiveTiles\\(*)";

		public const string ApplicationFolderRRight = "0x001200A9";

		public const string ApplicationFolderRWRight = "0x001201bf";

		public const string ApplicationFolderRWDRight = "0x001301bf";

		public const string ApplicationFolderAllRights = "0x001301ff";

		public const string ApplicationFolderFullRight = "FA";

		public const string System = "SY";

		public const string CapabilityChamberProfileRead = "ID_CAP_CHAMBER_PROFILE_CODE_R";

		public const string CapabilityChamberProfileWrite = "ID_CAP_CHAMBER_PROFILE_CODE_RW";

		public const string CapabilityChamberProfileTempInstall = "ID_CAP_CHAMBER_PROFILE_CODE_INSTALLTEMP_RWD";

		public const string CapabilityChamberProfileTempNi = "ID_CAP_CHAMBER_PROFILE_CODE_NITEMP_RW";

		public const string CapabilityChamberProfileDataRead = "ID_CAP_CHAMBER_PROFILE_DATA_R";

		public const string CapabilityChamberProfileDataWrite = "ID_CAP_CHAMBER_PROFILE_DATA_RW";

		public const string CapabilityChamberProfileShellContentRead = "ID_CAP_CHAMBER_PROFILE_DATA_SHELLCONTENT_R";

		public const string CapabilityChamberProfileShellContentAll = "ID_CAP_CHAMBER_PROFILE_DATA_SHELLCONTENT_RWD";

		public const string CapabilityChamberProfileMediaAll = "ID_CAP_CHAMBER_PROFILE_DATA_MEDIA_RWD";

		public const string CapabilityChamberProfilePlatformDataAll = "ID_CAP_CHAMBER_PROFILE_DATA_PLATFORMDATA_ALL";

		public const string CapabilityChamberProfileLiveTilesAll = "ID_CAP_CHAMBER_PROFILE_DATA_LIVETILES_RWD";

		public const string RequiredInheritancePostfix = "\\(*)";

		public const string RequiredInheritanceOnlyPostfix = "\\(+)";

		public const string RegKeyRuleInheritanceFlag = "CI";

		public const string DirectoryRuleInheritanceFlag = "CIOI";

		public const string DirectoryRuleInheritanceOnlyFlag = "IOCIOI";

		public const string ApplicationCapabilitySIDPrefix = "S-1-15-3-1024";

		public const string ServiceCapabilitySIDPrefix = "S-1-5-21-2702878673-795188819-444038987";

		public const string WindowsCapabilitySIDPrefix = "S-1-15-3";

		public const int ServiceCapabilityStartRID = 1031;

		public const int NumberOfServiceCapabilities = 1750;

		public const string AllApplicationCapabilityGroupSID = "S-1-5-21-2702878673-795188819-444038987-1030";

		public const string DefAppsAccountSID = "S-1-5-21-2702878673-795188819-444038987-2781";

		public const string TrustedInstallerSID = "S-1-5-80-956008885-3418522649-1831038044-1853292631-2271478464";

		public const string DataSharingServiceSID = "S-1-5-80-1551822644-3134808374-1042292604-2865742758-3851661496";

		public const string PolicyCompilerTestFlag = "POLICY_COMPILER_TEST";

		public const string CapabilityVisibilityPrivate = "Private";

		public const string CapabilityVisibilityInternal = "Internal";

		public const string CapabilityVisibilityPublic = "Public";

		private static string[] capabilityVisibilityList = new string[3] { "Public", "Internal", "Private" };

		public const string ApplicationSIDPrefix = "S-1-15-2";

		public const string ServiceSIDPrefix = "S-1-5-80";

		public const string BuiltinUsers = "BU";

		public const string WriteRestrictedSID = "S-1-5-33";

		public const string NtServicesCapability = "ID_CAP_NTSERVICES";

		public static string[] PredefinedServiceCapabilities = new string[1] { "ID_CAP_NTSERVICES" };

		public const string DACLFormat = "(A;{0};{1};;;{2})";

		public const string RuleString = "Rule";

		public static string[] ComponentCapabilityIdFilter = new string[3] { "ID_CAP_EVERYONE", "ID_CAP_BUILTIN_DEFAULT", "ID_CAP_EVERYONE_INROM" };

		public static string[] ServiceCapabilityIdFilter = new string[1] { "ID_CAP_BUILTIN_CREATEGLOBAL" };

		public static string EveryoneCapability = "ID_CAP_EVERYONE";

		public static string[] BlockedCapabilityIdForApplicationFilter = new string[3] { "ID_CAP_BUILTIN_TCB", "ID_CAP_BUILTIN_SYMBOLICLINK", "ID_CAP_BUILTIN_IMPERSONATE" };

		public const string WindowsRulesIdPrefix = "ID_CAP_WINRULES_";

		public const string PrivateResourcesIdPrefix = "ID_CAP_PRIV_";

		public const string PrivateResourcesFriendlyNamePostfix = " private capability";

		public const string PrivateResourcesRights = "$(ALL_ACCESS)";

		public const string PrivateResourcesReadOnlyRights = "$(GENERIC_READ)";

		public const string PrivateResourcesServiceAccessRights = "$(SERVICE_PRIVATE_RESOURCE_ACCESS)";

		public const string PrivateResourcesCOMAccessRuleRights = "$(COM_LOCAL_ACCESS)";

		public const string PrivateResourcesCOMLaunchRuleRights = "$(COM_LOCAL_LAUNCH)";

		public static Regex[] BlockedFilePathRegexes = new Regex[7]
		{
			new Regex("^\\\\CRASHDUMP($|\\\\.*)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
			new Regex("^\\\\DATA$", RegexOptions.IgnoreCase | RegexOptions.Compiled),
			new Regex("^\\\\DPP($|\\\\.*)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
			new Regex("^\\\\EFIESP($|\\\\.*)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
			new Regex("^\\\\MMOS($|\\\\.*)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
			new Regex("^\\\\WINDOWS\\\\SERVICEPROFILES($|\\\\.*)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
			new Regex("^\\\\SDCARD($|\\\\.*)", RegexOptions.IgnoreCase | RegexOptions.Compiled)
		};

		public static Regex[] BlockedRegPathRegexes = new Regex[2]
		{
			new Regex("^HKEY_LOCAL_MACHINE\\\\SYSTEM\\\\CONTROLSET001\\\\SERVICES\\\\[A-Z0-9_-]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled),
			new Regex("^HKEY_LOCAL_MACHINE\\\\SYSTEM\\\\CONTROLSET001\\\\SERVICES\\\\[A-Z0-9_-]+\\\\PARAMETERS$", RegexOptions.IgnoreCase | RegexOptions.Compiled)
		};

		public static string[] AllowedRegPaths = new string[2] { "HKEY_LOCAL_MACHINE\\SYSTEM\\CONTROLSET001\\SERVICES\\TCPIP\\PARAMETERS", "HKEY_LOCAL_MACHINE\\SYSTEM\\CONTROLSET001\\SERVICES\\W32TIME\\PARAMETERS" };

		public const string ServiceAccessRulePathPrefix = "HKEY_LOCAL_MACHINE\\SYSTEM\\ControlSet001\\services\\";

		public const string DeviceSetupClassRulePathPrefix = "HKEY_LOCAL_MACHINE\\SYSTEM\\ControlSet001\\Control\\Class\\";

		public const string COMRulePathPrefix = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Classes\\AppID\\";

		public const string WinRTRulePathPrefix = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\WindowsRuntime\\Server\\";

		public const string EtwProviderPathPrefix = "HKEY_LOCAL_MACHINE\\SYSTEM\\ControlSet001\\Control\\WMI\\Security\\";

		public const string WnfPathPrefix = "HKEY_LOCAL_MACHINE\\SYSTEM\\ControlSet001\\Control\\Notifications\\";

		public const string ServiceAccessRulePathPostfix = "\\Security\\Security";

		public const string DeviceSetupClassRulePathPostfix = "\\Properties\\Security";

		public const string COMRuleAccessPermissionPathPostfix = "\\AccessPermission";

		public const string COMRuleLaunchPermissionPathPostfix = "\\LaunchPermission";

		public const string WinRTRulePermissionsPathPostfix = "\\Permissions";

		public const string COMRuleLaunchPermissionTag = "COMLaunchPermission";

		public const string COMRuleAccessPermissionTag = "COMAccessPermission";

		public const string WinRTRuleLaunchPermissionTag = "WinRTLaunchPermission";

		public const string WinRTRuleAccessPermissionTag = "WinRTAccessPermission";

		public const string TransientObjectPathPrefix = "%5C%5C.%5C";

		public const string TransientObjectPathSeparator = "%5C";

		public const string TransientObjectPathOriginalSeparator = "\\";

		public const string AttributeNotSet = null;

		public const string AttributeNotCalculated = "Not Calculated";

		public const string DefaultHashType = "Sha256";

		public const string DefaultServiceExecutableAttribute = "$(RUNTIME.SYSTEM32)\\SvcHost.exe";

		public const string DefaultServiceIsTCBAttribute = "No";

		private static string[] packageAllowList = new string[2] { "Microsoft.BaseOS.SecurityModel", "Microsoft.BaseOS.CoreSecurityPolicy" };

		private const int IndentationLength = 3;

		public const string IndentationLevel0 = "";

		public static readonly string IndentationLevel1 = string.Format(GlobalVariables.Culture, "{0}{1}", new object[2]
		{
			"",
			new string(' ', 3)
		});

		public static readonly string IndentationLevel2 = string.Format(GlobalVariables.Culture, "{0}{1}", new object[2]
		{
			"",
			new string(' ', 6)
		});

		public static readonly string IndentationLevel3 = string.Format(GlobalVariables.Culture, "{0}{1}", new object[2]
		{
			"",
			new string(' ', 9)
		});

		public static readonly string IndentationLevel4 = string.Format(GlobalVariables.Culture, "{0}{1}", new object[2]
		{
			"",
			new string(' ', 12)
		});

		public static readonly string IndentationLevel5 = string.Format(GlobalVariables.Culture, "{0}{1}", new object[2]
		{
			"",
			new string(' ', 15)
		});

		public const string ErrorMessagePrefix = "Error: CompileSecurityPolicy {0}";

		public const string DebugMessagePrefix = "Debug: ";

		public const string OutputFilePrintElementFormat = "{0}{1}";

		public const string OutputFilePrintAttributeFormat = "{0}{1}=\"{2}\"";

		public const string MacroReferencingErrorMessage = "Macro Referencing Error: {0}, Value= {1}";

		public const string MacroDefinitionNotFoundErrorMessage = "Macro Definition Not Found: {0}, Value= {1}";

		public const string ElementAndAttributeFormat = "Element={0}, Attribute={1}";

		public const string ElementTwoAttributeExclusiveMessageFormat = "Element={0} '{1}', Attributes {2} and {3} are mutually exclusive";

		public const string ReadOnlyViolationErrorMessage = "It is not allowed to grants write access on '{0}'. Only the folders under \\DATA can be granted write access.";

		public const string DirectoryOnlyErrorMessage = "It is not allowed to define a capability rule for a file '{0}' under \\DATA, only Directory rule is allowed.";

		public const string CapabilityDefinitionErrorMessage = "The package definition file should not have capability definition";

		public const string ServiceChangeConfigViolationErrorMessage = "It is not allowed to grant SERVICE_CHANGE_CONFIG on service '{0}'.";

		public const string BlockedCapabilityIdForApplicationErrorMessage = "The capability '{0}' can't be used in application.";

		public const string UnsupportedUserRegKeyErrorMessage = "It is not allowed to define a capability rule or private resource for registry key '{0}' under HKEY_USERS or HKEY_CURRENT_USER.";

		public const string CoexistenceErrorMessage = "'{0}' and '{1}' can't be present in the same time in service '{2}'.";

		public const string PrivateResourcesDefinitionErrorMessage = "The private resources can't be defined in this package.";

		public const string BlockedPathErrorMessage = "'{0}' should not be defined in capability rule or private resource.";

		public const string UnsupportedRegKeyErrorMessage = "It is not allowed to define a capability rule or private resource for write access on registry key '{0}'.";

		public static string[] GetValidCapabilityVisibilityList()
		{
			return capabilityVisibilityList;
		}

		public static string[] GetPackageAllowList()
		{
			return packageAllowList;
		}
	}
}
