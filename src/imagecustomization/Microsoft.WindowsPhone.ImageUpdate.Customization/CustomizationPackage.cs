using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;

namespace Microsoft.WindowsPhone.ImageUpdate.Customization
{
	internal class CustomizationPackage
	{
		public static readonly XNamespace PackageNamespace = "urn:Microsoft.WindowsPhone/PackageSchema.v8.00";

		public const string StaticApplicationsName = "StaticApps";

		public const string VariantApplicationsName = "VariantApps";

		public const string ShadowRegFileName = "OEMSettings.reg";

		public static readonly string ShadowRegFilePath = PkgConstants.c_strRguDeviceFolder + "\\OEMSettings.reg";

		private const string PkgGen = "PkgGen.exe";

		private const string PkgGenArguments = "\"{0}\" /output:\"{1}\" /build:{2} /cpu:{3} /version:{4}";

		private const string PackageXmlExtension = ".pkg.xml";

		public string Owner { get; set; }

		public string Component { get; set; }

		public string SubComponent { get; set; }

		public OwnerType OwnerType { get; set; }

		public ReleaseType ReleaseType { get; set; }

		public string Partition { get; set; }

		public CpuId CpuType { get; set; }

		public BuildType BuildType { get; set; }

		public VersionInfo Version { get; set; }

		public List<CustomizationFile> Files { get; private set; }

		public CustomizationPackage()
			: this(PkgConstants.c_strMainOsPartition)
		{
		}

		public CustomizationPackage(string partition)
		{
			Files = new List<CustomizationFile>();
			Component = "Device";
			SubComponent = "Customizations";
			OwnerType = OwnerType.OEM;
			BuildType = BuildType.Retail;
			ReleaseType = ReleaseType.Production;
			Partition = partition;
		}

		public void AddFile(string sourcePath, string destinationPath)
		{
			AddFile(FileType.Regular, sourcePath, destinationPath);
		}

		public void AddFile(FileType type, string sourcePath, string destinationPath)
		{
			Files.Add(new CustomizationFile(type, sourcePath, destinationPath));
		}

		public void AddFile(CustomizationFile customizationFile)
		{
			AddFile(customizationFile.Source, customizationFile.Destination);
		}

		public void AddFiles(IEnumerable<CustomizationFile> customizationFiles)
		{
			foreach (CustomizationFile customizationFile in customizationFiles)
			{
				AddFile(customizationFile);
			}
		}

		public string SavePackage(string folderPath)
		{
			using (IPkgBuilder pkgBuilder = Package.Create())
			{
				pkgBuilder.Owner = Owner;
				pkgBuilder.OwnerType = OwnerType;
				pkgBuilder.Component = Component;
				pkgBuilder.SubComponent = SubComponent + "." + Partition;
				pkgBuilder.Partition = Partition;
				pkgBuilder.ReleaseType = ReleaseType;
				pkgBuilder.CpuType = CpuType;
				pkgBuilder.Version = Version;
				pkgBuilder.BuildType = BuildType;
				foreach (CustomizationFile file in Files)
				{
					pkgBuilder.AddFile(file.FileType, file.Source, file.Destination, PkgConstants.c_defaultAttributes, null);
				}
				string text = Path.Combine(Path.GetFullPath(folderPath), pkgBuilder.Name + PkgConstants.c_strPackageExtension);
				pkgBuilder.SaveCab(text);
				return text;
			}
		}
	}
}
