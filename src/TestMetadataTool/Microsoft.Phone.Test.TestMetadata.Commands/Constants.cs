using System.Collections.Generic;

namespace Microsoft.Phone.Test.TestMetadata.Commands
{
	internal static class Constants
	{
		public const string PackageManfiestFileExtension = "man.dsm.xml";

		public const string PackageMetadataExtension = ".spkg.meta.xml";

		public const string MetadataExtension = ".meta.xml";

		public const string MetaExtension = ".meta";

		public const string DepXmlFileExtension = ".dep.xml";

		public const string GuestCabUniqueExtension = ".guest";

		public const string SpkgFileExtension = ".spkg";

		public const string CabFileExtension = ".cab";

		public const string GuestCabFileExtension = ".guest.cab";

		public const string DllExtension = ".dll";

		public const string ExeExtension = ".exe";

		public const string DriverExtension = ".sys";

		public const string ConfigExtension = ".config";

		public const string MumFileName = "update.mum";

		public const string WowFolderName = "wow";

		public const string BinaryDirectory = "bin";

		public const string MetadataDirectory = "metadata";

		public static readonly List<string> ArchitecturesOf64Bit = new List<string> { "arm64", "amd64" };
	}
}
