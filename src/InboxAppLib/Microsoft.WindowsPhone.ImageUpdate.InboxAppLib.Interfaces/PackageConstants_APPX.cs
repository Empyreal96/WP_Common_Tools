using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib.Interfaces
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct PackageConstants_APPX
	{
		public const string AppxExtension = ".appx";

		public const string AppxBundleExtension = ".appxbundle";

		public const string AppxMetadataSubDir = "AppxMetadata";

		public const string InRomInstallBaseDestinationPath = "$(runtime.windows)\\InfusedApps";

		public const string InRomInstallApplicationsDestinationPath = "Applications";

		public const string InRomInstallFrameworksDestinationPath = "Frameworks";

		public const string InRomInstallPackagesDestinationPath = "Packages";

		public const string DataPartitionInstallBaseDestinationPath = "$(runtime.data)\\Programs\\WindowsApps";

		public const string AppxFrameworkMacroHeader = "appxframework.";

		public const string AppxFrameworkRegEx_VersionQuad = "(0|[1-9][0-9]{0,3}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])(\\.(0|[1-9][0-9]{0,3}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])){3}";
	}
}
