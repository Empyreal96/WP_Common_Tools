using System.Collections.Generic;

namespace Microsoft.CompPlat.PkgBldr.Base.Security.SecurityPolicy
{
	public static class ConstantStrings
	{
		public const string EveryoneCapabilityName = "everyone";

		public static Dictionary<string, string> LegacyApplicationCapabilityRids = new Dictionary<string, string>
		{
			{ "internetClient", "1" },
			{ "internetServer", "2" },
			{ "privateNetworkClientServer", "3" },
			{ "picturesLibrary", "4" },
			{ "videosLibrary", "5" },
			{ "musicLibrary", "6" },
			{ "documentsLibrary", "7" },
			{ "enterpriseAuthentication", "8" },
			{ "sharedUserCertificates", "9" },
			{ "removableStorage", "10" },
			{ "appointments", "11" },
			{ "contacts", "12" }
		};

		public const string AuthenticatedUsers = "AU";

		public const string AllApplicationPackages = "S-1-15-2-1";

		public const string InteractiveUsers = "IU";

		public const string ApplicationSidPrefix = "S-1-15-2";

		public const string ServiceSidPrefix = "S-1-5-80";

		public const string LegacyCapabilitySidPrefix = "S-1-15-3";

		public const string ApplicationCapabilitySidPrefix = "S-1-15-3-1024";

		public const string ServiceCapabilitySidPrefix = "S-1-5-32";

		public const string TrustedInstallerSid = "S-1-5-80-956008885-3418522649-1831038044-1853292631-2271478464";

		public const string TrustedInstallerOwner = "O:S-1-5-80-956008885-3418522649-1831038044-1853292631-2271478464";

		public const string TrustedInstallerGroup = "G:S-1-5-80-956008885-3418522649-1831038044-1853292631-2271478464";

		public const string SystemOwner = "O:SY";

		public const string SystemGroup = "G:SY";

		public const string DaclPrefix = "D:";

		public const string SaclPrefix = "S:";

		public const string ProtectedAclFlag = "P";

		public const string AutoInheritAclFlag = "AI";

		public const string AutoInheritReqAclFlag = "AR";

		public const string SystemAdminAllAccessNoInheritanceAce = "(A;;0x111FFFFF;;;SY)(A;;0x111FFFFF;;;BA)";

		public const string ProtectedSystemAllAccessNoInheritanceAce = "P(A;;GA;;;SY)";

		public const string NoWriteUpLowLabelAce = "(ML;;NX;;;LW)";

		public const string PrivateResourceAccess = "0x111FFFFF";

		public const string PrivateResourceReadOnlyAccess = "GR";

		public const string ServiceAccessPrivateResourceAccess = "CCLCSWRPLO";

		public const string ComLaunchPrivateResourceAccess = "CCDCSW";

		public const string ComAccessPrivateResourceAccess = "CCDC";

		public const string FileOwner = "O:S-1-5-80-956008885-3418522649-1831038044-1853292631-2271478464";

		public const string FileGroup = "G:S-1-5-80-956008885-3418522649-1831038044-1853292631-2271478464";

		public const string FileDefaultDacl = "(A;;0x111FFFFF;;;SY)(A;;0x111FFFFF;;;BA)";

		public const string DirectoryInheritanceFlags = "CIOI";

		public const string DirectoryOwner = "O:S-1-5-80-956008885-3418522649-1831038044-1853292631-2271478464";

		public const string DirectoryGroup = "G:S-1-5-80-956008885-3418522649-1831038044-1853292631-2271478464";

		public const string DirectoryDefaultDacl = "(A;CIOI;0x111FFFFF;;;CO)(A;CIOI;0x111FFFFF;;;SY)(A;CIOI;0x111FFFFF;;;BA)";

		public const string RegistryInheritanceFlags = "CI";

		public const string RegistryOwner = "O:S-1-5-80-956008885-3418522649-1831038044-1853292631-2271478464";

		public const string RegistryGroup = "G:S-1-5-80-956008885-3418522649-1831038044-1853292631-2271478464";

		public const string RegistryDefaultDacl = "(A;CI;0x111FFFFF;;;CO)(A;CI;0x111FFFFF;;;SY)(A;CI;0x111FFFFF;;;BA)";

		public const string TransientObjectDefaultDacl = "(A;;0x111FFFFF;;;CO)(A;;0x111FFFFF;;;SY)(A;;0x111FFFFF;;;BA)";

		public const string ServiceAccessOwner = "O:SY";

		public const string ServiceAccessGroup = "G:SY";

		public const string ServiceAccessDefaultDacl = "(A;;GRCR;;;IU)(A;;GRCR;;;SU)(A;;0x111FFFFF;;;SY)(A;;0x111FFFFF;;;BA)";

		public const string ComOwner = "O:SY";

		public const string ComGroup = "G:SY";

		public const string ComDefaultDacl = "(A;;0x111FFFFF;;;SY)(A;;0x111FFFFF;;;BA)";

		public const string ComDefaultSacl = "(ML;;NX;;;LW)";

		public const string WinRtOwner = "O:SY";

		public const string WinRtGroup = "G:SY";

		public const string WinRtDefaultDacl = "(A;;0x111FFFFF;;;SY)(A;;0x111FFFFF;;;BA)";

		public const string WinRtDefaultSacl = "(ML;;NX;;;LW)";

		public const string EtwProviderOwner = "O:SY";

		public const string EtwProviderGroup = "G:SY";

		public const string EtwProviderDefaultDacl = "(A;;0x111FFFFF;;;SY)(A;;0x111FFFFF;;;BA)";

		public const string WnfDefaultDacl = "(A;;0x111FFFFF;;;SY)(A;;0x111FFFFF;;;BA)";

		public const string SdRegValueOwner = "O:SY";

		public const string SdRegValueGroup = "G:SY";

		public const string SdRegValueDefaultDacl = "(A;;0x111FFFFF;;;SY)(A;;0x111FFFFF;;;BA)";

		public const string DriverDefaultDacl = "P(A;;GA;;;SY)";

		public const string TransientObjectRegistryPath = "HKEY_LOCAL_MACHINE\\Software\\Microsoft\\SecurityManager\\TransientObjects\\";

		public const string TransientObjectSecurityDescriptorValueName = "SecurityDescriptor";

		public const string TransientObjectTypePrefix = "%5C%5C.%5C";

		public const string TransientObjectTypeSuffix = "%5C";

		public const string ComPermissionRegistryPath = "HKEY_LOCAL_MACHINE\\Software\\Classes\\AppId\\";

		public const string ComAccessPermissionValueName = "AccessPermission";

		public const string ComLaunchPermissionValueName = "LaunchPermission";

		public const string WinRtPermissionRegistryPath = "HKEY_LOCAL_MACHINE\\Software\\Microsoft\\WindowsRuntime\\Server\\";

		public const string WinRtPermissionValueName = "Permissions";

		public const string EtwProviderRegistryPath = "HKEY_LOCAL_MACHINE\\System\\CurrentControlSet\\Control\\WMI\\Security";
	}
}
