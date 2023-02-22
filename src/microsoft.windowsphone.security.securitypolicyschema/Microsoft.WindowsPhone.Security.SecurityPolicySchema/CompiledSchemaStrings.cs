using System.Diagnostics.CodeAnalysis;

namespace Microsoft.WindowsPhone.Security.SecurityPolicySchema
{
	public static class CompiledSchemaStrings
	{
		public const string Namespace = "urn:Microsoft.WindowsPhone/PhoneSecurityPolicyInternal.v8.00";

		public const string ElementRoot = "PhoneSecurityPolicy";

		public const string AttributeDescription = "Description";

		public const string AttributeVendor = "Vendor";

		public const string AttributeRequiredOSVersion = "RequiredOSVersion";

		public const string AttributeFileVersion = "FileVersion";

		public const string AttributePackageId = "PackageID";

		public const string AttributeHashType = "HashType";

		public const string ElementRules = "Rules";

		public const string ElementRule = "Rule";

		public const string ElementSDRegValue = "SDRegValue";

		public const string AttributeSaveAsString = "SaveAsString";

		public const string AttributeProtected = "Protected";

		public const string AttributeOwner = "Owner";

		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "DACL", Justification = "DACL is correct acronym")]
		public const string AttributeDACL = "DACL";

		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "SACL", Justification = "SACL is correct acronym")]
		public const string AttributeSACL = "SACL";

		public const string AttributeAttributeHash = "AttributeHash";

		public const string AttributeElementId = "ElementID";

		public const string ElementCapabilities = "Capabilities";

		public const string ElementCapability = "Capability";

		public const string AttributeFriendlyName = "FriendlyName";

		public const string AttributeVisibility = "Visibility";

		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "SID", Justification = "SID is correct acronym")]
		public const string AttributeAppCapSID = "AppCapSID";

		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "SID", Justification = "SID is correct acronym")]
		public const string AttributeSvcCapSID = "SvcCapSID";

		public const string ElementWindowsCapability = "WindowsCapability";

		public const string ElementCapabilityRules = "CapabilityRules";

		public const string ElementFile = "File";

		public const string ElementDirectory = "Directory";

		public const string ElementRegKey = "RegKey";

		public const string ElementTransientObject = "TransientObject";

		public const string ElementPrivilege = "Privilege";

		public const string ElementDeviceSetupClass = "DeviceSetupClass";

		public const string ElementServiceAccess = "ServiceAccess";

		public const string ElementEtwProvider = "ETWProvider";

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Wnf", Justification = "Wnf is correct acronym for Windows Notification Framework")]
		public const string ElementWnf = "WNF";

		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "COM", Justification = "COM is correct acronym")]
		public const string ElementCOM = "COM";

		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "WinRT", Justification = "WinRT is correct acronym")]
		public const string ElementWinRT = "WinRT";

		public const string AttributeRights = "Rights";

		public const string AttributeFlags = "Flags";

		public const string AttributePath = "Path";

		public const string AttributeName = "Name";

		public const string AttributeType = "Type";

		public const string AttributeGuid = "Guid";

		public const string AttributeAppId = "AppId";

		public const string AttributeServerName = "ServerName";

		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "SID", Justification = "SID is correct acronym")]
		public const string AttributeSID = "SID";

		public const string AttributeId = "Id";

		public const string AttributeLaunchPermission = "LaunchPermission";

		public const string AttributeAccessPermission = "AccessPermission";

		public const string ElementComponents = "Components";

		public const string ElementApplication = "Application";

		public const string ElementBinaries = "Binaries";

		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "SID", Justification = "SID is correct acronym")]
		public const string AttributePrivateCapSID = "PrivateCapSID";

		public const string ElementAppBinaries = "AppBinaries";

		public const string AttributeAppName = "AppName";

		public const string ElementBinary = "Binary";

		public const string AttributeBinaryId = "BinaryId";

		public const string ElementRequiredCapabilities = "RequiredCapabilities";

		public const string ElementRequiredCapability = "RequiredCapability";

		public const string AttributeCapId = "CapId";

		public const string ElementPrivateResources = "PrivateResources";

		public const string ElementService = "Service";

		public const string AttributeExecutable = "Executable";

		public const string AttributeSvcHostGroupName = "SvcHostGroupName";

		public const string AttributeSvcProcessOwnership = "OwnedProc";

		public const string AttributeSvcProcessOwnershipIsOwned = "Y";

		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "TCB", Justification = "TCB is correct acronym")]
		public const string AttributeIsTCB = "IsTCB";

		public const string AttributeLogonAccount = "LogonAccount";

		public const string AttributeOEMExtensible = "OEMExtensible";

		public const string ElementAuthorization = "Authorization";

		public const string ElementPrincipalClass = "PrincipalClass";

		public const string ElementExecutables = "Executables";

		public const string ElementExecutable = "Executable";

		public const string ElementDirectories = "Directories";

		public const string ElementCertificates = "Certificates";

		public const string ElementCertificate = "Certificate";

		public const string ElementChambers = "Chambers";

		public const string ElementChamber = "Chamber";

		public const string ElementCapabilityClass = "CapabilityClass";

		public const string ElementMemberCapability = "MemberCapability";

		public const string ElementMemberCapabilityClass = "MemberCapabilityClass";

		public const string ElementExecuteRule = "ExecuteRule";

		public const string ElementCapabilityRule = "CapabilityRule";

		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "EKU", Justification = "EKU is correct acronym")]
		public const string AttributeEKU = "EKU";

		public const string AttributeThumbprint = "Thumbprint";

		public const string AttributeThumbprintAlgorithm = "Alg";

		public const string AttributePrincipalClass = "PrincipalClass";

		public const string AttributeTargetChamber = "TargetChamber";

		public const string AttributeCapabilityClass = "CapabilityClass";

		public const string ElementFullTrust = "FullTrust";

		public const string AttributeSkip = "Skip";
	}
}
