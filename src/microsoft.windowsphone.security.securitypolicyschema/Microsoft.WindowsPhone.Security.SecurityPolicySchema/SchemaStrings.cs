using System.Diagnostics.CodeAnalysis;

namespace Microsoft.WindowsPhone.Security.SecurityPolicySchema
{
	public static class SchemaStrings
	{
		public const string Namespace = "urn:Microsoft.WindowsPhone/PackageSchema.v8.00";

		public const string ElementRoot = "PhoneSecurityPolicy";

		public const string AttributeDescription = "Description";

		public const string AttributeVendor = "Vendor";

		public const string AttributeRequiredOSVersion = "RequiredOSVersion";

		public const string AttributeFileVersion = "FileVersion";

		public const string ElementMacros = "Macros";

		public const string ElementMacro = "Macro";

		public const string AttributeId = "Id";

		public const string AttributeValue = "Value";

		public const string ElementCapabilities = "Capabilities";

		public const string ElementCapability = "Capability";

		public const string AttributeFriendlyName = "FriendlyName";

		public const string AttributeVisibility = "Visibility";

		public const string ElementWindowsCapability = "WindowsCapability";

		public const string ElementCapabilityRules = "CapabilityRules";

		public const string ElementWindowsRules = "WindowsRules";

		public const string ElementFile = "File";

		public const string ElementDirectory = "Directory";

		public const string ElementRegKey = "RegKey";

		public const string ElementService = "Service";

		public const string ElementServiceAccess = "ServiceAccess";

		public const string ElementTransientObject = "TransientObject";

		public const string ElementPrivilege = "Privilege";

		public const string ElementDeviceSetupClass = "DeviceSetupClass";

		public const string ElementEtwProvider = "ETWProvider";

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Wnf", Justification = "Wnf is correct acronym for Windows Notification Framework")]
		public const string ElementWnf = "WNF";

		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "COM", Justification = "COM is correct acronym")]
		public const string ElementCOM = "COM";

		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "WinRT", Justification = "WinRT is correct acronym")]
		public const string ElementWinRT = "WinRT";

		public const string ElementSDRegValue = "SDRegValue";

		public const string AttributeSaveAsString = "SaveAsString";

		public const string AttributeSetOwner = "SetOwner";

		public const string ElementSecurity = "Security";

		public const string AttributeInfSectionName = "InfSectionName";

		public const string AttributeRuleTemplate = "RuleTemplate";

		public const string ElementAccessedByCapability = "AccessedByCapability";

		public const string ElementAccessedByService = "AccessedByService";

		public const string ElementAccessedByApplication = "AccessedByApplication";

		public const string AttributeRights = "Rights";

		public const string AttributePath = "Path";

		public const string AttributeSource = "Source";

		public const string AttributeDestinationDir = "DestinationDir";

		public const string AttributeName = "Name";

		public const string AttributeGuid = "Guid";

		public const string AttributeType = "Type";

		public const string AttributeAppId = "AppId";

		public const string AttributeServerName = "ServerName";

		public const string AttributeLaunchPermission = "LaunchPermission";

		public const string AttributeAccessPermission = "AccessPermission";

		public const string AttributeScope = "Scope";

		public const string AttributeTag = "Tag";

		public const string AttributeSequence = "Sequence";

		public const string AttributeDataPermanent = "DataPermanent";

		public const string AttributeReadOnly = "ReadOnly";

		public const string ElementComponents = "Components";

		public const string ElementApplication = "Application";

		public const string ElementFiles = "Files";

		public const string ElementExecutable = "Executable";

		public const string ElementAppResource = "AppResource";

		public const string ElementRequiredCapabilities = "RequiredCapabilities";

		public const string ElementRequiredCapability = "RequiredCapability";

		public const string AttributeCapId = "CapId";

		public const string ElementPrivateResources = "PrivateResources";

		public const string AttributeSvcHostGroupName = "SvcHostGroupName";

		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "TCB", Justification = "TCB is correct acronym")]
		public const string AttributeIsTCB = "IsTCB";

		public const string AttributeLogonAccount = "LogonAccount";

		public const string AttributeOEMExtensible = "OEMExtensible";

		public const string ElementServiceDll = "ServiceDll";

		public const string AttributeHostExe = "HostExe";

		public const string ElementDriverRule = "DriverRule";

		public const string ElementAuthorization = "Authorization";

		public const string ElementPrincipalClass = "PrincipalClass";

		public const string ElementExecutables = "Executables";

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

		public const string AttributeSvcOwnProcess = "Win32OwnProcess";

		public const string ElementFullTrust = "FullTrust";

		public const string AttributeSkip = "Skip";

		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "SID", Justification = "SID is correct acronym")]
		public const string AttributeSID = "SID";
	}
}
